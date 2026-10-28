import * as fs from "fs";
import * as path from "path";
import * as vscode from "vscode";
import {
  BUILTIN_SIGNATURES,
  KEYWORDS,
  NATIVES,
  NATIVE_DOCS,
  OPERATORS,
  TYPES,
} from "./catalog";
import { classOfVariable, enclosingClass, formatSignature, SymbolInfo } from "./analyze";
import { AlanaIndex } from "./index";
import { Token } from "./lexer";

function lastNonEnd(tokens: Token[], index: number): Token | undefined {
  for (let i = index; i >= 0; i--) {
    if (tokens[i].kind !== "END") {
      return tokens[i];
    }
  }
  return undefined;
}

function tokenIndexAt(tokens: Token[], position: vscode.Position): number {
  const line = position.line + 1;
  const col = position.character + 1;
  let best = -1;
  for (let i = 0; i < tokens.length; i++) {
    const t = tokens[i];
    if (t.line < line || (t.line === line && t.col + t.text.length <= col)) {
      best = i;
    } else if (t.line > line || (t.line === line && t.col > col)) {
      break;
    }
  }
  return best;
}

function snippetForSymbol(symbol: SymbolInfo): vscode.SnippetString {
  if (symbol.kind === "function" || symbol.kind === "method") {
    const params = symbol.params ?? [];
    const inner = params.map((p, i) => `\${${i + 1}:${p.name}}`).join(", ");
    return new vscode.SnippetString(`${symbol.name}(${inner})`);
  }
  if (symbol.kind === "constructor") {
    const params = symbol.params ?? [];
    const inner = params.map((p, i) => `\${${i + 1}:${p.name}}`).join(", ");
    return new vscode.SnippetString(`${inner}`);
  }
  return new vscode.SnippetString(symbol.name);
}

function item(
  label: string,
  kind: vscode.CompletionItemKind,
  detail?: string,
  insert?: string | vscode.SnippetString,
  doc?: string
): vscode.CompletionItem {
  const completion = new vscode.CompletionItem(label, kind);
  completion.detail = detail;
  if (insert) {
    completion.insertText = insert;
  }
  if (doc) {
    completion.documentation = new vscode.MarkdownString(doc);
  }
  return completion;
}

