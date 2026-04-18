using System.IO;

class Program
{
    static void Main()
    {
        string p1 = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\PageKayitOlustur.xaml.cs";
        string cs = @"using System.Windows.Controls;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class PageKayitOlustur : Page
    {
        public PageKayitOlusturViewModel ViewModel { get; private set; }

        public PageKayitOlustur() : this(0)
        {
        }

        public PageKayitOlustur(int kayitId)
        {
            InitializeComponent();
            ViewModel = new PageKayitOlusturViewModel(kayitId);
            DataContext = ViewModel;
        }
    }
}";
        File.WriteAllText(p1, cs);

        string p2 = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\PageKayitOlustur.xaml";
        string xaml = File.ReadAllText(p2);
        xaml = xaml.Replace("CornerRadius=\"20\"", "");
        xaml = xaml.Replace("CornerRadius=\"5\"", "");
        File.WriteAllText(p2, xaml);
    }
}
