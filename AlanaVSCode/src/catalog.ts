export const TYPES = ["num", "str", "decimal", "schar", "boolean", "any"] as const;

export const KEYWORDS: Record<string, string> = {
  define: "Declares a variable, function, class, or constructor.",
  redefine: "Assigns a new value to an existing variable, field, or array slot.",
  is: "Assignment operator. Used with define and redefine.",
  func: "Declares a function. Body ends with end;",
  class: "Declares a class. Body ends with end;",
  constructor: "Declares a class constructor. Body ends with end;",
  end: "Closes a func, class, constructor, if, while, for, or forevery block.",
  return: "Returns a value from a function or method.",
  returning: "Declares the return type of a function: returning num, returning Person, returning any[].",
  call: "Calls a function or method: call add(1, 2); or call person.GetName();",
  import: "Imports another .alana file relative to the current file.",
  public: "Starts a public: visibility section for functions, classes, and members.",
  private: "Starts a private: visibility section. Private names are only visible in their own module.",
  new: "Marks a variable as declared but not initialized.",
  newobj: "Creates a class instance: newobj Person(\"Ada\", 30).",
  self: "Refers to the current instance inside a class. Use self.field or call self.Method().",
  if: "Starts a conditional block. Closed by end;",
  elseif: "Additional condition after if, checked if previous conditions were false.",
  else: "Runs when every if / elseif condition was false. Written else;",
  while: "Loop that runs while the condition is true. Closed by end;",
  for: "Counted loop: for (define num i is 0, i lessthan 10, redefine i is i + 1);",
  forevery: "Iterates an array: forevery (define str name within names);",
  within: "Names the array iterated by forevery.",
  continue: "Skips the rest of the current loop iteration.",
  break: "Exits the current loop.",
  show: "Writes a value to the console without a newline.",
  showln: "Writes a value to the console followed by a newline.",
  readln: "Reads a full line of input. Returns str.",
  readk: "Reads one key. Returns schar.",
  push: "Appends a value to an array: push numbers with 40;",
  pop: "Removes the last value of an array: pop numbers;",
  with: "Introduces the value pushed onto an array.",
  length: "Returns the number of elements in an array: length names.",
  parsenum: "Converts a value to num. Accepts str, schar, or num.",
  parsestr: "Converts a value to str.",
  parsedecimal: "Converts a value to decimal. Accepts str, schar, num, or decimal.",
  parseschar: "Converts a value to schar. Accepts str, num, or schar.",
  parseboolean: "Converts a value to boolean. Accepts str or boolean.",
  tryparsenum: "Statement: tryparsenum(value, target, success); Writes the num and a boolean success flag.",
  tryparsedecimal: "Statement: tryparsedecimal(value, target, success);",
  tryparseschar: "Statement: tryparseschar(value, target, success);",
  tryparseboolean: "Statement: tryparseboolean(value, target, success);",
  sameas: "Equality comparison. Numbers, strings, booleans, characters, and objects (same instance).",
  greaterthan: "Numeric comparison: left > right.",
  lessthan: "Numeric comparison: left < right.",
  grorsame: "Numeric comparison: left >= right.",
  leorsame: "Numeric comparison: left <= right.",
  not: "Boolean negation.",
  and: "Boolean and.",
  or: "Boolean or.",
  times: "Multiplication.",
  divide: "Division. num divide num yields decimal.",
  mod: "Modulo.",
  num: "Integer type.",
  str: "String type.",
  decimal: "Decimal number type.",
  schar: "Single character type. Written 'A'.",
  boolean: "Boolean type. Written true or false.",
  any: "Holds any Alana value: primitives, arrays, or class instances. Like object in C#.",
};

export const OPERATORS = [
  "times",
  "divide",
  "mod",
  "sameas",
  "greaterthan",
  "lessthan",
  "grorsame",
  "leorsame",
  "and",
  "or",
  "not",
];

export interface SignatureInfo {
  label: string;
  parameters: { label: string; documentation: string }[];
  documentation: string;
}

