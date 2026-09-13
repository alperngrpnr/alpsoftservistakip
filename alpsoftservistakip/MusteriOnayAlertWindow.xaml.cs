using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace alpsoftservistakip
{
    public partial class MusteriOnayAlertWindow : Window
    {
        private DispatcherTimer _autoCloseTimer;

        public MusteriOnayAlertWindow(string srvNo, string musteri, string cihaz, string tutar, bool isApproved)
        {
            InitializeComponent();

            txtServisNo.Text = srvNo;
            txtMusteri.Text = !string.IsNullOrWhiteSpace(musteri) ? musteri : "Müşteri";
            txtCihaz.Text = !string.IsNullOrWhiteSpace(cihaz) ? cihaz : "Cihaz";
            txtTutar.Text = !string.IsNullOrWhiteSpace(tutar) ? tutar : "-";

            if (!isApproved)
            {
                txtBaslik.Text = "MÜŞTERİ TEKLİFİ REDDETTİ!";
                txtBaslik.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                txtAltBaslik.Text = "Cihaz işlem yapılmadan iade edilecek";
                txtIcon.Text = "❌";
                bdIcon.Background = new SolidColorBrush(Color.FromRgb(127, 29, 29));
                bdIcon.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                borderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }

            Loaded += MusteriOnayAlertWindow_Loaded;
        }

        private void MusteriOnayAlertWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Sağ alt köşeye konumlandır
            var workingArea = SystemParameters.WorkArea;
            this.Left = workingArea.Right - this.ActualWidth - 20;
            this.Top = workingArea.Bottom - this.ActualHeight - 20;

            // 15 saniye sonra kendiliğinden kapanır
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _autoCloseTimer.Tick += (s, ev) =>
            {
                _autoCloseTimer.Stop();
                this.Close();
            };
            _autoCloseTimer.Start();
        }

        private void BtnKapat_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer?.Stop();
            this.Close();
        }

        public static void ShowAlert(string srvNo, string musteri, string cihaz, string tutar, bool isApproved)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                try
                {
                    var wnd = new MusteriOnayAlertWindow(srvNo, musteri, cihaz, tutar, isApproved);
                    wnd.Show();
                }
                catch { }
            });
        }
    }
}
