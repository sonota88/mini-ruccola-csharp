using System;

class CodeGenerator
{
    private int _labelId;

    public CodeGenerator()
    {
        this._labelId = 0;
    }

    public void Run()
    {
        string json = Utils.ReadStdInAll();
        NodeList ast = Json.Parse(json);

        Codegen(ast);
    }

    // --------------------------------

    private void _GenExpr_add()
    {
        Puts("  pop reg_b");
        Puts("  pop reg_a");
        Puts("  add reg_a reg_b");
    }

    private void _GenExpr_mult()
    {
        Puts("  pop reg_b");
        Puts("  pop reg_a");
        Puts("  mul reg_b");
    }

    private void _GenExpr_eq_neq(VarList fnArgs, VarList lvars, Node expr, bool isEq)
    {
        int labelId = NextLabelId();
        string labelThen = $"then_{labelId}";
        string labelEnd;
        if (isEq) {
            labelEnd = $"end_eq_{labelId}";
        } else {
            labelEnd = $"end_neq_{labelId}";
        }

        Puts("  pop reg_b");
        Puts("  pop reg_a");

        Puts("  compare");
        Puts($"  jump_eq {labelThen}");

        if (isEq) {
            Puts("  mov reg_a 0");
        } else {
            Puts("  mov reg_a 1");
        }
        Puts($"  jmp {labelEnd}");

        Puts($"label {labelThen}");
        if (isEq) {
            Puts("  mov reg_a 1");
        } else {
            Puts("  mov reg_a 0");
        }

        Puts($"label {labelEnd}");
    }

    private void _GenExpr_eq(VarList fnArgs, VarList lvars, Node expr)
    {
        _GenExpr_eq_neq(fnArgs, lvars, expr, true);
    }

    private void _GenExpr_neq(VarList fnArgs, VarList lvars, Node expr)
    {
        _GenExpr_eq_neq(fnArgs, lvars, expr, false);
    }

    private void _GenExpr_binary(VarList fnArgs, VarList lvars, Node expr)
    {
        NodeList xs = expr.Listval;
        string op = xs.GetStr(0);
        Node lhs = xs.Get(1);
        Node rhs = xs.Get(2);

        GenExpr(fnArgs, lvars, lhs);
        Puts("  push reg_a");
        GenExpr(fnArgs, lvars, rhs);
        Puts("  push reg_a");

        switch (op) {
            case "+" : _GenExpr_add();
                break;
            case "*" : _GenExpr_mult();
                break;
            case "==": _GenExpr_eq(fnArgs, lvars, expr);
                break;
            case "!=": _GenExpr_neq(fnArgs, lvars, expr);
                break;
            default:
                throw Utils.Panic();
        }
    }

    private void GenExpr(VarList fnArgs, VarList lvars, Node expr)
    {
        switch (expr.Type) {
            case NodeType.INT:
                Puts($"  mov reg_a {expr.Intval}");
                break;
            case NodeType.STR:
                string varName = expr.Strval;
                if (lvars.Includes(varName)) {
                    int disp = LvarDisp(lvars, varName);
                    Puts($"  mov reg_a [bp:{disp}]");
                } else if (fnArgs.Includes(varName)) {
                    int disp = FnArgDisp(fnArgs, varName);
                    Puts($"  mov reg_a [bp:{disp}]");
                } else {
                    Utils.Puts_e(varName);
                    throw Utils.Panic();
                }
                break;
            case NodeType.LIST:
                _GenExpr_binary(fnArgs, lvars, expr);
                break;
            default:
                throw Utils.Panic();
        }
    }

    private void GenReturn(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        if (stmt.Count == 2) {
            Node expr = stmt.Get(1);
            GenExpr(fnArgs, lvars, expr);
        }

        AsmEpilogue();
        Puts("  ret");
    }

    private void _GenSet(VarList fnArgs, VarList lvars, string varName, Node expr)
    {
        GenExpr(fnArgs, lvars, expr);

        if (lvars.Includes(varName)) {
            int disp = LvarDisp(lvars, varName);
            Puts($"  mov [bp:{disp}] reg_a");
        } else {
            Utils.Puts_e(varName);
            Utils.Puts_e(lvars.ToString());
            throw Utils.Panic();
        }
    }

    private void GenSet(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        string varName = stmt.GetStr(1);
        Node expr = stmt.Get(2);

        _GenSet(fnArgs, lvars, varName, expr);
    }

    private void _GenCall(VarList fnArgs, VarList lvars, NodeList funcall)
    {
        string funcallName = funcall.GetStr(0);
        NodeList funcallArgs = funcall.Rest();

        for (int i = funcallArgs.Count - 1; i >= 0; i--) {
            GenExpr(fnArgs, lvars, funcallArgs.Get(i));
            Puts("  push reg_a");
        }

        _GenVmComment($"call  {funcallName}");
        Puts($"  call {funcallName}");

        Puts($"  add sp {funcallArgs.Count}");
    }

    private void GenCall(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        NodeList funcall = stmt.GetList(1);
        _GenCall(fnArgs, lvars, funcall);
    }

    private void GenCallSet(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        string varName = stmt.GetStr(1);
        NodeList funcall = stmt.GetList(2);

        _GenCall(fnArgs, lvars, funcall);

        int disp = LvarDisp(lvars, varName);
        Puts($"  mov [bp:{disp}] reg_a");
    }

