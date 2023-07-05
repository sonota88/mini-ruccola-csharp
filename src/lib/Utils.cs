using System;
using System.IO;
using System.Text.RegularExpressions;

class Utils
{
    public static void Puts(object arg)
    {
        Print(arg);
        Print("\n");
    }

    public static void Puts_e(object arg)
    {
        Console.Error.WriteLine(arg);
    }

    public static void Print(object arg)
    {
        Console.Write(arg);
    }

    public static int ParseInt(string str)
    {
        return Int32.Parse(str);
    }

    // very neive
    public static string ReadStdInAll()
    {
        int max = 10_000;
        byte[] bytes = new byte[max];
        int i = 0;

        using (Stream inStream = Console.OpenStandardInput()) {
            byte[] bs = new byte[1];

            while (true) {
                int n = inStream.Read(bs, 0, 1);
                if (n == 0) {
                    break;
                }
                bytes[i] = bs[0];
                i++;
                if (i > max) {
                    throw new Exception();
                }
            }
        }

        return System.Text.Encoding.UTF8.GetString(bytes, 0, i);
    }

    public static Exception Panic()
    {
        return new Exception("PANIC");
    }
}

class RegExp
{
    private Match m;

    public bool Match(string str, string pattern)
    {
        this.m = Regex.Match(str, pattern);

        if (this.m.Success) {
            return true;
        } else {
            this.m = null;
            return false;
        }
    }

    public string Group(int i)
    {
        if (this.m == null) {
            throw new Exception("not available");
        }
        return this.m.Groups[i].Value;
    }
}
