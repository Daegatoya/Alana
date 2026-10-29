import { displayType } from "./catalog";
import { lex, Token, TokenKind, tokenEndCol } from "./lexer";

export type SymbolKind =
  | "function"
  | "class"
  | "method"
  | "constructor"
  | "variable"
  | "field"
  | "parameter";

export type Visibility = "public" | "private";

export interface ParamInfo {
  name: string;
  type: string;
  isArray: boolean;
  line: number;
  col: number;
}

export interface SymbolInfo {
  kind: SymbolKind;
  name: string;
  type?: string;
  isArray: boolean;
  visibility: Visibility;
  line: number;
  col: number;
  endLine: number;
  endCol: number;
  container?: string;
  params?: ParamInfo[];
  returnType?: string;
}

export interface BlockInfo {
  kind: "if" | "while" | "for" | "forevery" | "func" | "class" | "constructor" | "try" | "catch" | "finally";
  startLine: number;
  startCol: number;
  endLine: number;
  endCol: number;
  name?: string;
}

export interface ImportInfo {
  path: string;
  line: number;
  col: number;
  length: number;
  kind?: "import" | "including";
}

export interface AnalyzeDiagnostic {
  message: string;
  line: number;
  col: number;
  length: number;
  severity: "error" | "warning";
}

export interface Analysis {
  tokens: Token[];
  symbols: SymbolInfo[];
  blocks: BlockInfo[];
  imports: ImportInfo[];
  diagnostics: AnalyzeDiagnostic[];
  classes: Map<string, SymbolInfo>;
  openBlocks: OpenBlock[];
}

interface OpenBlock {
  kind: BlockInfo["kind"];
  startLine: number;
  startCol: number;
  name?: string;
  className?: string;
}

function at(tokens: Token[], i: number): Token | undefined {
  return i >= 0 && i < tokens.length ? tokens[i] : undefined;
}

function is(tokens: Token[], i: number, kind: TokenKind): boolean {
  return at(tokens, i)?.kind === kind;
}

function parseArray(tokens: Token[], i: number): { isArray: boolean; next: number } {
  if (is(tokens, i, "OPENARRAY") && is(tokens, i + 1, "CLOSEARRAY")) {
    return { isArray: true, next: i + 2 };
  }
  return { isArray: false, next: i };
}

function parseParams(tokens: Token[], i: number): { params: ParamInfo[]; next: number } {
  const params: ParamInfo[] = [];
  if (!is(tokens, i, "OPEN")) {
    return { params, next: i };
  }
  i++;
  while (i < tokens.length && !is(tokens, i, "CLOSE")) {
    const typeTok = at(tokens, i);
    if (!typeTok) {
      break;
    }
    i++;
    const array = parseArray(tokens, i);
    i = array.next;
    const nameTok = at(tokens, i);
    if (!nameTok || nameTok.kind !== "VAR") {
      break;
    }
    params.push({
      name: nameTok.value ?? nameTok.text,
      type: displayType(typeTok.kind, typeTok.value ?? typeTok.text) + (array.isArray ? "[]" : ""),
      isArray: array.isArray,
      line: nameTok.line,
      col: nameTok.col,
    });
    i++;
    if (is(tokens, i, "COMA")) {
      i++;
    }
  }
  if (is(tokens, i, "CLOSE")) {
    i++;
  }
  return { params, next: i };
}

function parseReturnType(tokens: Token[], i: number): { returnType?: string; next: number } {
  if (!is(tokens, i, "RETURN_TYPE")) {
    return { next: i };
  }
  i++;
  const typeTok = at(tokens, i);
  if (!typeTok) {
    return { next: i };
  }
  i++;
  if (typeTok.kind === "VAR") {
    return { returnType: typeTok.value ?? typeTok.text, next: i };
  }
  const array = parseArray(tokens, i);
  i = array.next;
  return {
    returnType: displayType(typeTok.kind, typeTok.text) + (array.isArray ? "[]" : ""),
    next: i,
  };
}

function typeTokenName(token: Token): string {
  return displayType(token.kind, token.value ?? token.text);
}

const PRIMITIVE_TYPES: TokenKind[] = ["NUM", "STR", "DECIMAL", "CHAR", "BOOL", "ANY"];

