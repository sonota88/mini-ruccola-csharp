using System;
using System.Collections.Generic;

class Parser {

    List<Token> Tokens;
    int Pos;

    public Parser() {
        Tokens = new List<Token>();
        Pos = 0;
    }

    public void Run() {
        ReadTokens();
        NodeList ast = Parse();
        Json.Print(ast);
    }

    // --------------------------------

    private Node ParseArg() {
        Token t = Peek();

        switch (t.Kind) {
            case TokenKind.IDENT:
                Bump();
                return new Node(t.Str);
            case TokenKind.INT:
                Bump();
                return new Node(Utils.ParseInt(t.Str));
            default:
                throw Utils.Panic();
        }
    }

    private NodeList ParseArgs() {
        NodeList args = new NodeList();

        if (Peek().Str == ")") {
            return args;
        }

        args.Add(ParseArg());

        while (Peek().Str == ",") {
            Consume(",");
            args.Add(ParseArg());
        }

        return args;
    }

    private Node ParseExprFactor() {
        Token t = Peek();

        switch (t.Kind) {
            case TokenKind.INT:
                Bump();
                return new Node(Utils.ParseInt(t.Str));
            case TokenKind.IDENT:
                Bump();
                return new Node(t.Str);
            case TokenKind.SYM:
                Consume("(");
                Node expr = ParseExpr();
                Consume(")");
                return expr;
            default:
                throw Utils.Panic();
        }
    }

    private bool IsBinOp(Token t) {
        return (
                t.Str == "+"
                || t.Str == "*"
                || t.Str == "=="
                || t.Str == "!="
                );
    }

    private Node ParseExpr() {
        Node expr = ParseExprFactor();

        while (IsBinOp(Peek())) {
            string op = Peek().Str;
            Bump();

            Node rhs = ParseExprFactor();

            NodeList list = new NodeList();
            list.Add(op);
            list.Add(expr);
            list.Add(rhs);
            expr = new Node(list);
        }

        return expr;
    }

    private NodeList ParseReturn() {
        NodeList stmt = new NodeList();
        stmt.Add("return");

        Consume("return");

        if (Peek().Str != ";") {
            stmt.Add(ParseExpr());
        }
        Consume(";");

        return stmt;
    }

    private NodeList ParseSet() {
        Consume("set");

        string varName = Peek().Str;
        Bump();

        Consume("=");

        Node expr = ParseExpr();

        Consume(";");

        NodeList stmt = new NodeList();
        stmt.Add("set");
        stmt.Add(varName);
        stmt.Add(expr);
        return stmt;
    }

    private NodeList ParseFuncall() {
        string fnName = Peek().Str;
        Bump();

        Consume("(");
        NodeList args = ParseArgs();
        Consume(")");

        NodeList funcall = new NodeList();
        funcall.Add(fnName);
        funcall.AddAll(args);
        return funcall;
    }

    private NodeList ParseCall() {
        Consume("call");

        NodeList funcall = ParseFuncall();

        Consume(";");

        NodeList stmt = new NodeList();
        stmt.Add("call");
        stmt.Add(funcall);
        return stmt;
    }

    private NodeList ParseCallSet() {
        Consume("call_set");

        string varName = Peek().Str;
        Bump();

        Consume("=");

        NodeList funcall = ParseFuncall();

        Consume(";");

        NodeList stmt = new NodeList();
        stmt.Add("call_set");
        stmt.Add(varName);
        stmt.Add(funcall);
        return stmt;
    }

    private NodeList ParseWhile() {
        Consume("while");

        Consume("(");
        Node cond = ParseExpr();
        Consume(")");

        Consume("{");
        NodeList stmts = ParseStmts();
        Consume("}");

        NodeList stmt = new NodeList();
        stmt.Add("while");
        stmt.Add(cond);
        stmt.Add(stmts);
        return stmt;
    }

    private NodeList _ParseWhenClause() {
        Consume("when");

        Consume("(");
        Node expr = ParseExpr();
        Consume(")");

        Consume("{");
        NodeList stmts = ParseStmts();
        Consume("}");

        NodeList whenClause = new NodeList();
        whenClause.Add(expr);
        whenClause.AddAll(stmts);
        return whenClause;
    }

