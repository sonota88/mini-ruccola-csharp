using System.Collections.Generic;

class JsonTester
{
    public void Run()
    {
        string json = Utils.ReadStdInAll();

        // TestPrint1();
        // TestPrint2();
        // TestPrint3();
        // TestPrint4();
        // TestPrint5();
        // TestPrint6();
        // TestPrint7();
        // TestPrint8();

        NodeList list = Json.Parse(json);
        Json.Print(list);
    }

    void TestPrint1()
    {
        NodeList list = new NodeList();
        Json.Print(list);
    }

    void TestPrint2()
    {
        NodeList list = new NodeList();
        list.Add(1);
        Json.Print(list);
    }

    void TestPrint3()
    {
        NodeList list = new NodeList();
        list.Add("fdsa");
        Json.Print(list);
    }

    void TestPrint4()
    {
        NodeList list = new NodeList();
        list.Add(-123);
        Json.Print(list);
    }

    void TestPrint5()
    {
        NodeList list = new NodeList();
        list.Add(123);
        list.Add("fdsa");
        Json.Print(list);
    }

    void TestPrint6()
    {
        NodeList list = new NodeList();
        list.Add(new NodeList());
        Json.Print(list);
    }

    void TestPrint7()
    {
        NodeList list = new NodeList();

        list.Add(1);
        list.Add("a");

        NodeList innerList = new NodeList();
        innerList.Add(2);
        innerList.Add("b");
        list.Add(innerList);

        list.Add(3);
        list.Add("c");

        Json.Print(list);
    }

    void TestPrint8()
    {
        NodeList list = new NodeList();

        list.Add("漢字");

        Json.Print(list);
    }
}