    private void GenWhile(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        Node condExpr = stmt.Get(1);
        NodeList stmts = stmt.GetList(2);

        int labelId = NextLabelId();

        string labelBegin = $"while_{labelId}";
        string labelEnd = $"end_while_{labelId}";

        Puts("");

        Puts($"label {labelBegin}");

        GenExpr(fnArgs, lvars, condExpr);
        Puts("  mov reg_b 0");
        Puts("  compare");

        Puts($"  jump_eq {labelEnd}");

        GenStmts(fnArgs, lvars, stmts);

        Puts($"  jmp {labelBegin}");

        Puts($"label {labelEnd}");
        Puts("");
    }

    private void GenCase(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        NodeList whenClauses = stmt.Rest();

        int labelId = NextLabelId();
        string labelEnd = $"end_case_{labelId}";
        string labelEndWhenHead = $"end_when_{labelId}";

        int whenIdx = -1;
        foreach (Node _whenClause in whenClauses.Data) {
            NodeList whenClause = _whenClause.Listval;
            whenIdx++;

            Node cond = whenClause.Get(0);
            NodeList stmts = whenClause.Rest();

            GenExpr(fnArgs, lvars, cond);
            Puts("  mov reg_b 0");
            Puts("  compare");

            Puts($"  jump_eq {labelEndWhenHead}_{whenIdx}");

            GenStmts(fnArgs, lvars, stmts);

            Puts($"  jmp {labelEnd}");

            Puts($"label {labelEndWhenHead}_{whenIdx}");
        }

        Puts($"label {labelEnd}");
    }

    private void _GenVmComment(string comment)
    {
        string _comment = comment.Replace(" ", "~");
        Puts($"  _cmt {_comment}");
    }

    private void GenVmComment(NodeList stmt)
    {
        string comment = stmt.GetStr(1);
        _GenVmComment(comment);
    }

    private void GenStmt(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        switch (stmt.GetStr(0)) {
            case "return"  : GenReturn( fnArgs, lvars, stmt);
                break;
            case "set"     : GenSet(    fnArgs, lvars, stmt);
                break;
            case "call"    : GenCall(   fnArgs, lvars, stmt);
                break;
            case "call_set": GenCallSet(fnArgs, lvars, stmt);
                break;
            case "while"   : GenWhile(  fnArgs, lvars, stmt);
                break;
            case "case"    : GenCase(   fnArgs, lvars, stmt);
                break;
            case "_cmt"    : GenVmComment(stmt);
                break;
            default:
                throw Utils.Panic();
        }
    }

    private void GenStmts(VarList fnArgs, VarList lvars, NodeList stmts)
    {
        foreach (Node node in stmts.Data) {
            NodeList stmt = node.Listval;
            GenStmt(fnArgs, lvars, stmt);
        }
    }

    private void GenVar(VarList fnArgs, VarList lvars, NodeList stmt)
    {
        Puts("  add sp -1");

        if (stmt.Count == 3) {
            string varName = stmt.GetStr(1);
            Node expr = stmt.Get(2);
            _GenSet(fnArgs, lvars, varName, expr);
        }
    }

    private void GenFuncDef(NodeList funcDef)
    {
        string fnName = funcDef.GetStr(1);
        VarList fnArgs = VarList.From(funcDef.GetList(2));
        NodeList stmts = funcDef.GetList(3);

        var lvars = new VarList();

        Puts($"label {fnName}");
        AsmPrologue();

        foreach (Node node in stmts.Data) {
            NodeList stmt = node.Listval;
            
            string head = stmt.GetStr(0);
            if (head == "var") {
                string varName = stmt.GetStr(1);
                lvars.Add(varName);
                GenVar(fnArgs, lvars, stmt);
            } else {
                GenStmt(fnArgs, lvars, stmt);
            }
        }

        AsmEpilogue();
        Puts("  ret");
    }

    private void GenTopStmt(NodeList topStmt)
    {
        GenFuncDef(topStmt);
    }

    private void GenTopStmts(NodeList topStmts)
    {
        foreach (Node node in topStmts.Data) {
            NodeList topStmt = node.Listval;
            GenTopStmt(topStmt);
        }
    }

    private void GenBuiltinSetVram()
    {
        Puts("");
        Puts("label set_vram");
        AsmPrologue();
        Puts("  set_vram [bp:2] [bp:3]"); // vram_addr value
        AsmEpilogue();
        Puts("  ret");
    }

    private void GenBuiltinGetVram()
    {
        Puts("");
        Puts("label set_vram");
        AsmPrologue();
        Puts("  set_vram [bp:2] reg_a"); // vram_addr dest
        AsmEpilogue();
        Puts("  ret");
    }

    private void Codegen(NodeList ast)
    {
        Puts("  call main");
        Puts("  exit");

        GenTopStmts(ast.Rest());

        Puts("#>builtins");
        GenBuiltinSetVram();
        GenBuiltinGetVram();
        Puts("#<builtins");
    }

    // --------------------------------

    private int FnArgDisp(VarList fnArgs, String fnArgName)
    {
        int i = fnArgs.IndexOf(fnArgName);
        return i + 2;
    }

    private int LvarDisp(VarList lvars, String lvarName)
    {
        int i = lvars.IndexOf(lvarName);
        return -(i + 1);
    }

    private void AsmPrologue()
    {
        Puts("  push bp");
        Puts("  mov bp sp");
    }

    private void AsmEpilogue()
    {
        Puts("  mov sp bp");
        Puts("  pop bp");
    }

    private void Puts(object arg)
    {
        Utils.Puts(arg);
    }

    int NextLabelId()
    {
        this._labelId++;
        return this._labelId;
    }
}
