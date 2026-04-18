using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\alpsoftservistakip.csproj";
        string t = File.ReadAllText(p);

        string s1 = "<Compile Include=\"PageCariListesi.xaml.cs\">";
        string r1 = "<Compile Include=\"PageToptanciListesi.xaml.cs\">\r\n      <DependentUpon>PageToptanciListesi.xaml</DependentUpon>\r\n    </Compile>\r\n    <Compile Include=\"PageToptanciEkle.xaml.cs\">\r\n      <DependentUpon>PageToptanciEkle.xaml</DependentUpon>\r\n    </Compile>\r\n    <Compile Include=\"PageToptanciDetay.xaml.cs\">\r\n      <DependentUpon>PageToptanciDetay.xaml</DependentUpon>\r\n    </Compile>\r\n    " + s1;
        t = t.Replace(s1, r1);

        string s2 = "<Page Include=\"PageCariListesi.xaml\">";
        string r2 = "<Page Include=\"PageToptanciListesi.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    <Page Include=\"PageToptanciEkle.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    <Page Include=\"PageToptanciDetay.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    " + s2;
        t = t.Replace(s2, r2);

        File.WriteAllText(p, t);
        Console.WriteLine("Done");
    }
}
