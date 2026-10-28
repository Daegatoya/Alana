import * as vscode from "vscode";
import { KEYWORDS, NATIVE_DOCS, NATIVES } from "./catalog";
import { formatSignature, identifierAt } from "./analyze";
import { AlanaIndex } from "./index";

export class AlanaHoverProvider implements vscode.HoverProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideHover(document: vscode.TextDocument, position: vscode.Position): vscode.Hover | undefined {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    if (!token) {
      const word = document.getWordRangeAtPosition(position);
      if (!word) {
        return undefined;
      }
      const text = document.getText(word);
      return this.hoverForName(document, text);
    }

    const text = token.value ?? token.text;
    return this.hoverForName(document, text);
  }

  private hoverForName(document: vscode.TextDocument, name: string): vscode.Hover | undefined {
    if (NATIVE_DOCS[name]) {
      return md(name, NATIVE_DOCS[name], "native");
    }
    if (KEYWORDS[name]) {
      return md(name, KEYWORDS[name]);
    }

    const analysis = this.index.getDocument(document);
    const local = [...analysis.symbols].reverse().find((s) => s.name === name);
    if (local) {
      return md(name, formatSignature(local), local.kind);
    }

    const fn = this.index.lookupFunction(document, name);
    if (fn) {
      return md(name, formatSignature(fn.symbol), "function");
    }

    const cls = this.index.lookupClass(document, name);
    if (cls) {
      return md(name, `define class ${cls.symbol.name}`, "class");
    }

    if (NATIVES.includes(name)) {
      return md(name, NATIVE_DOCS[name] ?? "Native function.", "native");
    }

    return undefined;
  }
}

function md(title: string, body: string, kind?: string): vscode.Hover {
  const markdown = new vscode.MarkdownString();
  markdown.appendCodeblock(kind ? `${kind} ${title}` : title, "alana");
  markdown.appendMarkdown(body);
  return new vscode.Hover(markdown);
}
