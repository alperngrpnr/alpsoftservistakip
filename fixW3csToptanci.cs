using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml.cs";
        string t = File.ReadAllText(p);

        string search = "private void disserviskayitlari_Copy_Click(object sender, RoutedEventArgs e)";
        string repl = "private void btnToptanciYonetimi_Click(object sender, RoutedEventArgs e)\r\n        {\r\n            ShowOverlayPage(new PageToptanciListesi());\r\n        }\r\n\r\n        " + search;

        if (!t.Contains("btnToptanciYonetimi_Click"))
        {
            t = t.Replace(search, repl);
            File.WriteAllText(p, t);
        }
    }
}
