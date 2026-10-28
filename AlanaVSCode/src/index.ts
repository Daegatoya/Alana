import * as fs from "fs";
import * as path from "path";
import * as vscode from "vscode";
import { Analysis, analyze, SymbolInfo } from "./analyze";

interface CacheEntry {
  version: number;
  analysis: Analysis;
}

export class AlanaIndex {
  private readonly cache = new Map<string, CacheEntry>();
  private readonly fileCache = new Map<string, { mtime: number; analysis: Analysis }>();

  getDocument(document: vscode.TextDocument): Analysis {
    const key = document.uri.toString();
    const cached = this.cache.get(key);
    if (cached && cached.version === document.version) {
      return cached.analysis;
    }
    const analysis = analyze(document.getText());
    this.cache.set(key, { version: document.version, analysis });
    return analysis;
  }

  invalidate(document: vscode.TextDocument): void {
    this.cache.delete(document.uri.toString());
  }

  getFile(uri: vscode.Uri): Analysis | undefined {
    const open = vscode.workspace.textDocuments.find((d) => d.uri.toString() === uri.toString());
    if (open) {
      return this.getDocument(open);
    }

    const filePath = uri.fsPath;
    try {
      const stat = fs.statSync(filePath);
      const cached = this.fileCache.get(filePath);
      if (cached && cached.mtime === stat.mtimeMs) {
        return cached.analysis;
      }
      const text = fs.readFileSync(filePath, "utf8");
      const analysis = analyze(text);
      this.fileCache.set(filePath, { mtime: stat.mtimeMs, analysis });
      return analysis;
    } catch {
      return undefined;
    }
  }

  resolveImport(from: vscode.Uri, importPath: string): vscode.Uri | undefined {
    const dir = path.dirname(from.fsPath);
    const resolved = path.normalize(path.resolve(dir, importPath));
    if (fs.existsSync(resolved)) {
      return vscode.Uri.file(resolved);
    }
    return undefined;
  }

  importedPublicSymbols(from: vscode.Uri, analysis: Analysis): SymbolInfo[] {
    const result: SymbolInfo[] = [];
    const visited = new Set<string>();
    const visit = (uri: vscode.Uri, current: Analysis, depth: number): void => {
      if (depth > 8) {
        return;
      }
      for (const imp of current.imports) {
        const target = this.resolveImport(uri, imp.path);
        if (!target) {
          continue;
        }
        const key = target.fsPath.toLowerCase();
        if (visited.has(key)) {
          continue;
        }
        visited.add(key);
        const imported = this.getFile(target);
        if (!imported) {
          continue;
        }
        for (const symbol of imported.symbols) {
          if (symbol.visibility !== "public") {
            continue;
          }
          if (symbol.kind === "function" || symbol.kind === "class") {
            result.push(symbol);
          }
        }
        visit(target, imported, depth + 1);
      }
    };
    visit(from, analysis, 0);
    return result;
  }

  lookupFunction(document: vscode.TextDocument, name: string): { symbol: SymbolInfo; uri: vscode.Uri } | undefined {
    const analysis = this.getDocument(document);
    const local = analysis.symbols.find((s) => s.kind === "function" && s.name === name);
    if (local) {
      return { symbol: local, uri: document.uri };
    }
    const imported = this.importedPublicSymbols(document.uri, analysis).find(
      (s) => s.kind === "function" && s.name === name
    );
    if (imported) {
      const match = this.findSymbolUri(document.uri, analysis, imported);
      if (match) {
        return match;
      }
    }
    return undefined;
  }

  lookupClass(document: vscode.TextDocument, name: string): { symbol: SymbolInfo; uri: vscode.Uri } | undefined {
    const analysis = this.getDocument(document);
    const local = analysis.symbols.find((s) => s.kind === "class" && s.name === name);
    if (local) {
      return { symbol: local, uri: document.uri };
    }
    const imported = this.importedPublicSymbols(document.uri, analysis).find(
      (s) => s.kind === "class" && s.name === name
    );
    if (imported) {
      const match = this.findSymbolUri(document.uri, analysis, imported);
      if (match) {
        return match;
      }
    }
    return undefined;
  }

  classMembers(document: vscode.TextDocument, className: string): SymbolInfo[] {
    const found = this.lookupClass(document, className);
    if (!found) {
      return [];
    }
    const analysis = this.getFile(found.uri);
    if (!analysis) {
      return [];
    }
    return analysis.symbols.filter(
      (s) => s.container === className && (s.kind === "method" || s.kind === "field" || s.kind === "constructor")
    );
  }

  private findSymbolUri(
    from: vscode.Uri,
    analysis: Analysis,
    symbol: SymbolInfo
  ): { symbol: SymbolInfo; uri: vscode.Uri } | undefined {
    const visited = new Set<string>();
    const walk = (uri: vscode.Uri, current: Analysis, depth: number): { symbol: SymbolInfo; uri: vscode.Uri } | undefined => {
      if (depth > 8) {
        return undefined;
      }
      for (const imp of current.imports) {
        const target = this.resolveImport(uri, imp.path);
        if (!target) {
          continue;
        }
        const key = target.fsPath.toLowerCase();
        if (visited.has(key)) {
          continue;
        }
        visited.add(key);
        const imported = this.getFile(target);
        if (!imported) {
          continue;
        }
        const match = imported.symbols.find(
          (s) => s.kind === symbol.kind && s.name === symbol.name && s.visibility === "public"
        );
        if (match) {
          return { symbol: match, uri: target };
        }
        const nested = walk(target, imported, depth + 1);
        if (nested) {
          return nested;
        }
      }
      return undefined;
    };
    return walk(from, analysis, 0);
  }
}
