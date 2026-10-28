import * as vscode from "vscode";
import { NATIVES } from "./catalog";
import { classOfVariable, identifierAt, membersOf } from "./analyze";
import { AlanaIndex } from "./index";
import { Token, tokenEndCol } from "./lexer";

function rangeOf(token: Token): vscode.Range {
  return new vscode.Range(token.line - 1, token.col - 1, token.line - 1, tokenEndCol(token) - 1);
}

function location(uri: vscode.Uri, line: number, col: number, length: number): vscode.Location {
  return new vscode.Location(
    uri,
    new vscode.Range(line - 1, col - 1, line - 1, col - 1 + Math.max(1, length))
  );
}

export class AlanaDefinitionProvider implements vscode.DefinitionProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideDefinition(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.Definition | undefined {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    if (!token) {
      return undefined;
    }

    if (token.kind === "STRING") {
      const around = analysis.imports.find((i) => i.line === token.line && i.path === token.value);
      if (around) {
        const uri = this.index.resolveImport(document.uri, around.path);
        if (uri) {
          return new vscode.Location(uri, new vscode.Position(0, 0));
        }
      }
    }

    const name = token.value ?? token.text;

    const idx = analysis.tokens.indexOf(token);
    const prev = idx > 0 ? analysis.tokens[idx - 1] : undefined;
    const next = idx >= 0 ? analysis.tokens[idx + 1] : undefined;

    if (prev?.kind === "NEWOBJ" || prev?.kind === "CLASS") {
      const cls = this.index.lookupClass(document, name);
      if (cls) {
        return location(cls.uri, cls.symbol.line, cls.symbol.col, cls.symbol.name.length);
      }
    }

    if (prev?.kind === "DOT") {
      const owner = idx >= 2 ? analysis.tokens[idx - 2] : undefined;
      if (owner?.kind === "SELF") {
        const classBlock = analysis.blocks.find(
          (b) => b.kind === "class" && b.startLine <= token.line && b.endLine >= token.line
        );
        if (classBlock?.name) {
          const member = membersOf(analysis, classBlock.name).find((s) => s.name === name);
          if (member) {
            return location(document.uri, member.line, member.col, member.name.length);
          }
        }
      }
      if (owner?.kind === "VAR") {
        const className = classOfVariable(analysis, owner.value ?? owner.text);
        if (className) {
          const member = this.index.classMembers(document, className).find((s) => s.name === name);
          const cls = this.index.lookupClass(document, className);
          if (member && cls) {
            return location(cls.uri, member.line, member.col, member.name.length);
          }
        }
      }
    }

    if (prev?.kind === "CALL" && next?.kind === "OPEN") {
      const fn = this.index.lookupFunction(document, name);
      if (fn) {
        return location(fn.uri, fn.symbol.line, fn.symbol.col, fn.symbol.name.length);
      }
    }

    const local = [...analysis.symbols].reverse().find((s) => s.name === name && s.kind !== "class");
    if (local) {
      return location(document.uri, local.line, local.col, local.name.length);
    }

    const cls = this.index.lookupClass(document, name);
    if (cls) {
      return location(cls.uri, cls.symbol.line, cls.symbol.col, cls.symbol.name.length);
    }

    const fn = this.index.lookupFunction(document, name);
    if (fn) {
      return location(fn.uri, fn.symbol.line, fn.symbol.col, fn.symbol.name.length);
    }

    return undefined;
  }
}

export class AlanaReferenceProvider implements vscode.ReferenceProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideReferences(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.Location[] {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    if (!token) {
      return [];
    }
    const name = token.value ?? token.text;
    if (NATIVES.includes(name)) {
      return this.collectName(document, name);
    }
    return this.collectName(document, name);
  }

  private collectName(document: vscode.TextDocument, name: string): vscode.Location[] {
    const analysis = this.index.getDocument(document);
    const locations: vscode.Location[] = [];
    for (const t of analysis.tokens) {
      if ((t.kind === "VAR" || t.text === name) && (t.value ?? t.text) === name) {
        locations.push(new vscode.Location(document.uri, rangeOf(t)));
      }
    }
    return locations;
  }
}

export class AlanaRenameProvider implements vscode.RenameProvider {
  constructor(private readonly index: AlanaIndex) {}

  prepareRename(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.Range | undefined {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    if (!token || token.kind !== "VAR") {
      throw new Error("You can only rename identifiers.");
    }
    return rangeOf(token);
  }

  provideRenameEdits(
    document: vscode.TextDocument,
    position: vscode.Position,
    newName: string
  ): vscode.WorkspaceEdit {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    const edit = new vscode.WorkspaceEdit();
    if (!token) {
      return edit;
    }
    const name = token.value ?? token.text;
    for (const t of analysis.tokens) {
      if (t.kind === "VAR" && (t.value ?? t.text) === name) {
        edit.replace(document.uri, rangeOf(t), newName);
      }
    }
    return edit;
  }
}

export class AlanaHighlightProvider implements vscode.DocumentHighlightProvider {
  constructor(private readonly index: AlanaIndex) {}

  provideDocumentHighlights(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.DocumentHighlight[] {
    const analysis = this.index.getDocument(document);
    const token = identifierAt(analysis.tokens, position.line, position.character);
    if (!token) {
      return [];
    }
    const name = token.value ?? token.text;
    const highlights: vscode.DocumentHighlight[] = [];
    for (const t of analysis.tokens) {
      if ((t.value ?? t.text) === name && (t.kind === "VAR" || t.kind === token.kind)) {
        highlights.push(new vscode.DocumentHighlight(rangeOf(t)));
      }
    }
    return highlights;
  }
}
