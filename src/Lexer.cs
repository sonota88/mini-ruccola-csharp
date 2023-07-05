using System;

public class Lexer
{
    public void Run()
    {
        string src = Utils.ReadStdInAll();

        int pos = 0;
        var re = new RegExp();
        int lineno = 1;

        while (pos < src.Length) {
            string rest = src.Substring(pos);
            string c0 = rest.Substring(0, 1);

            if (c0 == " ") {
                pos++;
            } else if (c0 == "\n") {
                lineno++;
                pos++;
            } else if (c0 == "\"") {
                re.Match(rest, "^\"(.*?)\"");
                string str = re.Group(1);
                PrintToken("str", str, lineno);
                pos += str.Length + 2;
            } else if (re.Match(rest, "^(//.*)")) {
                string str = re.Group(1);
                pos += str.Length;
            } else if (re.Match(rest, "^(==|!=|[(){};=,+*])")) {
                string str = re.Group(1);
                PrintToken("sym", str, lineno);
                pos += str.Length;
            } else if (re.Match(rest, "^(-?[1-9][0-9]*|0)")) {
                string str = re.Group(1);
                PrintToken("int", str, lineno);
                pos += str.Length;
            } else if (re.Match(rest, "^([a-z_][a-z0-9_]*)")) {
                string str = re.Group(1);
                if (IsKw(str)) {
                    PrintToken("kw", str, lineno);
                } else {
                    PrintToken("ident", str, lineno);
                }
                pos += str.Length;
            } else {
                throw Utils.Panic();
            }
        }
    }

    private bool IsKw(string str)
    {
        return (
                str == "func"
                || str == "return"
                || str == "var"
                || str == "set"
                || str == "call"
                || str == "call_set"
                || str == "while"
                || str == "case"
                || str == "when"
                || str == "_cmt"
                || str == "_debug"
                );
    }

    private void PrintToken(string kind, string str, int lineno)
    {
        Utils.Puts($"[{lineno}, \"{kind}\", \"{str}\"]");
    }
}
