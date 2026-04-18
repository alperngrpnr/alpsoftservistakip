using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\alpsoftservistakip.csproj";
        string t = File.ReadAllText(p);

        string s1 = "<Compile Include=\"Page6.xaml.cs\">";
        string r1 = "<Compile Include=\"PageCariListesi.xaml.cs\">\r\n      <DependentUpon>PageCariListesi.xaml</DependentUpon>\r\n    </Compile>\r\n    <Compile Include=\"PageCariEkle.xaml.cs\">\r\n      <DependentUpon>PageCariEkle.xaml</DependentUpon>\r\n    </Compile>\r\n    <Compile Include=\"PageCariDetay.xaml.cs\">\r\n      <DependentUpon>PageCariDetay.xaml</DependentUpon>\r\n    </Compile>\r\n    " + s1;
        t = t.Replace(s1, r1);

        string s2 = "<Page Include=\"Page6.xaml\">";
        string r2 = "<Page Include=\"PageCariListesi.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    <Page Include=\"PageCariEkle.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    <Page Include=\"PageCariDetay.xaml\">\r\n      <SubType>Designer</SubType>\r\n      <Generator>MSBuild:Compile</Generator>\r\n    </Page>\r\n    " + s2;
        t = t.Replace(s2, r2);

        File.WriteAllText(p, t);
        Console.WriteLine("Done");
    }
}