    private NodeList ParseCase() {
        Consume("case");

        NodeList whenClauses = new NodeList();

        while (Peek().Str == "when") {
            NodeList whenClause = _ParseWhenClause();
            whenClauses.Add(whenClause);
        }

        NodeList stmt = new NodeList();
        stmt.Add("case");
        stmt.AddAll(whenClauses);
        return stmt;
    }

    private NodeList ParseVmComment() {
        Consume("_cmt");
        Consume("(");

        string comment = Peek().Str;
        Bump();

        Consume(")");
        Consume(";");

        NodeList stmt = new NodeList();
        stmt.Add("_cmt");
        stmt.Add(comment);
        return stmt;
    }

    private NodeList ParseDebug() {
        Consume("_debug");
        Consume("(");
        Consume(")");
        Consume(";");

        NodeList stmt = new NodeList();
        stmt.Add("_debug");
        return stmt;
    }

    private NodeList ParseStmt() {
        switch (Peek().Str) {
            case "return"  : return ParseReturn();
            case "set"     : return ParseSet();
            case "call"    : return ParseCall();
            case "call_set": return ParseCallSet();
            case "while"   : return ParseWhile();
            case "case"    : return ParseCase();
            case "_cmt"    : return ParseVmComment();
            case "_debug"  : return ParseDebug();
            default:
                throw Utils.Panic();
        }
    }

    private NodeList ParseStmts() {
        NodeList stmts = new NodeList();
        while (Peek().Str != "}") {
            stmts.Add(ParseStmt());
        }

        return stmts;
    }

    private NodeList ParseVar() {
        NodeList stmt = new NodeList();
        stmt.Add("var");

        Consume("var");

        string varName = Peek().Str;
        Bump();

        stmt.Add(varName);

        if (Peek().Str == "=") {
            Consume("=");
            Node expr = ParseExpr();
            stmt.Add(expr);
        }

        Consume(";");

        return stmt;
    }

    private NodeList ParseFuncDef() {
        NodeList fnDef = new NodeList();
        Consume("func");

        string fnName = Peek().Str;
        Bump();

        Consume("(");
        NodeList args = ParseArgs();
        Consume(")");

        Consume("{");

        NodeList stmts = new NodeList();
        while (Peek().Str != "}") {
            if (Peek().Str == "var") {
                stmts.Add(ParseVar());
            } else {
                stmts.Add(ParseStmt());
            }
        }

        Consume("}");

        fnDef.Add("func");
        fnDef.Add(fnName);
        fnDef.Add(args);
        fnDef.Add(stmts);

        return fnDef;
    }

    private NodeList ParseTopStmt() {
        return ParseFuncDef();
    }

    private NodeList ParseTopStmts() {
        NodeList topStmts = new NodeList();
        topStmts.Add("top_stmts");

        while (!IsEnd()) {
            topStmts.Add(ParseTopStmt());
        }

        return topStmts;
    }

    private NodeList Parse() {
        return ParseTopStmts();
    }

    // --------------------------------

    private void Consume(string str) {
        if (Peek().Str == str) {
            Bump();
        } else {
            Utils.Puts_e($"expected ({str}) / actual ({Peek().Str})");
            throw Utils.Panic();
        }
    }

    private Token Peek() {
        return Peek(0);
    }

    private Token Peek(int offset) {
        return Tokens[Pos + offset];
    }

    private bool IsEnd() {
        return Pos >= Tokens.Count;
    }

    private void Bump() {
        this.Pos++;
    }

    private void ReadTokens() {
        string src = Utils.ReadStdInAll();
        string rest = src;

        while (true) {
            int i = rest.IndexOf("\n");
            string line;
            if (i >= 0) {
                line = rest.Substring(0, i);
                rest = rest.Substring(i + 1);
                NodeList list = Json.Parse(line);
                Token t = Token.FromList(list);
                Tokens.Add(t);
            } else {
                break;
            }
        }
    }

}
