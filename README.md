# BasicSharp
- Just another BASIC-inspired language interpreter written in C#
- C# code documentation can be found in [DEV_DOCS.md](DEV_DOCS.md)
- Available at: [https://github.com/gamecraftCZ/BasicSharp](https://github.com/gamecraftCZ/BasicSharp)
- Source code file should be encoded in UTF-8

### Usage
`basicsharp <source_code_file>`

### Example code
```basic
REM Personalised welcome script

INPUT "Whats your name: ", name
INPUT "How old are you: ", age
TONUM age

IF age < 12 THEN
  PRINT "Hi kid!"
ELSE
  PRINT "Welcome " + name
END
```

- More examples in `examples/` directory


### Variable names
- Can contain only english alphabet and underscore `a-zA-Z_`
- Must not collide with builtin keywords
- Case sensitive


### Variable types
- String: variable length UTF-8 string
- Number: IEEE double-precision floating point number (64 bit)
- Boolean: true / false


### Variable assigment
Variable are declared and assigned using `LET` keyword
- eg. `LET num = 14`
- eg. `LET VarOne = "Age: " + 14`


- You must use let even when assigning to already existing variable.
- There is no such value as null. Every variable must have a value.

### Operations with types

- Number
    - Comparing `<`,`>`,`<=`,`>=`,`==`,`<>`
        - `<number> OP <number> -> <boolean>`
        - eg. `7 > 5 -> true`

    - Arithmetic `+`,`-`,`*`,`/`
        - `<number> OP <number> -> <number>`
        - eg. `7 / 2 -> 3.5`
        - Division by zero results in Infinity or -Infinity
        - 0/0 results in NaN

    - Negation `-`
        - `- <number> -> <number>`
        - eg. `-7 -> -7`

- String
    - Joining `+`
        - `<string> + <string> -> <string>`
        - `<string> + <number> -> <string>`
        - `<string> + <boolean> -> <string>`
        - `<number> + <string> -> <string>`
        - `<boolean> + <string> -> <string>`
        - eg. `"I am from" + "Pilsen" -> "I am from Pilsen"`
        - eg. `"I am " + 7 + "years old." -> "I am 7 years old."`
        - eg. `"This statement is " + True -> "This statement is TRUE"`

    - Comparing `==`, `<>`
        - `<string> OP <string> -> <boolean>`
        - eg. `"Hello" == "hello" -> false`
        - Comparing is case-sensitive

- Boolean
    - Negation `NOT `
        - `NOT <boolean> -> <boolean>`
        - eg. `NOT true -> false`

    - Logic operators `AND`,`OR`
        - `<boolean> OP <boolean> -> <boolean>`
        - eg. `false OR 1 > 0 AND true -> true`


### Operator precedence

- You can group expressions using `(` and `)`
- Precedence is from highest to lowest
    1. `()` - grouping
    2. `-` - negation
    3. `/`,`*`
    4. `-`,`+`
    5. `<`,`>`,`<=`,`>=`,`==`,`<>`
    6. `NOT`
    7. `AND`
    8. `OR`

### Language functionality

- Keywords are case-insensitive

- There is no such value as null. Every variable must have a value.

- You can use explicit semicolon `;` to separate statements/expressions explicitly. 
   Otherwise expressions can span multiple lines.

#### Input / Output
- `PRINT <expr>`
    - Evaluates and prints expr to stdout with new line at the end
    - \<expr\> can be `string`, `number`, `boolean`
    - `number` is printed to two decimal places if decimal, otherwise it gets printed without decimal places if whole.
    - eg. `PRINT "Age: " + 7`
- `INPUT <expr>, var`
    - Evaluates and prints expr to stdout and reads user input to variable var
    - \<expr\> can be `string`, `number`, `boolean`
    - eg. `INPUT "Age: ", age`

#### Control flow

- `IF`
    - Executes `expr`, if result is true, execute expressions in `then_branch`, else execute expressions in `else_branch` if exists.
    - Else branch is optional
    - Branches can contain multiple expressions
    - Usage:
  ```basic
  IF expr THEN
    ...then_branch...
  (ELSE
    ...else_branch...)
  END
  ```
    - eg.
  ```basic
  IF 5 < 7 THEN
    PRINT "Hello"
    PRINT "Low number"
  ELSE
    PRINT "Else never occures"
  END
  ```

- `WHILE`
    - Execute `code` until `expr` evaluates to true
    - Usage:
  ```basic
  WHILE expr DO
    code
  END 
  ```
    - eg.
  ```basic
  LET i = 0
  WHILE i < 10 DO
    PRINT "Hello for " + i + " time."
    LET i = i + 1
  END
  ```

- `BREAK`
    - Stops execution of `code` and exits the closest `WHILE` loop.
    - `LoopControlOutsideLoop` error may occur if not inside a loop.

- `CONTINUE`
    - Stops execution of `code` and continues at the beginning of the closest `WHILE` loop.
    - `LoopControlOutsideLoop` error may occur if not inside a loop.

#### Other
- `REM comment`
    - Comment. Anything after `REM` on that line is ignored.

- `TONUM var(, out_var)`
    - Converts `var` to number and saves the result to `out_var`
    - If only `var` is defined, output is saved back to `var`
    - `InvalidNumberFormat` error may occur if conversion failed

- `TOSTR var(, out_var)`
    - Converts `var` to string and saves the result to `out_var`
    - If only `var` is defined, output is saved back to `var`

- `RND var, lowerBound, upperBound`
    - Generates random integer in range [lowerBound, upperBound) including lowerBound, excluding upperBound.
    - Stores result to `var`


### Runtime errors
Things that can result in runtime error:

- Using `continue` or `break` outside of loop body
- Parsing string that is not a number using `TONUM`
- Using variable that was not declared before
- Condition in `IF` or `WHILE` evaluated to non-boolean value
