using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace alpsoftservistakip
{
    public partial class MusteriOnayAlertWindow : Window
    {
        private DispatcherTimer _autoCloseTimer;
        private bool _isRandevuAlert = false;

        // Fiyat Onay/Red Bildirimi Constructor
        public MusteriOnayAlertWindow(string srvNo, string musteri, string cihaz, string tutar, bool isApproved)
        {
            InitializeComponent();
            _isRandevuAlert = false;

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

        // Randevu Talebi Bildirimi Constructor
        public MusteriOnayAlertWindow(string musteri, string cihaz, string islemTuru, string tarihSaat)
        {
            InitializeComponent();
            _isRandevuAlert = true;

            txtBaslik.Text = "📅 YENİ RANDEVU TALEBİ!";
            txtBaslik.Foreground = new SolidColorBrush(Color.FromRgb(255, 208, 38)); // Gold
            txtAltBaslik.Text = "Web sitenizden yeni bir randevu oluşturuldu";
            txtIcon.Text = "📅";
            bdIcon.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
            bdIcon.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 208, 38));
            borderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 208, 38));

            lblServisNo.Text = "Talep:";
            txtServisNo.Text = !string.IsNullOrWhiteSpace(islemTuru) ? islemTuru : "Teknik Servis";

            lblMusteri.Text = "Müşteri:";
            txtMusteri.Text = !string.IsNullOrWhiteSpace(musteri) ? musteri : "Müşteri";

            lblCihaz.Text = "Cihaz:";
            txtCihaz.Text = !string.IsNullOrWhiteSpace(cihaz) ? cihaz : "-";

            lblTutar.Text = "Randevu Zamanı:";
            txtTutar.Text = !string.IsNullOrWhiteSpace(tarihSaat) ? tarihSaat : "-";
            txtTutar.Foreground = new SolidColorBrush(Color.FromRgb(255, 208, 38));

            btnAksiyon.Content = "📅 Randevuları Aç";
            btnAksiyon.Background = new SolidColorBrush(Color.FromRgb(255, 208, 38));
            btnAksiyon.Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42));

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

        private void BtnAksiyon_Click(object sender, RoutedEventArgs e)
        {
            _autoCloseTimer?.Stop();

            if (_isRandevuAlert)
            {
                try
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Window3 w3)
                        {
                            w3.ShowOverlayPage(new PageRandevular(), isRoot: true);
                            if (w3.WindowState == WindowState.Minimized)
                                w3.WindowState = WindowState.Normal;
                            w3.Activate();
                            break;
                        }
                    }
                }
                catch { }
            }

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

        public static void ShowRandevuAlert(string musteri, string cihaz, string islemTuru, string tarihSaat)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                try
                {
                    var wnd = new MusteriOnayAlertWindow(musteri, cihaz, islemTuru, tarihSaat);
                    wnd.Show();
                }
                catch { }
            });
        }
    }
}
