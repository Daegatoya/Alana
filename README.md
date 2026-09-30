# Alana
![Alana Logo](alana.ico)
### Write what you mean.
> Check out the official [Alana website](https://csalana.com)
<hr>

**Introducing Alana**, a simple programming language I started coding for my portfolio.
Alana source files use the .alana extension and there's a command-line interface to execute commands.

When launching alana.bat (or simply adding it to your environment variables, which will be done automatically with the Inno installer when I make it), you will simply need to type:

`alana <filename>.alana`

Then, if your file is well written and doesn't contain errors, your program will launch in the console.

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

## But how do we use the language?

Good question! Here is a little list showing you all implemented features so far:

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
- [ ] Loops
- [ ] Making you a sandwich (now that I think about it, I don't think I'll add this feature...)

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

</details>

### Arithmetics
<details>
    
| Syntax | Description |
|------|-------------|
| `+` | Addition |
| `-` | Subtraction |
| `times` | Multiplication |
| `divide` | Division |

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

The language uses as many English and simple terms as possible to be able to use it quickly and easily.

The return type of a function can **NOT** be declared. Therefore, it's up to you to make sure the variable you assign the return value to is the right type.

<hr>

## So, where's the example?

**Right here friends!**

Here are some quick codes in .alana:

### *Basic arithmetic and variables/functions*
<details>
    
```txt
define num x is 10#
define decimal y is 3.3#

define func division(num x, decimal y)#
  return x divide y#
end#

define decimal z is call division(x, y)#
showln z#
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

### *Complex code using variables, functions, scopes, conditions, calls, arrays, and recursion*
<details>
    
```txt
define num base is 10#
define decimal multiplier is 2.5#
define str language is "Alana"#
define schar symbol is 'A'#
define boolean active is true#

define decimal lastReportValue is new#
define num processCount is 0#

define num[] history is []#

define func calculate(num x, num y)#
    define num result is x + y#

    if not result lessthan 20#
        return result#
    elseif result sameas 15#
        return result + 5#
    else#
        return 0#
    end#
end#

define func process(num value)#
    define num bonus is 15#
    define num calculated is call calculate(value, bonus)#

    redefine processCount is processCount + 1#

    if active and calculated greaterthan 20#
        define num extra is new#

        redefine extra is calculated + 10#
        redefine calculated is extra#

        push history with calculated#

        return calculated#
    else#
        push history with calculated#

        return calculated#
    end#
end#

define func countdown(num value)#
    showln value#

    if value greaterthan 1#
        return call countdown(value - 1)#
    else#
        return 0#
    end#
end#

define func showHistory(num[] values)#
    showln length values#

    if length values greaterthan 0#
        showln values[0]#
    end#
end#

define func report(str name, num value)#
    define num processed is call process(value)#
    define decimal finalValue is new#

    redefine finalValue is processed times multiplier#
    redefine lastReportValue is finalValue#

    if not active or finalValue lessthan 50#
        showln "Processing inactive or too small"#
    else#
        showln name#
        showln finalValue#
        showln language#
        showln symbol#
    end#

    return finalValue#
end#

show "Enter your name: "#
define str userName is readln#

show "Hello "#
showln userName#

show "Enter a number: "#
define num userValue is parsenum(readln)#

show "Parsed value: "#
showln userValue#

define num result is call calculate(base, userValue)#
define decimal reportResult is call report("Calculation result:", result)#

showln result#
showln reportResult#

call countdown(5)#

showln call process(50)#

showln lastReportValue#
showln processCount#

showln length history#
showln history[0]#
showln history[1]#

redefine history[0] is 100#
showln history[0]#

push history with 200#
showln length history#

pop history#
showln length history#

call showHistory(history)#

define str processCountText is parsestr(processCount)#

show "Process count as string: "#
showln processCountText#

show "Press any key to finish: "#
define schar exitKey is readk#

showln#
show "You pressed: "#
showln exitKey#

showln "Done."#
```

</details>

<hr>

## So what's next?

The next major features I plan to work on are loops. Once this is implemented, the core language should be in a pretty solid state.

After that, I will focus on cleaning up the codebase, improving error handling (which I already started doing, sneak peek in the lexer!), adding more tests and documentation, and eventually creating a full installer so you can try Alana without having to build or debug the entire source code yourself.

And, of course, Alana will probably keep evolving as I come up with more ideas.

<hr>

I hope this little project found your heart, and I will keep you guys updated! Thank you for following my work!

*And don't forget to ⭐ the repo!*

> Copyright © Daegatoya - 2026 | Alana v0.5