export const BUILTIN_SIGNATURES: Record<string, SignatureInfo[]> = {
  parsenum: [
    {
      label: "parsenum(value)",
      documentation: "Parses value as num.",
      parameters: [{ label: "value", documentation: "str, schar, or num." }],
    },
  ],
  parsestr: [
    {
      label: "parsestr(value)",
      documentation: "Converts value to str.",
      parameters: [{ label: "value", documentation: "Any value." }],
    },
  ],
  parsedecimal: [
    {
      label: "parsedecimal(value)",
      documentation: "Parses value as decimal.",
      parameters: [{ label: "value", documentation: "str, schar, num, or decimal." }],
    },
  ],
  parseschar: [
    {
      label: "parseschar(value)",
      documentation: "Parses value as schar.",
      parameters: [{ label: "value", documentation: "str, num, or schar." }],
    },
  ],
  parseboolean: [
    {
      label: "parseboolean(value)",
      documentation: "Parses value as boolean.",
      parameters: [{ label: "value", documentation: "str or boolean." }],
    },
  ],
  tryparsenum: [
    {
      label: "tryparsenum(value, target, success)",
      documentation: "Tries to parse value as num into target and sets success.",
      parameters: [
        { label: "value", documentation: "Value to parse." },
        { label: "target", documentation: "num variable to write." },
        { label: "success", documentation: "boolean variable set to true or false." },
      ],
    },
  ],
  tryparsedecimal: [
    {
      label: "tryparsedecimal(value, target, success)",
      documentation: "Tries to parse value as decimal into target and sets success.",
      parameters: [
        { label: "value", documentation: "Value to parse." },
        { label: "target", documentation: "decimal variable to write." },
        { label: "success", documentation: "boolean variable set to true or false." },
      ],
    },
  ],
  tryparseschar: [
    {
      label: "tryparseschar(value, target, success)",
      documentation: "Tries to parse value as schar into target and sets success.",
      parameters: [
        { label: "value", documentation: "Value to parse." },
        { label: "target", documentation: "schar variable to write." },
        { label: "success", documentation: "boolean variable set to true or false." },
      ],
    },
  ],
  tryparseboolean: [
    {
      label: "tryparseboolean(value, target, success)",
      documentation: "Tries to parse value as boolean into target and sets success.",
      parameters: [
        { label: "value", documentation: "Value to parse." },
        { label: "target", documentation: "boolean variable to write." },
        { label: "success", documentation: "boolean variable set to true or false." },
      ],
    },
  ],
  __http_request: [
    {
      label: "__http_request(method, url)",
      documentation: "Sends a GET request. Returns a numeric handle.",
      parameters: [
        { label: "method", documentation: "HTTP method as str, for example \"GET\"." },
        { label: "url", documentation: "Request URL as str." },
      ],
    },
    {
      label: "__http_request(method, url, body, contentType)",
      documentation: "Sends a POST request. Returns a numeric handle.",
      parameters: [
        { label: "method", documentation: "Must be \"POST\"." },
        { label: "url", documentation: "Request URL as str." },
        { label: "body", documentation: "Request body as str." },
        { label: "contentType", documentation: "Content-Type as str, for example \"application/json\"." },
      ],
    },
  ],
  __http_status: [
    {
      label: "__http_status(handle)",
      documentation: "Returns the HTTP status code of a response handle.",
      parameters: [{ label: "handle", documentation: "num handle from __http_request." }],
    },
  ],
  __http_body: [
    {
      label: "__http_body(handle)",
      documentation: "Returns the response body as str.",
      parameters: [{ label: "handle", documentation: "num handle from __http_request." }],
    },
  ],
  __http_header: [
    {
      label: "__http_header(handle, name)",
      documentation: "Returns a response header, or an empty str if missing.",
      parameters: [
        { label: "handle", documentation: "num handle from __http_request." },
        { label: "name", documentation: "Header name as str." },
      ],
    },
  ],
  __http_dispose: [
    {
      label: "__http_dispose(handle)",
      documentation: "Releases a response handle.",
      parameters: [{ label: "handle", documentation: "num handle from __http_request." }],
    },
  ],
  __json_serialize: [
    {
      label: "__json_serialize(value)",
      documentation: "Serializes an Alana value (including class instances) to a JSON str.",
      parameters: [{ label: "value", documentation: "Any serializable value." }],
    },
  ],
};

export const NATIVES = [
  "__http_request",
  "__http_status",
  "__http_body",
  "__http_header",
  "__http_dispose",
  "__json_serialize",
];

export const NATIVE_DOCS: Record<string, string> = {
  __http_request: "Native HTTP request. GET takes (str method, str url). POST takes (str method, str url, str body, str contentType). Returns a num handle.",
  __http_status: "Native. Returns the HTTP status code for a response handle.",
  __http_body: "Native. Returns the HTTP response body as str.",
  __http_header: "Native. Returns an HTTP header as str, or \"\" if it is missing.",
  __http_dispose: "Native. Releases an HTTP response handle.",
  __json_serialize: "Native. Serializes a value to a JSON str. Public fields of class instances are included.",
};

export const KEYWORD_LIST = Object.keys(KEYWORDS);

export function isTypeName(name: string): boolean {
  return (TYPES as readonly string[]).includes(name);
}

export function displayType(kind: string, text?: string): string {
  switch (kind) {
    case "NUM":
      return "num";
    case "STR":
      return "str";
    case "DECIMAL":
      return "decimal";
    case "CHAR":
      return "schar";
    case "BOOL":
      return "boolean";
    case "ANY":
      return "any";
    case "VAR":
      return text ?? "any";
    default:
      return text ?? kind.toLowerCase();
  }
}
