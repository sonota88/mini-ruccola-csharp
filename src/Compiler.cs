using System;
using System.Diagnostics;

public class Compiler {

    static public void Main(string[] args) {
        if (args.Length >= 1) {
            // ok
        } else {
            throw new Exception("invalid arguments");
        }

        string cmd = args[0];
        switch (cmd) {
        case "lex"      : new Lexer().Run();
            break;
        case "parse"    : new Parser().Run();
            break;
        case "codegen"  : new CodeGenerator().Run();
            break;
        case "test_json": new JsonTester().Run();
            break;
        default:
            throw new Exception("invalid command");
        }
    }

}
