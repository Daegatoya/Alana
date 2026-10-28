import * as vscode from "vscode";
import { AlanaCompletionProvider } from "./completion";
import { refreshDiagnostics } from "./diagnostics";
import { AlanaHoverProvider } from "./hover";
import { AlanaIndex } from "./index";
import {
  AlanaDefinitionProvider,
  AlanaHighlightProvider,
  AlanaReferenceProvider,
  AlanaRenameProvider,
} from "./navigation";
import { AlanaSignatureProvider } from "./signature";
import { AlanaDocumentSymbolProvider, AlanaFoldingProvider, AlanaWorkspaceSymbolProvider } from "./symbols";

const ALANA = { language: "alana" };

export function activate(context: vscode.ExtensionContext): void {
  const index = new AlanaIndex();
  const diagnostics = vscode.languages.createDiagnosticCollection("alana");
  context.subscriptions.push(diagnostics);

  const refresh = (document: vscode.TextDocument): void => {
    index.invalidate(document);
    refreshDiagnostics(diagnostics, document, index);
  };

  context.subscriptions.push(
    vscode.workspace.onDidOpenTextDocument(refresh),
    vscode.workspace.onDidChangeTextDocument((e) => refresh(e.document)),
    vscode.workspace.onDidCloseTextDocument((document) => diagnostics.delete(document.uri)),
    vscode.workspace.onDidChangeConfiguration((e) => {
      if (e.affectsConfiguration("alana")) {
        vscode.workspace.textDocuments.forEach(refresh);
      }
    })
  );

  vscode.workspace.textDocuments.forEach(refresh);

  context.subscriptions.push(
    vscode.languages.registerCompletionItemProvider(ALANA, new AlanaCompletionProvider(index), ".", "\""),
    vscode.languages.registerHoverProvider(ALANA, new AlanaHoverProvider(index)),
    vscode.languages.registerSignatureHelpProvider(ALANA, new AlanaSignatureProvider(index), "(", ","),
    vscode.languages.registerDefinitionProvider(ALANA, new AlanaDefinitionProvider(index)),
    vscode.languages.registerReferenceProvider(ALANA, new AlanaReferenceProvider(index)),
    vscode.languages.registerRenameProvider(ALANA, new AlanaRenameProvider(index)),
    vscode.languages.registerDocumentHighlightProvider(ALANA, new AlanaHighlightProvider(index)),
    vscode.languages.registerDocumentSymbolProvider(ALANA, new AlanaDocumentSymbolProvider(index)),
    vscode.languages.registerWorkspaceSymbolProvider(new AlanaWorkspaceSymbolProvider(index)),
    vscode.languages.registerFoldingRangeProvider(ALANA, new AlanaFoldingProvider(index)),
    vscode.commands.registerCommand("alana.runFile", () => runCurrentFile())
  );
}

export function deactivate(): void {}

async function runCurrentFile(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor || editor.document.languageId !== "alana") {
    vscode.window.showInformationMessage("Open an Alana file to run it.");
    return;
  }

  if (editor.document.isUntitled) {
    vscode.window.showInformationMessage("Save the Alana file before running it.");
    return;
  }

  await editor.document.save();
  const file = editor.document.uri.fsPath;
  const terminal = vscode.window.terminals.find((t) => t.name === "Alana") ?? vscode.window.createTerminal("Alana");
  terminal.show();
  const quoted = file.includes(" ") ? `"${file}"` : file;
  terminal.sendText(`alana ${quoted}`);
}
