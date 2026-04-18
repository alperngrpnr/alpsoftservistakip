using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\alpsoftservistakip.csproj";
        string t = File.ReadAllText(p);

        string s1 = "<Compile Include=\"Class1.cs\" />";
        string r1 = "<Compile Include=\"Models\\Class1.cs\" />\r\n    <Compile Include=\"Models\\Class2.cs\" />\r\n    " + s1;

        t = t.Replace(s1, r1);
        File.WriteAllText(p, t);
    }
}
