using System;
using System.Collections.Generic;

class Json {

    public static NodeList Parse(string json) {
        var parser = new Json();
        (NodeList List, int _) retval = parser.ParseList(json);
        return retval.List;
    }

    public (NodeList, int) ParseList(string json) {
        int pos = 1; // skip first '['

        var list = new NodeList();
        var re = new RegExp();

        while (pos < json.Length) {
            string rest = json.Substring(pos);
            string c0 = rest.Substring(0, 1);

            if (c0 == "]") {
                pos++;
                break;
            }

            if (c0 == " " || c0 == "," || c0 == "\n") {
                pos++;
            } else if (c0 == "[") {
                (NodeList List, int Size) retval = ParseList(rest);
                NodeList childList = retval.List;
                int size = retval.Size;
                list.Add(childList);
                pos += size;
            } else if (c0 == "\"") {
                re.Match(rest, "^\"(.*?)\"");
                string str = re.Group(1);
                list.Add(str);
                pos += str.Length + 2;
            } else if (re.Match(rest, "^(-?[1-9][0-9]*|0)")) {
                string str = re.Group(1);
                int n = Utils.ParseInt(str);
                list.Add(n);
                pos += str.Length;
            } else {
                pos++;
            }
        }

        return (list, pos);
    }

    public static void Print(NodeList list) {
        PrintList(list, 0);
    }

    private static void PrintList(NodeList list, int lv) {
        Utils.Print("[");
        Utils.Print("\n");

        for (int i = 0; i < list.Count; i++) {

            Node node = list.Get(i);
            PrintNode(node, lv + 1);
            if (i <= list.Count - 2) {
                Utils.Print(",");
            }
            Utils.Print("\n");
        }

        PrintIndent(lv);
        Utils.Print("]");
    }

    private static void PrintNode(Node node, int lv) {
        PrintIndent(lv);

        switch (node.Type) {
        case NodeType.INT:
            Utils.Print(node.Intval);
            break;
        case NodeType.STR:
            Utils.Print("\"");
            Utils.Print(node.Strval);
            Utils.Print("\"");
            break;
        case NodeType.LIST:
            PrintList(node.Listval, lv);
            break;
        default:
            throw Utils.Panic();
        }
    }

    private static void PrintIndent(int lv) {
        for (int i = 0; i < lv; i++) {
            Utils.Print("  ");
        }
    }

}
