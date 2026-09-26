# Alana
![Alana Logo](alana.ico)
### Simple language for people who love simple things
<hr>

**Introducing Alana**, a simple programming language I started coding for my portfolio.
Alana source files use the .alana extension and there's a built-in terminal to execute commands.

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
- AST
- Parser
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
- [ ] Arrays
- [ ] Loops
- [ ] Making you a sandwich (now that I think about it, I don't think I'll add this feature...)

In the table below, you can see all **variable types** included in the language:

| Type | Description | Example |
|------|-------------|---------|
| `num` | Integer | `25` |
| `decimal` | Decimal number | `12.5` |
| `str` | String | `"Hello"` |
| `schar` | Singular Character | `'A'` |
| `boolean` | Boolean | `true` |

So, the syntax goes as follows:

### Basic keywords
<details>
    
| Syntax | Description |
|------|-------------|
| `define` | Define a variable |
| `is` | Assignation |
| `show` | Function to print text or values |
| `func` | Definition of a function |
| `end` | Ending a function / if statement |
| `call` | Calling a function |
| `#` | Endline character |
| `return` | Return a value |

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

define func division(num x, decimal y)
  return x divide y#
end#

define decimal z is call division(x, y)#
show z#
```

    
</details>

### *Playing with all variable types*
<details>
    
```txt
define str x is "Hello"#
define schar c is '3'#
define str y is "World"#
define boolean zeroOrOne is true#

show zeroOrOne#
show x#
show c#
show y#
```

</details>

### *If/elseif/else statements*
<details>
    
```txt
define num x is 50#

if x sameas 40 or x sameas 30#
    show true#
elseif x greaterthan 60 and x lessthan 70#
    show true#
else#
    show false#
end#
```

</details>

### *Comparisons*
<details>
    
```txt
define num x is 50#
define num y is 60#

if x sameas y#
    show x#
elseif x greaterthan y#
    show x#
elseif x lessthan y#
    show x#
elseif x leorsame y#
    show x#
elseif x grorsame y#
    show x#
elseif not x sameas y#
    show x#
else#
    show y#
end#
```

</details>

### *Functions*
<details>
    
```txt
define func compare(num y, decimal z)
    if y sameas z#
        return true#
    else#
        return false#
    end#
end#

define boolean isTheSame is call compare(50, 50.0)#
show isTheSame#
```

</details>

### *Complex code using variables, functions, scopes, conditions, calls, and recursion*
<details>
    
```txt
define num base is 10#
define decimal multiplier is 2.5#
define str language is "Alana"#
define schar symbol is 'A'#
define boolean active is true#

define func calculate(num x, num y)
    define num result is x + y#

    if not result lessthan 20#
        return result#
    elseif result sameas 15#
        return result + 5#
    else#
        return 0#
    end#
end#

define func process(num value)
    define num bonus is 15#
    define num calculated is call calculate(value, bonus)#

    if active and calculated greaterthan 20#
        define num extra is calculated + 10#
        return extra#
    else#
        return calculated#
    end#
end#

define func countdown(num value)
    show value#

    if value greaterthan 1#
        return call countdown(value - 1)#
    else#
        return 0#
    end#
end#

define func report(str name, num value)
    define num processed is call process(value)#
    define decimal finalValue is processed times multiplier#

    if not active or finalValue lessthan 50#
        show "Processing inactive or too small"#
    else#
        show name#
        show finalValue#
        show language#
        show symbol#
    end#

    return finalValue#
end#

define num result is call calculate(base, 20)#
define decimal reportResult is call report("Calculation result:", result)#

show result#
show reportResult#

call countdown(5)#

show call process(50)#
```

</details>

<hr>

## So what's next?

Alana can now handle variables, functions, local scopes, recursion, comparisons, boolean logic, and full if / elseif / else conditional statements.

The next major features I plan to work on are arrays and loops. Once those are implemented, the core language should be in a pretty solid state.

After that, I will focus on cleaning up the codebase, improving error handling (which I already started doing, sneak peek in the parser!), adding more tests and documentation, and eventually creating a full installer so you can try Alana without having to build or debug the entire source code yourself.

And, of course, Alana will probably keep evolving as I come up with more ideas.

<hr>

I hope this little project found your heart, and I will keep you guys updated! Thank you for following my work!

*And don't forget to ⭐ the repo!*

> Copyright © Daegatoya - 2026 | Alana v0.3
