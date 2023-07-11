using System;
using System.Collections.Generic;

enum NodeType
{
    INT, STR, LIST
}

class Node
{
    public NodeType Type;
    public int _intval;
    public string _strval;
    public NodeList _listval;

    public Node(int n)
    {
        this.Type = NodeType.INT;
        this._intval = n;
    }

    public Node(string str)
    {
        this.Type = NodeType.STR;
        this._strval = str;
    }

    public Node(NodeList xs)
    {
        this.Type = NodeType.LIST;
        this._listval = xs;
    }

    public int Intval
    {
        get {
            if (this.Type != NodeType.INT) {
                throw Utils.Panic();
            }
            return _intval;
        }
    }

    public string Strval
    {
        get {
            if (this.Type != NodeType.STR) {
                throw Utils.Panic();
            }
            return _strval;
        }
    }

    public NodeList Listval
    {
        get {
            if (this.Type != NodeType.LIST) {
                throw Utils.Panic();
            }
            return _listval;
        }
    }

    public override string ToString()
    {
        switch (Type) {
        case NodeType.INT : return $"{Intval}";
        case NodeType.STR : return $"\"{Strval}\"";
        case NodeType.LIST: return Listval.ToString();
        default:
            throw Utils.Panic();
        }
    }
}

class NodeList
{
    public List<Node> Data;

    public int Count
    {
        get {
            return Data.Count;
        }
    }

    public NodeList()
    {
        Data = new List<Node>();
    }

    public void Add(Node node)
    {
        Data.Add(node);
    }

    public void Add(int n)
    {
        Add(new Node(n));
    }

    public void Add(string s)
    {
        Add(new Node(s));
    }

    public void Add(NodeList xs)
    {
        Add(new Node(xs));
    }

    public void AddAll(NodeList xs)
    {
        foreach (Node node in xs.Data) {
            Add(node);
        }
    }

    public Node Get(int i)
    {
        return Data[i];
    }

    public int GetInt(int i)
    {
        return Get(i).Intval;
    }

    public string GetStr(int i)
    {
        return Get(i).Strval;
    }

    public NodeList GetList(int i)
    {
        return Get(i).Listval;
    }

    public NodeList Rest()
    {
        var newlist = new NodeList();
        for (int i = 1; i < this.Count; i++) {
            newlist.Add(this.Get(i));
        }
        return newlist;
    }

    public override string ToString()
    {
        string s = "[";
        foreach (Node node in this.Data) {
            s += " ," + node.ToString();
        }
        return s + "]";
    }
}

enum TokenKind
{
    INT, STR, KW, SYM, IDENT
}

class Token
{
    public int Lineno;
    public TokenKind Kind;
    public string Str;

    Token(int lineno, TokenKind kind, string str)
    {
        this.Lineno = lineno;
        this.Kind = kind;
        this.Str = str;
    }

    public static Token FromList(NodeList list)
    {
        int lineno = list.GetInt(0);
        TokenKind kind = StrToTokenKind(list.GetStr(1));
        string str = list.GetStr(2);

        return new Token(lineno, kind, str);
    }

    private static TokenKind StrToTokenKind(string s)
    {
        switch (s) {
        case "int"  : return TokenKind.INT;
        case "str"  : return TokenKind.STR;
        case "kw"   : return TokenKind.KW;
        case "sym"  : return TokenKind.SYM;
        case "ident": return TokenKind.IDENT;
        default:
            throw Utils.Panic();
        }
    }
}

class VarList
{
    public List<string> Data;

    public int Count
    {
        get { return Data.Count; }
    }

    public VarList()
    {
        Data = new List<string>();
    }

    public void Add(string varName)
    {
        Data.Add(varName);
    }

    public string Get(int i)
    {
        return Data[i];
    }

    public int IndexOf(string str)
    {
        return this.Data.IndexOf(str);
    }

    public bool Includes(string str)
    {
        return 0 <= IndexOf(str);
    }

    public static VarList From(NodeList xs)
    {
        var vars = new VarList();
        foreach (Node node in xs.Data) {
            vars.Add(node.Strval);
        }
        return vars;
    }

    public override string ToString()
    {
        string s = "[";
        for (int i = 0; i < Count; i++) {
            s += " ," + Get(i);
        }
        return s + "]";
    }
}