export function analyze(source: string): Analysis {
  const lexed = lex(source);
  const tokens = lexed.tokens;
  const diagnostics: AnalyzeDiagnostic[] = lexed.diagnostics.map((d) => ({
    ...d,
    severity: "error" as const,
  }));

  const symbols: SymbolInfo[] = [];
  const blocks: BlockInfo[] = [];
  const imports: ImportInfo[] = [];
  const classes = new Map<string, SymbolInfo>();
  const stack: OpenBlock[] = [];
  let visibility: Visibility = "public";
  let currentClass: string | undefined;

  const currentContainer = (): string | undefined => {
    for (let s = stack.length - 1; s >= 0; s--) {
      if (stack[s].kind === "func" || stack[s].kind === "constructor") {
        return stack[s].className ? `${stack[s].className}.${stack[s].name}` : stack[s].name;
      }
      if (stack[s].kind === "class") {
        return stack[s].name;
      }
    }
    return currentClass;
  };

  const inClass = (): boolean => {
    return stack.some((b) => b.kind === "class");
  };

  const pushBlock = (kind: BlockInfo["kind"], token: Token, name?: string): void => {
    stack.push({
      kind,
      startLine: token.line,
      startCol: token.col,
      name,
      className: inClass() ? currentClass : undefined,
    });
  };

  const seenFunctions = new Map<string, SymbolInfo>();
  const seenClasses = new Map<string, SymbolInfo>();

  let i = 0;
  while (i < tokens.length) {
    const tok = tokens[i];

    if (tok.kind === "PUBLIC" && is(tokens, i + 1, "TWODOTS")) {
      visibility = "public";
      i += 2;
      continue;
    }

    if (tok.kind === "PRIVATE" && is(tokens, i + 1, "TWODOTS")) {
      visibility = "private";
      i += 2;
      continue;
    }

    if (tok.kind === "IMPORT" || tok.kind === "INCLUDING") {
      const strTok = is(tokens, i + 1, "OPENCLOSESTR") ? at(tokens, i + 2) : undefined;
      if (strTok && strTok.kind === "STRING" && strTok.value !== undefined) {
        imports.push({
          path: strTok.value,
          line: strTok.line,
          col: strTok.col,
          length: strTok.text.length,
          kind: tok.kind === "INCLUDING" ? "including" : "import",
        });
      }
      i++;
      continue;
    }

    if (tok.kind === "SELF" && !inClass()) {
      diagnostics.push({
        message: "self can only be used inside a class",
        line: tok.line,
        col: tok.col,
        length: tok.text.length,
        severity: "error",
      });
    }

    if (tok.kind === "IF") {
      pushBlock("if", tok);
      i++;
      continue;
    }

    if (tok.kind === "WHILE") {
      pushBlock("while", tok);
      i++;
      continue;
    }

    if (tok.kind === "FOR") {
      pushBlock("for", tok);
      i++;
      continue;
    }

    if (tok.kind === "FOREVERY") {
      pushBlock("forevery", tok);
      i++;
      continue;
    }

    if (tok.kind === "TRY") {
      pushBlock("try", tok);
      i++;
      continue;
    }

    if (tok.kind === "CATCH") {
      pushBlock("catch", tok);
      i++;
      continue;
    }

    if (tok.kind === "FINALLY") {
      pushBlock("finally", tok);
      i++;
      continue;
    }

    const funcKeyword =
      tok.kind === "DEFINE" && is(tokens, i + 1, "DEFFUNC")
        ? i + 1
        : tok.kind === "DEFINE" &&
            (is(tokens, i + 1, "OVERRIDE") || is(tokens, i + 1, "ABSTRACT")) &&
            is(tokens, i + 2, "DEFFUNC")
          ? i + 2
          : -1;

    if (funcKeyword >= 0) {
      const isAbstract = is(tokens, i + 1, "ABSTRACT");
      const nameTok = at(tokens, funcKeyword + 1);
      const name = nameTok?.kind === "VAR" ? (nameTok.value ?? nameTok.text) : "func";
      const parsed = parseParams(tokens, nameTok?.kind === "VAR" ? funcKeyword + 2 : funcKeyword + 1);
      const ret = parseReturnType(tokens, parsed.next);
      const method = inClass();
      const symbol: SymbolInfo = {
        kind: method ? "method" : "function",
        name,
        visibility,
        isArray: false,
        line: nameTok?.line ?? tok.line,
        col: nameTok?.col ?? tok.col,
        endLine: nameTok?.line ?? tok.line,
        endCol: nameTok ? tokenEndCol(nameTok) : tokenEndCol(tok),
        container: method ? currentClass : undefined,
        params: parsed.params,
        returnType: ret.returnType,
      };
      symbols.push(symbol);
      for (const p of parsed.params) {
        symbols.push({
          kind: "parameter",
          name: p.name,
          type: p.type,
          isArray: p.isArray,
          visibility: "private",
          line: p.line,
          col: p.col,
          endLine: p.line,
          endCol: p.col + p.name.length,
          container: method && currentClass ? `${currentClass}.${name}` : name,
        });
      }
      const key = method && currentClass ? `${currentClass}.${name}` : name;
      const existing = seenFunctions.get(key);
      if (existing && !method) {
        diagnostics.push({
          message: `Function ${name} is already defined`,
          line: symbol.line,
          col: symbol.col,
          length: name.length,
          severity: "error",
        });
      } else {
        seenFunctions.set(key, symbol);
      }
      if (!isAbstract) {
        pushBlock("func", tok, name);
      }
      i = ret.next;
      continue;
    }

    const classKeyword =
      tok.kind === "DEFINE" && is(tokens, i + 1, "CLASS")
        ? i + 1
        : tok.kind === "DEFINE" && is(tokens, i + 1, "ABSTRACT") && is(tokens, i + 2, "CLASS")
          ? i + 2
          : -1;

    if (classKeyword >= 0) {
      const nameTok = at(tokens, classKeyword + 1);
      const name = nameTok?.kind === "VAR" ? (nameTok.value ?? nameTok.text) : "class";
      const symbol: SymbolInfo = {
        kind: "class",
        name,
        visibility,
        isArray: false,
        line: nameTok?.line ?? tok.line,
        col: nameTok?.col ?? tok.col,
        endLine: nameTok?.line ?? tok.line,
        endCol: nameTok ? tokenEndCol(nameTok) : tokenEndCol(tok),
      };
      symbols.push(symbol);
      classes.set(name, symbol);
      const existing = seenClasses.get(name);
      if (existing) {
        diagnostics.push({
          message: `Class ${name} is already defined`,
          line: symbol.line,
          col: symbol.col,
          length: name.length,
          severity: "error",
        });
      } else {
        seenClasses.set(name, symbol);
      }
      currentClass = name;
      pushBlock("class", tok, name);
      let next = nameTok?.kind === "VAR" ? classKeyword + 2 : classKeyword + 1;
      if (is(tokens, next, "EXTENDS") && is(tokens, next + 1, "VAR")) {
        next += 2;
      }
      i = next;
      continue;
    }

    if (tok.kind === "DEFINE" && is(tokens, i + 1, "CONSTRUCTOR")) {
      const parsed = parseParams(tokens, i + 2);
      const symbol: SymbolInfo = {
        kind: "constructor",
        name: "constructor",
        visibility,
        isArray: false,
        line: tok.line,
        col: tok.col,
        endLine: tok.line,
        endCol: tokenEndCol(at(tokens, i + 1) ?? tok),
        container: currentClass,
        params: parsed.params,
      };
      symbols.push(symbol);
      for (const p of parsed.params) {
        symbols.push({
          kind: "parameter",
          name: p.name,
          type: p.type,
          isArray: p.isArray,
          visibility: "private",
          line: p.line,
          col: p.col,
          endLine: p.line,
          endCol: p.col + p.name.length,
          container: currentClass ? `${currentClass}.constructor` : "constructor",
        });
      }
      pushBlock("constructor", tok, "constructor");
      i = parsed.next;
      continue;
    }

    if (tok.kind === "DEFINE" && at(tokens, i + 1) && PRIMITIVE_TYPES.includes(at(tokens, i + 1)!.kind)) {
      const typeTok = at(tokens, i + 1)!;
      const array = parseArray(tokens, i + 2);
      const nameTok = at(tokens, array.next);
      if (nameTok && nameTok.kind === "VAR") {
        const typeName = typeTokenName(typeTok) + (array.isArray ? "[]" : "");
        symbols.push({
          kind: inClass() && stack[stack.length - 1]?.kind === "class" ? "field" : "variable",
          name: nameTok.value ?? nameTok.text,
          type: typeName,
          isArray: array.isArray,
          visibility,
          line: nameTok.line,
          col: nameTok.col,
          endLine: nameTok.line,
          endCol: tokenEndCol(nameTok),
          container: currentContainer(),
        });
        i = array.next + 1;
        continue;
      }
    }

    if (tok.kind === "DEFINE" && is(tokens, i + 1, "VAR") && is(tokens, i + 2, "OPENARRAY") && is(tokens, i + 3, "CLOSEARRAY") && is(tokens, i + 4, "VAR")) {
      const classTok = at(tokens, i + 1)!;
      const nameTok = at(tokens, i + 4)!;
      symbols.push({
        kind: inClass() && stack[stack.length - 1]?.kind === "class" ? "field" : "variable",
        name: nameTok.value ?? nameTok.text,
        type: (classTok.value ?? classTok.text) + "[]",
        isArray: true,
        visibility,
        line: nameTok.line,
        col: nameTok.col,
        endLine: nameTok.line,
        endCol: tokenEndCol(nameTok),
        container: currentContainer(),
      });
      i += 5;
      continue;
    }

    if (tok.kind === "DEFINE" && is(tokens, i + 1, "VAR") && is(tokens, i + 2, "VAR")) {
      const classTok = at(tokens, i + 1)!;
      const nameTok = at(tokens, i + 2)!;
      symbols.push({
        kind: inClass() && stack[stack.length - 1]?.kind === "class" ? "field" : "variable",
        name: nameTok.value ?? nameTok.text,
        type: classTok.value ?? classTok.text,
        isArray: false,
        visibility,
        line: nameTok.line,
        col: nameTok.col,
        endLine: nameTok.line,
        endCol: tokenEndCol(nameTok),
        container: currentContainer(),
      });
      i += 3;
      continue;
    }

    if (tok.kind === "EXITFUNC") {
      const opened = stack.pop();
      if (!opened) {
        diagnostics.push({
          message: "Unexpected end",
          line: tok.line,
          col: tok.col,
          length: tok.text.length,
          severity: "error",
        });
      } else {
        const endTok = is(tokens, i + 1, "END") ? at(tokens, i + 1)! : tok;
        blocks.push({
          kind: opened.kind,
          startLine: opened.startLine,
          startCol: opened.startCol,
          endLine: endTok.line,
          endCol: tokenEndCol(endTok),
          name: opened.name,
        });
        if (opened.kind === "class") {
          const parentClass = stack.filter((b) => b.kind === "class").pop();
          currentClass = parentClass?.name;
        }
      }
      i++;
      continue;
    }

    i++;
  }

  for (const opened of stack) {
    diagnostics.push({
      message: `Missing end for ${opened.kind}${opened.name ? " " + opened.name : ""}`,
      line: opened.startLine,
      col: opened.startCol,
      length: opened.name?.length ?? 1,
      severity: "error",
    });
  }

  return { tokens, symbols, blocks, imports, diagnostics, classes, openBlocks: [...stack] };
}

