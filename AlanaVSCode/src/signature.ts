import * as vscode from "vscode";
import { BUILTIN_SIGNATURES } from "./catalog";
import { formatSignature } from "./analyze";
import { AlanaIndex } from "./index";
import { Token } from "./lexer";

function tokenIndexAt(tokens: Token[], position: vscode.Position): number {
  const line = position.line + 1;
  const col = position.character + 1;
  let best = -1;
  for (let i = 0; i < tokens.length; i++) {
    const t = tokens[i];
    if (t.line < line || (t.line === line && t.col <= col)) {
      best = i;
    }
  }
  return best;
}

function activeCall(tokens: Token[], index: number): { name: string; arg: number } | undefined {
  let depth = 0;
  let arg = 0;
  for (let i = index; i >= 0; i--) {
    const t = tokens[i];
    if (t.kind === "CLOSE") {
      depth++;
    } else if (t.kind === "OPEN") {
      if (depth === 0) {
        const nameTok = tokens[i - 1];
        if (!nameTok) {
          return undefined;
        }
        if (nameTok.kind === "VAR" || nameTok.kind === "CONSTRUCTOR" || nameTok.text.startsWith("__")) {
          return { name: nameTok.value ?? nameTok.text, arg };
        }
        if (
          nameTok.kind === "PARSENUM" ||
          nameTok.kind === "PARSESTR" ||
          nameTok.kind === "PARSEBOOL" ||
          nameTok.kind === "PARSEDEC" ||
          nameTok.kind === "PARSECHAR" ||
          nameTok.kind === "TRYPARSENUM" ||
          nameTok.kind === "TRYPARSEDEC" ||
          nameTok.kind === "TRYPARSEBOOL" ||
          nameTok.kind === "TRYPARSECHAR"
        ) {
          return { name: nameTok.text, arg };
        }
        if (nameTok.kind === "NEWOBJ") {
          return { name: "newobj", arg };
        }
        return undefined;
      }
      depth--;
    } else if (t.kind === "COMA" && depth === 0) {
      arg++;
    }
  }
  return undefined;
}

function infoFromLabel(label: string, documentation: string, parameters: { label: string; documentation: string }[]): vscode.SignatureInformation {
  const info = new vscode.SignatureInformation(label, documentation);
  info.parameters = parameters.map((p) => new vscode.ParameterInformation(p.label, p.documentation));
  return info;
}

export class AlanaSignatureProvider implements vscode.SignatureHelpProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideSignatureHelp(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.SignatureHelp | undefined {
    const analysis = this.index.getDocument(document);
    const idx = tokenIndexAt(analysis.tokens, position);
    const call = activeCall(analysis.tokens, idx);
    if (!call) {
      return undefined;
    }

    const help = new vscode.SignatureHelp();
    help.activeParameter = call.arg;

    const builtin = BUILTIN_SIGNATURES[call.name];
    if (builtin) {
      help.signatures = builtin.map((s) => infoFromLabel(s.label, s.documentation, s.parameters));
      help.activeSignature = builtin.length > 1 && call.arg >= 2 ? 1 : 0;
      return help;
    }

    if (call.name === "newobj") {
      const tokens = analysis.tokens;
      const openAt = (() => {
        let depth = 0;
        for (let i = idx; i >= 0; i--) {
          if (tokens[i].kind === "CLOSE") {
            depth++;
          } else if (tokens[i].kind === "OPEN") {
            if (depth === 0) {
              return i;
            }
            depth--;
          }
        }
        return -1;
      })();
      const classTok = openAt > 0 ? tokens[openAt - 1] : undefined;
      const className = classTok?.kind === "VAR" ? classTok.value ?? classTok.text : undefined;
      if (className) {
        const ctor = this.index.classMembers(document, className).find((s) => s.kind === "constructor");
        if (ctor) {
          help.signatures = [
            infoFromLabel(
              `newobj ${className}(${(ctor.params ?? []).map((p) => `${p.type} ${p.name}`).join(", ")})`,
              formatSignature(ctor),
              (ctor.params ?? []).map((p) => ({ label: `${p.type} ${p.name}`, documentation: p.type }))
            ),
          ];
          return help;
        }
      }
    }

    const fn = this.index.lookupFunction(document, call.name);
    if (fn) {
      const params = fn.symbol.params ?? [];
      help.signatures = [
        infoFromLabel(
          formatSignature(fn.symbol),
          formatSignature(fn.symbol),
          params.map((p) => ({ label: `${p.type} ${p.name}`, documentation: p.type }))
        ),
      ];
      return help;
    }

    const method = analysis.symbols.find((s) => s.kind === "method" && s.name === call.name);
    if (method) {
      const params = method.params ?? [];
      help.signatures = [
        infoFromLabel(
          formatSignature(method),
          formatSignature(method),
          params.map((p) => ({ label: `${p.type} ${p.name}`, documentation: p.type }))
        ),
      ];
      return help;
    }

    return undefined;
  }
}
