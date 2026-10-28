export type TokenKind =
  | "NUM"
  | "DIGIT"
  | "ADD"
  | "SUB"
  | "END"
  | "VAR"
  | "EQUAL"
  | "DEFINE"
  | "SHOW"
  | "MULTI"
  | "DIVIDE"
  | "STR"
  | "COMA"
  | "OPEN"
  | "CLOSE"
  | "EXITFUNC"
  | "DEFFUNC"
  | "RETURN"
  | "OPENCLOSESTR"
  | "STRING"
  | "CALL"
  | "BOOL"
  | "DECIMAL"
  | "CHAR"
  | "DECIMAL_NUM"
  | "OPENCLOSECHAR"
  | "IF"
  | "EQUAL_EQUAL"
  | "NOT"
  | "GREATERTHAN"
  | "LESSTHAN"
  | "AND"
  | "OR"
  | "LESSOREQUAL"
  | "GREATEROREQUAL"
  | "ELSEIF"
  | "ELSE"
  | "REDEFINE"
  | "NEW"
  | "OPENARRAY"
  | "CLOSEARRAY"
  | "PUSH"
  | "WITH"
  | "POP"
  | "LENGTH"
  | "SHOWLN"
  | "READLN"
  | "READK"
  | "PARSENUM"
  | "PARSESTR"
  | "PARSEBOOL"
  | "PARSEDEC"
  | "PARSECHAR"
  | "TRYPARSENUM"
  | "TRYPARSEDEC"
  | "TRYPARSEBOOL"
  | "TRYPARSECHAR"
  | "WHILE"
  | "FOR"
  | "FOREVERY"
  | "CONTINUE"
  | "BREAK"
  | "WITHIN"
  | "MODULO"
  | "IMPORT"
  | "PUBLIC"
  | "PRIVATE"
  | "TWODOTS"
  | "RETURN_TYPE"
  | "CLASS"
  | "CONSTRUCTOR"
  | "NEWOBJ"
  | "DOT"
  | "SELF"
  | "ANY";

export interface Token {
  kind: TokenKind;
  text: string;
  value?: string;
  line: number;
  col: number;
}

export interface LexDiagnostic {
  message: string;
  line: number;
  col: number;
  length: number;
}

const KEYWORDS = {
  define: "DEFINE",
  num: "NUM",
  is: "EQUAL",
  show: "SHOW",
  times: "MULTI",
  divide: "DIVIDE",
  str: "STR",
  end: "EXITFUNC",
  func: "DEFFUNC",
  return: "RETURN",
  call: "CALL",
  decimal: "DECIMAL",
  schar: "CHAR",
  boolean: "BOOL",
  any: "ANY",
  sameas: "EQUAL_EQUAL",
  if: "IF",
  not: "NOT",
  greaterthan: "GREATERTHAN",
  lessthan: "LESSTHAN",
  or: "OR",
  and: "AND",
  grorsame: "GREATEROREQUAL",
  leorsame: "LESSOREQUAL",
  elseif: "ELSEIF",
  else: "ELSE",
  redefine: "REDEFINE",
  new: "NEW",
  push: "PUSH",
  pop: "POP",
  with: "WITH",
  length: "LENGTH",
  showln: "SHOWLN",
  readln: "READLN",
  readk: "READK",
  parsedecimal: "PARSEDEC",
  parsestr: "PARSESTR",
  parsenum: "PARSENUM",
  parseschar: "PARSECHAR",
  parseboolean: "PARSEBOOL",
  tryparsenum: "TRYPARSENUM",
  tryparsedecimal: "TRYPARSEDEC",
  tryparseboolean: "TRYPARSEBOOL",
  tryparseschar: "TRYPARSECHAR",
  while: "WHILE",
  for: "FOR",
  forevery: "FOREVERY",
  continue: "CONTINUE",
  break: "BREAK",
  within: "WITHIN",
  mod: "MODULO",
  import: "IMPORT",
  public: "PUBLIC",
  private: "PRIVATE",
  returning: "RETURN_TYPE",
  class: "CLASS",
  ["constructor"]: "CONSTRUCTOR",
  newobj: "NEWOBJ",
  self: "SELF",
} as Record<string, TokenKind>;

function isDigit(c: string): boolean {
  return c >= "0" && c <= "9";
}

function isIdentStart(c: string): boolean {
  return /[A-Za-z_]/.test(c);
}

function isIdentPart(c: string): boolean {
  return /[A-Za-z0-9_]/.test(c);
}

function stripComment(line: string): string {
  let inString = false;
  let inChar = false;
  let escaped = false;

  for (let i = 0; i < line.length; i++) {
    const c = line[i];

    if (escaped) {
      escaped = false;
      continue;
    }

    if (c === "\\") {
      escaped = true;
      continue;
    }

    if (c === "\"" && !inChar) {
      inString = !inString;
      continue;
    }

    if (c === "'" && !inString) {
      inChar = !inChar;
      continue;
    }

    if (!inString && !inChar && c === "/" && i + 1 < line.length && line[i + 1] === "/") {
      return line.substring(0, i);
    }
  }

  return line;
}