export function enclosingClass(analysis: Analysis, line: number): string | undefined {
  for (const block of analysis.blocks) {
    if (block.kind === "class" && block.startLine <= line && block.endLine >= line) {
      return block.name;
    }
  }
  for (const block of analysis.openBlocks) {
    if (block.kind === "class" && block.startLine <= line) {
      return block.name;
    }
  }
  return undefined;
}

export function tokenAt(tokens: Token[], line: number, col: number): Token | undefined {
  for (let i = tokens.length - 1; i >= 0; i--) {
    const t = tokens[i];
    if (t.line > line) {
      continue;
    }
    if (t.line < line) {
      return t;
    }
    if (t.col <= col && tokenEndCol(t) >= col) {
      return t;
    }
    if (t.col <= col) {
      return t;
    }
  }
  return undefined;
}

export function identifierAt(tokens: Token[], line: number, character: number): Token | undefined {
  const col = character + 1;
  for (const t of tokens) {
    if (t.line !== line + 1) {
      continue;
    }
    if (t.kind !== "VAR" && t.kind !== "SELF" && t.kind !== "STRING") {
      if (!/^[A-Za-z_]/.test(t.text)) {
        continue;
      }
    }
    if (t.col <= col && tokenEndCol(t) >= col) {
      return t;
    }
  }
  return undefined;
}

