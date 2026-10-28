import * as vscode from "vscode";
import { SymbolInfo } from "./analyze";
import { AlanaIndex } from "./index";

function kindOf(symbol: SymbolInfo): vscode.SymbolKind {
  switch (symbol.kind) {
    case "class":
      return vscode.SymbolKind.Class;
    case "function":
      return vscode.SymbolKind.Function;
    case "method":
      return vscode.SymbolKind.Method;
    case "constructor":
      return vscode.SymbolKind.Constructor;
    case "field":
      return vscode.SymbolKind.Field;
    case "parameter":
      return vscode.SymbolKind.Variable;
    default:
      return vscode.SymbolKind.Variable;
  }
}

function range(symbol: SymbolInfo, endLine?: number, endCol?: number): vscode.Range {
  const selection = new vscode.Range(
    symbol.line - 1,
    symbol.col - 1,
    symbol.line - 1,
    symbol.endCol - 1
  );
  const full = new vscode.Range(
    symbol.line - 1,
    Math.max(0, symbol.col - 1),
    (endLine ?? symbol.endLine) - 1,
    (endCol ?? symbol.endCol) - 1
  );
  return full.union(selection);
}

export class AlanaDocumentSymbolProvider implements vscode.DocumentSymbolProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideDocumentSymbols(document: vscode.TextDocument): vscode.DocumentSymbol[] {
    const analysis = this.index.getDocument(document);
    const roots: vscode.DocumentSymbol[] = [];
    const classes = new Map<string, vscode.DocumentSymbol>();

    for (const symbol of analysis.symbols) {
      if (symbol.kind === "parameter") {
        continue;
      }

      const block = analysis.blocks.find(
        (b) =>
          (symbol.kind === "class" && b.kind === "class" && b.name === symbol.name) ||
          (symbol.kind === "function" && b.kind === "func" && b.name === symbol.name && !symbol.container) ||
          (symbol.kind === "method" && b.kind === "func" && b.name === symbol.name && b.startLine >= symbol.line) ||
          (symbol.kind === "constructor" && b.kind === "constructor" && symbol.container && b.startLine >= symbol.line)
      );

      const fullRange = range(symbol, block?.endLine, block?.endCol);
      const selection = new vscode.Range(
        symbol.line - 1,
        symbol.col - 1,
        symbol.line - 1,
        symbol.endCol - 1
      );
      const node = new vscode.DocumentSymbol(
        symbol.name,
        symbol.returnType ?? symbol.type ?? symbol.kind,
        kindOf(symbol),
        fullRange,
        selection
      );

      if (symbol.kind === "class") {
        classes.set(symbol.name, node);
        roots.push(node);
        continue;
      }

      if (symbol.container && classes.has(symbol.container.split(".")[0]) && (symbol.kind === "method" || symbol.kind === "field" || symbol.kind === "constructor")) {
        const cls = classes.get(symbol.container.split(".")[0]);
        cls?.children.push(node);
        continue;
      }

      if (symbol.kind === "function" || (symbol.kind === "variable" && !symbol.container)) {
        roots.push(node);
      }
    }

    return roots;
  }
}

export class AlanaWorkspaceSymbolProvider implements vscode.WorkspaceSymbolProvider {
  constructor(private readonly index: AlanaIndex) {}

  async provideWorkspaceSymbols(query: string): Promise<vscode.SymbolInformation[]> {
    const files = await vscode.workspace.findFiles("**/*.alana", "**/node_modules/**");
    const results: vscode.SymbolInformation[] = [];
    const q = query.toLowerCase();

    for (const uri of files) {
      const analysis = this.index.getFile(uri);
      if (!analysis) {
        continue;
      }
      for (const symbol of analysis.symbols) {
        if (symbol.kind === "parameter") {
          continue;
        }
        if (q && !symbol.name.toLowerCase().includes(q)) {
          continue;
        }
        results.push(
          new vscode.SymbolInformation(
            symbol.name,
            kindOf(symbol),
            symbol.container ?? "",
            new vscode.Location(
              uri,
              new vscode.Range(symbol.line - 1, symbol.col - 1, symbol.line - 1, symbol.endCol - 1)
            )
          )
        );
      }
    }

    return results;
  }
}

export class AlanaFoldingProvider implements vscode.FoldingRangeProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideFoldingRanges(document: vscode.TextDocument): vscode.FoldingRange[] {
    const analysis = this.index.getDocument(document);
    return analysis.blocks
      .filter((b) => b.endLine > b.startLine)
      .map((b) => new vscode.FoldingRange(b.startLine - 1, b.endLine - 1, vscode.FoldingRangeKind.Region));
  }
}
