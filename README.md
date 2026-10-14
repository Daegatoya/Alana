# Alana v1.2
<img src="alana.png" alt="Alana Logo" width="500">

> Check out the official [Alana website](https://csalana.com)
<hr>

**Introducing Alana**, a programming language I started coding for my portfolio.
Alana source files use the .alana extension and there's a command-line interface to execute commands.

The language has been built using C# dotnet 8.0. The program to launch the language is divided between a class library (.DLL) and a C# program using the library.

**The main program file looks like this**:

```cs
namespace Alana
{
    public static class Alana
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: alana <file.alana>");
                    return;
                }
                string file = args[0];
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Alana error: file '{file}' not found.");
                    return;
                }

                string source = File.ReadAllText(file);

                Lexer lexer = new Lexer(source);
                Token[] tokens = lexer.TranslateToken();

                Parser parser = new Parser(tokens);
                List<Statement> statements = parser.Parse();

                Interpreter interpreter = new Interpreter();
                interpreter.Execute(statements);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alana error: {ex.Message}");
            }
        }
    }
}
```

The C# program interprets the language using the following layers, in order:

- Lexer
- Tokens
- Parser
- AST
- Interpreter

<hr>

## How to use Alana

To use Alana, here are the steps:
- First, you must download the installer from the **[Alana official website](https://csalana.com)**
- Then, proceed to the installation.
- Afterwards, you simply need to create a file with the .alana extension on your computer.
- Once this is done, double click it or open any cmd in the folder where the file is located and enter the following command: `alana <filename>.alana`
- Your program should run as expected and show you the appropriate errors in case of a syntax misuse.

<hr>

## And how do we code in Alana?

Good question! Here is a little list showing you all implemented features so far:

### v1.0

<details>
    
- [x] Variables
- [x] Arithmetic
- [x] Functions
- [x] Parameters
- [x] Return values
- [x] Local scopes
- [x] Recursive functions
- [x] Comparisons
- [x] Boolean logic
- [x] If / elseif / else statements
- [x] Arrays
- [x] Loops
- [ ] Making you a sandwich (now that I think about it, I don't think I'll add this feature...)

</details>

### v1.x - v2.0

<details>
    
- [x] Importing files
- [x] Public/private visibility
- [x] Classes
- [x] Fields
- [x] Methods
- [x] "Self" expression
- [x] Object reference
- [ ] Fields access
- [ ] OOP type system
- [ ] Object reassignment
- [ ] Inheritance
- [ ] Override
- [ ] Runtime polymorphism
- [ ] Making you a sandwich (I already said I wouldn't add this feature.)

</details>

In the table below, you can see all **variable types** included in the language:

| Type | Description | Example |
|------|-------------|---------|
| `num` | Integer | `25` |
| `decimal` | Decimal number | `12.5` |
| `str` | String | `"Hello"` |
| `schar` | Single Character | `'A'` |
| `boolean` | Boolean | `true` |
| `[]` | Array | `[10, 20]` |

So, the syntax goes as follows:

### Basic keywords
<details>
    
| Syntax | Description |
|------|-------------|
| `define` | Define a variable |
| `redefine` | Redefine a variable |
| `is` | Assignment |
| `show` | Function to print text or values on same line |
| `showln` | Function to print text or values with a line return |
| `readln` | Reads an entire input of line (returns string) |
| `readk` | Reads a key input (returns char) |
| `func` | Definition of a function |
| `end` | Ending a function / if statement |
| `call` | Calling a function |
| `#` | Endline character |
| `return` | Return a value |
| `new` | Define a new uninitialized variable |
| `newobj` | Define a new object referencing a class |
| `returning` | Defines the return type of a function |

</details>

### Type parsing
<details>
    
| Syntax | Description |
|------|-------------|
| `parsestr` | Parses a variable to str |
| `parsenum` | Parses a variable to num |
| `parsedecimal` | Parses a variable to decimal |
| `parseschar` | Parses a variable to schar |
| `parseboolean` | Parses a variable to boolean |
| `tryparsenum` | Parses a variable to num if the value can be parsed |
| `tryparsedecimal` | Parses a variable to decimal if the value can be parsed |
| `tryparseschar` | Parses a variable to schar if the value can be parsed |
| `tryparseboolean` | Parses a variable to boolean if the value can be parsed |

</details>

### Arithmetics
<details>
    
| Syntax | Description |
|------|-------------|
| `+` | Addition |
| `-` | Subtraction |
| `times` | Multiplication |
| `divide` | Division |
| `mod` | Modulo |

</details>

### If statements
<details>
    
| Syntax | Description |
|------|-------------|
| `if` | Starting if statement |
| `elseif` | Adding a condition if prior statement is false |
| `else` | Executes if all previous conditions are false |

</details>

### Comparisons
<details>
    
| Syntax | Description |
|------|-------------|
| `sameas` | Compares if x is the same as y (==) |
| `greaterthan` | Compares if x is greater than y (>) |
| `lessthan` | Compares if x is less than y (<) |
| `leorsame` | Compares if x is less or same as y (<=) |
| `grorsame` | Compares if x is greater or same as y (>=) |
| `not` | Inverts the boolean result of an expression (!) |

</details>

### Multiple conditions
<details>
    
| Syntax | Description |
|------|-------------|
| `or` | Checks if left OR right expression is true |
| `and` | Checks if left AND right expression is true |

</details>

### Array management
<details>
    
| Syntax | Description |
|------|-------------|
| `length` | Return the length of the array |
| `push` | Add a value at the end of an array |
| `pop` | Remove a value at the end of an array |
| `with` | Defines the value to push |

</details>

### Loops
<details>
    
| Syntax | Description |
|------|-------------|
| `while` | Starts a while loop |
| `for` | Starts a for loop |
| `forevery` | Starts a forevery loop which goes through an array |
| `within` | Defines the array the forevery loop needs to go through |
| `continue` | Continue statement inside a loop |
| `break` | Break statement inside a loop |

</details>

### Object Oriented
<details>
    
| Syntax | Description |
|------|-------------|
| `import` | Imports the file passed as a string |
| `public` | Defines the public fields |
| `private` | Defines the public fields |
| `class` | Defines a class |
| `constructor` | Defines the constructor of a class |
| `self` | Calls a self field of a class |

</details>

The language uses as many English and simple terms as possible to be able to use it quickly and easily.

The return type of a function can be declared if you wish with the `returning` statement.

<hr>

## So, where's the example?

**Right here friends!**

Here are some quick codes in .alana:

### *Basic arithmetic and variables/functions*
<details>
    
```txt
define num x is 10#
define num y is 3#

define func sub(num x, num y) returning num#
  return x - y#
end#

define func add(num x, num y)#
  return x + y#
end#

define num z is call sub(x, y)#
showln z#

define num k is call add(x, y)#
showln k#
```

    
</details>

### *Playing with all variable types*
<details>
    
```txt
define str x is "Hello"#
define schar c is '3'#
define str y is "World"#
define boolean zeroOrOne is true#

define str x2 is new#
define schar c2 is new#
define str y2 is new#
define boolean zeroOrOne2 is new#

redefine x2 is "Hello"#
redefine c2 is '3'#
redefine y2 is "World"#
redefine zeroOrOne2 is true#

showln zeroOrOne#
showln x#
showln c#
showln y#

showln zeroOrOne2#
showln x2#
showln c2#
showln y2#
```

</details>

### *Parsing a variable and reading*
<details>
    
```txt
define num x is parsenum(readln)#
showln x#
readk#
```

</details>

### *Tryparsing a variable and reading*
<details>
    
```txt
define str badParse is "Hello World!"#
define num goodParse is 23#
define num parsing is new#
define boolean success is new#

tryparsenum(badParse, parsing, success)#
showln success#

tryparsenum(goodParse, parsing, success)#
showln success#
showln parsing#
```

</details>

### *Loops*
<details>
    
```txt
define num x is 0#

while x lessthan 5#
    showln x#
    redefine x is x + 1#
end#

showln#

for(define num i is 0, i lessthan 5, redefine i is i + 1)#
    showln i#
end#

showln#

define str[] names is ["Ahsoka", "Yoda", "Kenobi"]#

forevery(define str name within names)#
    showln name#
end#
```

</details>


### *If/elseif/else statements*
<details>
    
```txt
define num x is 50#

if x sameas 40 or x sameas 30#
    showln true#
elseif x greaterthan 60 and x lessthan 70#
    showln true#
else#
    showln false#
end#
```

</details>

### *Comparisons*
<details>
    
```txt
define num x is 50#
define num y is 60#

if x sameas y#
    showln x#
elseif x greaterthan y#
    showln x#
elseif x lessthan y#
    showln x#
elseif x leorsame y#
    show x#
elseif x grorsame y#
    showln x#
elseif not x sameas y#
    showln x#
else#
    showln y#
end#
```

</details>

### *Functions*
<details>
    
```txt
define func compare(num y, decimal z)#
    if y sameas z#
        return true#
    else#
        return false#
    end#
end#

define boolean isTheSame is call compare(50, 50.0)#
showln isTheSame#
```

</details>

### *Arrays using scopes*
<details>
    
```txt
define num[] numbers is [10, 20, 30]#

showln length numbers#

push numbers with 40#
push numbers with 50#

showln length numbers#
showln numbers[3]#
showln numbers[4]#

pop numbers#

showln length numbers#
showln numbers[3]#

define func modify(num[] arr)#
    push arr with 99#
    showln length arr#
    showln arr[length arr - 1]#
    pop arr#
    showln length arr#
end#

call modify(numbers)#

showln length numbers#
showln numbers[3]#
```

</details>

### *Imports and public/private fields*
<details>

**Main.alana**
```txt
import "Addition.alana"#
showln call Add(10, 50)#
```

**Addition.alana**
```txt
private:
    define num x is new#
    define num y is new#

public:
    define func Add(num left, num right)#
        redefine x is left#
        redefine y is right#
        return x + y#
    end#
```

</details>

### *Classes using self, fields, and methods*
<details>

**Main.alana**
```txt
import "Import.alana"#

define Person jacob is newobj Person("Jacob", 21)#
define Person alice is newobj Person("Alice", 30)#

call jacob.Birthday()#
call jacob.Birthday()#
call alice.Birthday()#

showln call jacob.GetName()#
showln call jacob.GetAge()#

showln call alice.GetName()#
showln call alice.GetAge()#
```

**Import.alana**
```txt
private:
define func DefaultName() returning str#
    return "Unknown"#
end#

public:
define class Person#

private:
define str name is call DefaultName()#
define num age is 0#

define func FormatName() returning str#
    return self.name#
end#

public:
define constructor(str name, num age)#
    redefine self.name is name#
    redefine self.age is age#
end#

define func GetName() returning str#
    return call self.FormatName()#
end#

define func GetAge() returning num#
    return self.age#
end#

define func Birthday()#
    redefine self.age is self.age + 1#
end#

end#
```

</details>

<hr>

## So what's next?

I am currently working on making Alana fully OOP, which has already started in v1.1. Stay tuned!

And, of course, Alana will probably keep evolving as I come up with more ideas.

<hr>

I hope this little project found your heart, and I will keep you guys updated! Thank you for following my work!

*And don't forget to ⭐ the repo!*

> Copyright © Daegatoya - 2026 | Alana v1.2