export class AlanaCompletionProvider implements vscode.CompletionItemProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideCompletionItems(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.CompletionItem[] {
    const analysis = this.index.getDocument(document);
    const tokens = analysis.tokens;
    const idx = tokenIndexAt(tokens, position);
    const prev = lastNonEnd(tokens, idx);
    const linePrefix = document.lineAt(position).text.substring(0, position.character);
    const results: vscode.CompletionItem[] = [];

    if (/import\s+"[^"]*$/.test(linePrefix)) {
      return this.fileCompletions(document);
    }

    if (prev?.kind === "DOT" || /\.\s*$/.test(linePrefix) || /self\.\s*$/.test(linePrefix)) {
      return this.memberCompletions(document, tokens, idx);
    }

    if (prev?.kind === "NEWOBJ") {
      return this.classCompletions(document);
    }

    if (prev?.kind === "RETURN_TYPE") {
      return [...this.typeCompletions(), ...this.classCompletions(document)];
    }

    if (prev?.kind === "DEFINE") {
      results.push(item("func", vscode.CompletionItemKind.Keyword, "define func"));
      results.push(item("class", vscode.CompletionItemKind.Keyword, "define class"));
      if (enclosingClass(analysis, position.line + 1)) {
        results.push(item("constructor", vscode.CompletionItemKind.Keyword, "define constructor"));
      }
      results.push(...this.typeCompletions());
      results.push(...this.classCompletions(document));
      return results;
    }

    if (prev?.kind === "CALL") {
      return this.callCompletions(document);
    }

    if (prev?.kind === "EQUAL") {
      results.push(item("new", vscode.CompletionItemKind.Keyword, "Uninitialized value"));
      results.push(item("newobj", vscode.CompletionItemKind.Keyword, "Create a class instance"));
      results.push(item("true", vscode.CompletionItemKind.Constant));
      results.push(item("false", vscode.CompletionItemKind.Constant));
      results.push(item("readln", vscode.CompletionItemKind.Function, "Read a line"));
      results.push(item("readk", vscode.CompletionItemKind.Function, "Read a key"));
      results.push(...this.callCompletions(document));
      results.push(...this.variableCompletions(document, position));
      return results;
    }

    results.push(...this.typeCompletions());
    results.push(...this.keywordCompletions());
    results.push(...this.variableCompletions(document, position));
    results.push(...this.callCompletions(document));
    results.push(...this.classCompletions(document));
    results.push(item("true", vscode.CompletionItemKind.Constant, "boolean"));
    results.push(item("false", vscode.CompletionItemKind.Constant, "boolean"));
    results.push(item("self", vscode.CompletionItemKind.Keyword, "Current instance"));

    if (/^\s*pub/.test(linePrefix)) {
      results.push(item("public:", vscode.CompletionItemKind.Keyword, "Public visibility section"));
    }
    if (/^\s*pri/.test(linePrefix)) {
      results.push(item("private:", vscode.CompletionItemKind.Keyword, "Private visibility section"));
    }

    return results;
  }

  private typeCompletions(): vscode.CompletionItem[] {
    return TYPES.map((t) => item(t, vscode.CompletionItemKind.TypeParameter, "type", t, KEYWORDS[t]));
  }

  private keywordCompletions(): vscode.CompletionItem[] {
    const items: vscode.CompletionItem[] = [];
    for (const [name, doc] of Object.entries(KEYWORDS)) {
      if ((TYPES as readonly string[]).includes(name)) {
        continue;
      }
      const kind = OPERATORS.includes(name)
        ? vscode.CompletionItemKind.Operator
        : vscode.CompletionItemKind.Keyword;
      items.push(item(name, kind, undefined, name, doc));
    }
    return items;
  }

  private classCompletions(document: vscode.TextDocument): vscode.CompletionItem[] {
    const analysis = this.index.getDocument(document);
    const names = new Map<string, SymbolInfo>();
    for (const s of analysis.symbols) {
      if (s.kind === "class") {
        names.set(s.name, s);
      }
    }
    for (const s of this.index.importedPublicSymbols(document.uri, analysis)) {
      if (s.kind === "class") {
        names.set(s.name, s);
      }
    }
    return [...names.values()].map((s) =>
      item(s.name, vscode.CompletionItemKind.Class, "class", s.name, formatSignature(s))
    );
  }

  private callCompletions(document: vscode.TextDocument): vscode.CompletionItem[] {
    const analysis = this.index.getDocument(document);
    const items: vscode.CompletionItem[] = [];
    const seen = new Set<string>();

    const addFn = (s: SymbolInfo, kind: vscode.CompletionItemKind): void => {
      if (seen.has(s.name)) {
        return;
      }
      seen.add(s.name);
      const completion = item(s.name, kind, formatSignature(s), snippetForSymbol(s), formatSignature(s));
      items.push(completion);
    };

    for (const s of analysis.symbols) {
      if (s.kind === "function") {
        addFn(s, vscode.CompletionItemKind.Function);
      }
    }
    for (const s of this.index.importedPublicSymbols(document.uri, analysis)) {
      if (s.kind === "function") {
        addFn(s, vscode.CompletionItemKind.Function);
      }
    }
    for (const name of NATIVES) {
      items.push(
        item(
          name,
          vscode.CompletionItemKind.Function,
          "native",
          new vscode.SnippetString(`${name}($1)`),
          NATIVE_DOCS[name]
        )
      );
    }
    for (const name of Object.keys(BUILTIN_SIGNATURES)) {
      if (name.startsWith("__")) {
        continue;
      }
      const sig = BUILTIN_SIGNATURES[name][0];
      items.push(item(name, vscode.CompletionItemKind.Function, sig.label, name, sig.documentation));
    }
    return items;
  }

  private variableCompletions(document: vscode.TextDocument, position: vscode.Position): vscode.CompletionItem[] {
    const analysis = this.index.getDocument(document);
    const items: vscode.CompletionItem[] = [];
    const seen = new Set<string>();
    for (const s of analysis.symbols) {
      if (s.kind !== "variable" && s.kind !== "field" && s.kind !== "parameter") {
        continue;
      }
      if (s.line - 1 > position.line) {
        continue;
      }
      if (seen.has(s.name)) {
        continue;
      }
      seen.add(s.name);
      items.push(item(s.name, vscode.CompletionItemKind.Variable, s.type, s.name, formatSignature(s)));
    }
    return items;
  }

  private memberCompletions(
    document: vscode.TextDocument,
    tokens: Token[],
    idx: number
  ): vscode.CompletionItem[] {
    let name: string | undefined;
    for (let i = idx; i >= 0; i--) {
      if (tokens[i].kind === "SELF") {
        const analysis = this.index.getDocument(document);
        const className = enclosingClass(analysis, tokens[idx]?.line ?? 1);
        if (!className) {
          return [];
        }
        return this.index.classMembers(document, className).map((s) =>
          item(
            s.name,
            s.kind === "method" ? vscode.CompletionItemKind.Method : vscode.CompletionItemKind.Field,
            formatSignature(s),
            s.kind === "method" ? snippetForSymbol(s) : s.name,
            formatSignature(s)
          )
        );
      }
      if (tokens[i].kind === "VAR") {
        name = tokens[i].value ?? tokens[i].text;
        break;
      }
    }
    if (!name) {
      return [];
    }
    const analysis = this.index.getDocument(document);
    const className = classOfVariable(analysis, name);
    if (!className) {
      return [];
    }
    return this.index.classMembers(document, className).map((s) =>
      item(
        s.name,
        s.kind === "method" ? vscode.CompletionItemKind.Method : vscode.CompletionItemKind.Field,
        formatSignature(s),
        s.kind === "method" ? snippetForSymbol(s) : s.name,
        formatSignature(s)
      )
    );
  }

  private fileCompletions(document: vscode.TextDocument): vscode.CompletionItem[] {
    const items: vscode.CompletionItem[] = [];
    const dir = vscode.Uri.joinPath(document.uri, "..");
    try {
      const entries = fs.readdirSync(dir.fsPath);
      for (const name of entries) {
        if (name.endsWith(".alana") && name !== path.basename(document.uri.fsPath)) {
          items.push(item(name, vscode.CompletionItemKind.File, "import"));
        }
      }
    } catch {
      return [];
    }
    return items;
  }
}
