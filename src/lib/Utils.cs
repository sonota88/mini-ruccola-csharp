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

    public static string ReadStdInAll()
    {
        using (Stream stream = Console.OpenStandardInput()) {
            using (StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8)) {
                return sr.ReadToEnd();
            }
        }
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
