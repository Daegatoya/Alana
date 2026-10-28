# Alana for Visual Studio Code

Language support for [Alana](https://csalana.com).

## Features

- Syntax highlighting aligned with the Alana lexer (`schar`, `any`, `end`, `times`, natives, …)
- IntelliSense: keywords, types, locals, functions, classes, methods, `self.`, natives
- Signature help for functions, constructors, `parse*` / `tryparse*`, and `__http_*` / `__json_serialize`
- Hover documentation
- Go to definition, find references, rename, and document highlights
- Outline and workspace symbols
- Folding for `func` / `class` / `if` / loops
- Diagnostics: missing `;`, unterminated strings, unmatched `end`, missing imports, unknown `call`
- Snippets for the main language constructs
- **Alana: Run File** from the editor title bar (`alana` must be on your PATH)

Statements end with `;`. Comments are `//`. Visibility sections are written `public:` and `private:`.

## Example

```alana
define num x is 10;
define str[] names is ["Ada", "Grace"];

define func add(num a, num b) returning num;
    return a + b;
end;

if x greaterthan 5;
    showln call add(x, 1);
end;
```