function checkLineTerminator(lineText: string, line: number, diagnostics: LexDiagnostic[]): void {
  const currentLine = stripComment(lineText).replace(/[\r \t]+$/, "");
  const trimmed = currentLine.trim();

  if (trimmed.length > 0 && trimmed !== "public:" && trimmed !== "private:") {
    if (currentLine[currentLine.length - 1] !== ";") {
      diagnostics.push({
        message: "Expected end of line (;) but couldn't find it",
        line,
        col: Math.max(1, lineText.replace(/\r$/, "").length),
        length: 1,
      });
    }
  }
}

export function lex(source: string): { tokens: Token[]; diagnostics: LexDiagnostic[] } {
  const tokens: Token[] = [];
  const diagnostics: LexDiagnostic[] = [];
  const sources = source.split("\n");
  let pos = 0;
  let line = 1;
  let col = 1;

  const add = (kind: TokenKind, text: string, atLine: number, atCol: number, value?: string): void => {
    tokens.push({ kind, text, line: atLine, col: atCol, value });
  };

  while (pos < source.length) {
    const c = source[pos];

    if (c === "\n") {
      checkLineTerminator(sources[line - 1] ?? "", line, diagnostics);
      line++;
      col = 1;
      pos++;
      continue;
    }

    if (c === " " || c === "\t" || c === "\r") {
      pos++;
      col++;
      continue;
    }

    if (c === "/") {
      if (pos + 1 < source.length && source[pos + 1] === "/") {
        while (pos < source.length && source[pos] !== "\n") {
          pos++;
          col++;
        }
        continue;
      }

      diagnostics.push({
        message: "Invalid character / - maybe you meant // ?",
        line,
        col,
        length: 1,
      });
      pos++;
      col++;
      continue;
    }

    if (c === ";") {
      add("END", ";", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === ".") {
      add("DOT", ".", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === ":") {
      add("TWODOTS", ":", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "[") {
      add("OPENARRAY", "[", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "]") {
      add("CLOSEARRAY", "]", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === ",") {
      add("COMA", ",", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "(") {
      add("OPEN", "(", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === ")") {
      add("CLOSE", ")", line, col);
      pos++;
      col++;
      continue;
    }

    if (isDigit(c)) {
      let number = "";
      const colBeforeLoop = col;
      const lineBefore = line;

      while (pos < source.length && isDigit(source[pos])) {
        number += source[pos];
        pos++;
        col++;
      }

      if (pos < source.length && source[pos] === ".") {
        number += source[pos];
        pos++;
        col++;
        while (pos < source.length && isDigit(source[pos])) {
          number += source[pos];
          pos++;
          col++;
        }
        add("DECIMAL_NUM", number, lineBefore, colBeforeLoop, number);
        continue;
      }

      add("DIGIT", number, lineBefore, colBeforeLoop, number);
      continue;
    }

    if (c === "+") {
      add("ADD", "+", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "-") {
      add("SUB", "-", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "\"") {
      add("OPENCLOSESTR", "\"", line, col);
      pos++;
      col++;

      let word = "";
      const startLine = line;
      const startCol = col;
      let closed = false;

      while (pos < source.length) {
        if (source[pos] === "\\" && pos + 1 < source.length && source[pos + 1] === "\"") {
          word += "\"";
          pos += 2;
          col += 2;
          continue;
        }

        if (source[pos] === "\"") {
          closed = true;
          break;
        }

        if (source[pos] === "\n") {
          word += "\n";
          pos++;
          line++;
          col = 1;
          continue;
        }

        word += source[pos];
        pos++;
        col++;
      }

      if (!closed) {
        diagnostics.push({
          message: "Unterminated string",
          line: startLine,
          col: startCol,
          length: 1,
        });
        break;
      }

      add("STRING", word, startLine, startCol, word);
      add("OPENCLOSESTR", "\"", line, col);
      pos++;
      col++;
      continue;
    }

    if (c === "'") {
      add("OPENCLOSECHAR", "'", line, col);
      pos++;
      col++;
      let letter = "";
      const colBeforeLoop = col;

      while (pos < source.length && source[pos] !== "'") {
        letter += source[pos];
        pos++;
        col++;
      }

      if (letter.length === 1) {
        add("CHAR", letter, line, colBeforeLoop, letter);
      } else {
        diagnostics.push({
          message: "SCHAR error: expected a char ('') but received another type",
          line,
          col: colBeforeLoop,
          length: Math.max(1, letter.length),
        });
      }

      if (pos < source.length && source[pos] === "'") {
        add("OPENCLOSECHAR", "'", line, col);
        pos++;
        col++;
        continue;
      }

      diagnostics.push({
        message: "Expected closing '",
        line,
        col: colBeforeLoop,
        length: 1,
      });
      continue;
    }

    if (isIdentStart(c)) {
      let word = "";
      const colBeforeLoop = col;

      while (pos < source.length && isIdentPart(source[pos])) {
        word += source[pos];
        pos++;
        col++;
      }

      const mapped = KEYWORDS[word];
      if (mapped) {
        add(mapped, word, line, colBeforeLoop);
      } else {
        add("VAR", word, line, colBeforeLoop, word);
      }
      continue;
    }

    diagnostics.push({
      message: `Unknown character ${c}`,
      line,
      col,
      length: 1,
    });
    pos++;
    col++;
  }

  if (!source.endsWith("\n") && sources.length > 0) {
    checkLineTerminator(sources[sources.length - 1] ?? "", sources.length, diagnostics);
  }

  return { tokens, diagnostics };
}

export function tokenEndCol(token: Token): number {
  return token.col + token.text.length;
}