export function classOfVariable(analysis: Analysis, name: string): string | undefined {
  const matches = analysis.symbols.filter(
    (s) => s.name === name && (s.kind === "variable" || s.kind === "field" || s.kind === "parameter") && s.type && !s.isArray
  );
  const last = matches[matches.length - 1];
  if (!last?.type) {
    return undefined;
  }
  if (["num", "str", "decimal", "schar", "boolean", "any"].some((t) => last.type === t || last.type === `${t}[]`)) {
    return undefined;
  }
  return last.type.replace(/\[\]$/, "");
}

export function membersOf(analysis: Analysis, className: string): SymbolInfo[] {
  return analysis.symbols.filter(
    (s) => s.container === className && (s.kind === "method" || s.kind === "field" || s.kind === "constructor")
  );
}

export function formatSignature(symbol: SymbolInfo): string {
  const params = (symbol.params ?? []).map((p) => `${p.type} ${p.name}`).join(", ");
  if (symbol.kind === "constructor") {
    return `constructor(${params})`;
  }
  const ret = symbol.returnType ? ` returning ${symbol.returnType}` : "";
  if (symbol.kind === "method" || symbol.kind === "function") {
    return `define func ${symbol.name}(${params})${ret}`;
  }
  const type = symbol.type ?? "any";
  return `define ${type} ${symbol.name}`;
}
