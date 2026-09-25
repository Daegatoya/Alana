# Alana
![Alana Logo](alana.ico)
### Simple language for people who love simple things
<hr>

**Introducing Alana**, a simple programming language I started coding for my portfolio.
The programming language works with the extension .alana and has a built-in console to execute commands.

When launching alana.bat (or simply adding it to your environment variables, which will be done automatically with the Inno installer when I make it), you will simply need to type:

`alana <filename>.alana`

Then, if your file is well written and doesn't contain errors, your program will launch in the console.

The language has been built using C# dotnet 8.0. The program to launch the language is divided between a classes library (.DLL) and a C# program using the library.

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

- A lexer
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
- [ ] Arrays
- [ ] Loops
- [ ] Conditions
- [ ] Making you a sandwich

In the board below, you can see all **variable types** included in the language:

| Type | Description | Example |
|------|-------------|---------|
| `num` | Integer | `25` |
| `decimal` | Decimal number | `12.5` |
| `str` | String | `"Hello"` |
| `schar` | Singular Character | `'A'` |
| `boolean` | Boolean | `true` |

So, the syntax goes as follow:

| Syntax | Description |
|------|-------------|
| `define` | Define a variable |
| `is` | Assignation |
| `+/-/times/divide` | Arithmetic |
| `show` | Function to print text or values |
| `func` | Definition of a function |
| `end` | Ending a function |
| `call` | Calling a function |
| `#` | Endline character |
| `return` | Return a value |

The language uses as many English and simple terms as possible to be able to use it quickly and easily.

The return type of a function is **NOT** declared. Therefore, it's up to you to make sure the variable you assign the return value to is the right type.

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

### *Playing with all type of variables*
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

### *Complex code using local variables, scopes, calls, definitions, etc.*
<details>
    
```txt
define num base is 10#
define decimal multiplier is 2.5#
define str language is "Alana"#
define schar symbol is 'A'#
define boolean active is true#

define func calculate(num x, num y)
    define num sum is x + y#
    return sum#
end#

define func process(num value)
    define num bonus is 15#
    define num result is call calculate(value, bonus)#

    return result#
end#

define func report(str name, num value)
    define num processed is call process(value)#
    define decimal finalValue is processed times multiplier#

    show name#
    show finalValue#
    show language#
    show symbol#
    show active#

    return finalValue#
end#

define func mainCalculation(num x, num y)
    define num intermediate is call calculate(x, y)#
    define num first is call process(intermediate)#
    define num second is call process(intermediate + 5)#

    return first + second#
end#

define num result is call mainCalculation(base, 20)#
define decimal reportResult is call report("Calculation result:", result)#

show result#
show reportResult#
show call process(50)#
```

</details>

<hr>

## So what's next?

I will continue improving the language and add more functionalities. Soon enough, if/else statements should be added. Then, I will be working on arrays and loops and the language should be good to go.

Finally, I will create a full on installer so you can try the language at home without having to debug the entire source code (obviously).

<hr>

I hope this little project found your heart, and I will keep you guys updated! Thank you for following my work!

*And don't forget to ⭐ the repo!*

> Copyright © Daegatoya - 2026
