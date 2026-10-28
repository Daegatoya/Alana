import * as vscode from "vscode";
import { NATIVES } from "./catalog";
import { AlanaIndex } from "./index";
import { tokenEndCol } from "./lexer";

export function refreshDiagnostics(
  collection: vscode.DiagnosticCollection,
  document: vscode.TextDocument,
  index: AlanaIndex
): void {
  if (document.languageId !== "alana") {
    return;
  }

  const enabled = vscode.workspace.getConfiguration("alana").get<boolean>("diagnostics.enabled", true);
  if (!enabled) {
    collection.delete(document.uri);
    return;
  }

  const analysis = index.getDocument(document);
  const diagnostics: vscode.Diagnostic[] = [];

  for (const d of analysis.diagnostics) {
    const line = Math.max(0, d.line - 1);
    const col = Math.max(0, d.col - 1);
    const range = new vscode.Range(line, col, line, col + Math.max(1, d.length));
    const item = new vscode.Diagnostic(
      range,
      d.message,
      d.severity === "warning" ? vscode.DiagnosticSeverity.Warning : vscode.DiagnosticSeverity.Error
    );
    item.source = "alana";
    diagnostics.push(item);
  }

  for (const imp of analysis.imports) {
    if (!index.resolveImport(document.uri, imp.path)) {
      const line = Math.max(0, imp.line - 1);
      const col = Math.max(0, imp.col - 1);
      const item = new vscode.Diagnostic(
        new vscode.Range(line, col, line, col + Math.max(1, imp.length)),
        `Imported file not found: ${imp.path}`,
        vscode.DiagnosticSeverity.Error
      );
      item.source = "alana";
      diagnostics.push(item);
    }
  }

  const imported = index.importedPublicSymbols(document.uri, analysis);
  const knownFunctions = new Set<string>(NATIVES);
  for (const s of analysis.symbols) {
    if (s.kind === "function") {
      knownFunctions.add(s.name);
    }
  }
  for (const s of imported) {
    if (s.kind === "function") {
      knownFunctions.add(s.name);
    }
  }

  const tokens = analysis.tokens;
  for (let i = 0; i < tokens.length; i++) {
    const tok = tokens[i];
    if (tok.kind !== "CALL") {
      continue;
    }
    const nameTok = tokens[i + 1];
    if (!nameTok || nameTok.kind !== "VAR") {
      continue;
    }
    const next = tokens[i + 2];
    if (!next || next.kind !== "OPEN") {
      continue;
    }
    const name = nameTok.value ?? nameTok.text;
    if (!knownFunctions.has(name)) {
      const item = new vscode.Diagnostic(
        new vscode.Range(nameTok.line - 1, nameTok.col - 1, nameTok.line - 1, tokenEndCol(nameTok) - 1),
        `Unknown function ${name}`,
        vscode.DiagnosticSeverity.Warning
      );
      item.source = "alana";
      diagnostics.push(item);
    }
  }

  collection.set(document.uri, diagnostics);
}
