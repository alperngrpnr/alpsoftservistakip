using System.Diagnostics;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace alpsoftservistakip
{
    public partial class PageMailKurulumYardim : Page
    {
        public PageMailKurulumYardim()
        {
            InitializeComponent();
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.HideOverlay();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Clipboard.SetText(e.Uri.AbsoluteUri);
                MessageBox.Show("Link kopyalandı:\n" + e.Uri.AbsoluteUri, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Link kopyalanamadı.");
            }
            e.Handled = true;
        }

    }
}


