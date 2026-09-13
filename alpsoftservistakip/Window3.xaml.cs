using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
namespace alpsoftservistakip
{
    public partial class Window3 : Window
    {
        private System.Windows.Threading.DispatcherTimer _sessionTimer;
        private bool _isLoggingOut = false;

        private async void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            await AnimateWindowOut();
            this.WindowState = WindowState.Minimized;
            ResetWindowAnimation();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

            UpdateMaximizeIcon();
        }

        private async void btnClose_Click(object sender, RoutedEventArgs e)
        {
            await AnimateWindowOut();
            this.Close();
        }

        private Task AnimateWindowOut()
        {
            var tcs = new TaskCompletionSource<bool>();
            
            var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            opacityAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            
            if (RootGrid.RenderTransform is TransformGroup group)
            {
                var scaleTransform = group.Children[0] as ScaleTransform;
                var translateTransform = group.Children[1] as TranslateTransform;

                if (scaleTransform != null && translateTransform != null)
                {
                    var scaleAnim = new DoubleAnimation(1, 0.95, TimeSpan.FromMilliseconds(200));
                    scaleAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };

                    var translateAnim = new DoubleAnimation(0, 15, TimeSpan.FromMilliseconds(200));
                    translateAnim.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
                    
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnim);
                }
            }

            opacityAnim.Completed += (s, ev) => tcs.SetResult(true);
            this.BeginAnimation(Window.OpacityProperty, opacityAnim);

            return tcs.Task;
        }

        private void ResetWindowAnimation()
        {
            this.BeginAnimation(Window.OpacityProperty, null);
            this.Opacity = 1;
            
            if (RootGrid.RenderTransform is TransformGroup group)
            {
                var scaleTransform = group.Children[0] as ScaleTransform;
                var translateTransform = group.Children[1] as TranslateTransform;

                if (scaleTransform != null && translateTransform != null)
                {
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                    
                    scaleTransform.ScaleX = 1;
                    scaleTransform.ScaleY = 1;
                    translateTransform.Y = 0;
                }
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    btnMaximize_Click(sender, new RoutedEventArgs());
                    return;
                }
                this.DragMove();
            }
        }



        public Window3()
        {
            InitializeComponent();
            UpdateMaximizeIcon();
            this.Loaded += Window3_Loaded;

            // Personel Yönetimi butonu menüde her zaman görünür, yetki kontrolü tıklamada yapılır

            // Session kontrol zamanlayıcısını başlat (Her 10 saniyede bir kalp atışı)
            _sessionTimer = new System.Windows.Threading.DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromSeconds(10);
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();

            InitMusteriOnayListener();
        }

        private System.Windows.Threading.DispatcherTimer _musteriOnayTimer;
        private bool _musteriOnayChecking = false;

        private void InitMusteriOnayListener()
        {
            _musteriOnayTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _musteriOnayTimer.Tick += async (s, e) => await CheckMusteriOnaylariAsync();
            _musteriOnayTimer.Start();
        }

        private async Task CheckMusteriOnaylariAsync()
        {
            if (_musteriOnayChecking) return;

            try
            {
                _musteriOnayChecking = true;
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new Uri(Helpers.ApiConfig.BaseUrl + "/");
                    if (!string.IsNullOrWhiteSpace(Class1.JwtToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    }
                    client.Timeout = TimeSpan.FromSeconds(5);

                    int companyId = Class1.AktifKullanici?.SirketID ?? 0;
                    string endpoint = companyId > 0 
                        ? $"api/takip/yeni-onay-bildirimleri?companyId={companyId}" 
                        : "api/takip/yeni-onay-bildirimleri";

                    var res = await client.GetAsync(endpoint);
                    if (res.IsSuccessStatusCode)
                    {
                        string json = await res.Content.ReadAsStringAsync();
                        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<Models.MusteriOnayItemDto>>(json);
                        if (list != null && list.Count > 0)
                        {
                            // 🔔 Sesli bildirim çal!
                            Helpers.SoundHelper.PlayOnayChime();

                            // 💬 Her onay/red için görsel popup göster
                            foreach (var item in list)
                            {
                                bool isApproved = !string.Equals(item.Karar, "Reddedildi", StringComparison.OrdinalIgnoreCase);
                                MusteriOnayAlertWindow.ShowAlert(item.ServisNo, item.MusteriAdi, item.Cihaz, item.FiyatBilgisi, isApproved);
                            }

                            // 🔄 Açık sayfalar varsa yenile
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window is Window5 win5 && win5.IsVisible)
                                {
                                    win5.VerileriYukle();
                                }
                            }
                        }
                    }
                    // 2. Randevu Bildirimlerini Kontrol Et
                    string randevuEndpoint = companyId > 0
                        ? $"api/randevu/yeni-bildirimler?companyId={companyId}"
                        : "api/randevu/yeni-bildirimler";

                    var resRandevu = await client.GetAsync(randevuEndpoint);
                    if (resRandevu.IsSuccessStatusCode)
                    {
                        string jsonR = await resRandevu.Content.ReadAsStringAsync();
                        var rList = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<Models.RandevuItemDto>>(jsonR);
                        if (rList != null && rList.Count > 0)
                        {
                            // 🔔 Özel melodi çal
                            Helpers.SoundHelper.PlayOnayChime();

                            // 💬 Her yeni randevu için görsel popup göster
                            foreach (var r in rList)
                            {
                                MusteriOnayAlertWindow.ShowAlert("📅 YENİ RANDEVU", r.MusteriAdi, $"{r.Cihaz} - {r.IslemTuru}", $"{r.RandevuTarihi} {r.RandevuSaati}", true);
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                _musteriOnayChecking = false;
            }
        }

        // ============================================================
        // Tam Ekranda Görev Çubuğunu Kapatmayı Engelle (WinAPI Hook)
        // ============================================================
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);
        }

        private const int WM_GETMINMAXINFO = 0x0024;
        private const int WM_ERASEBKGND = 0x0014;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const uint SWP_NOCOPYBITS = 0x0100;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
                return IntPtr.Zero;
            }
            else if (msg == WM_ERASEBKGND)
            {
                // Boyutlandırma sırasında arka planın silinip titremesini önle
                handled = true;
                return (IntPtr)1;
            }
            return IntPtr.Zero;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                if (GetMonitorInfo(monitor, ref monitorInfo))
                {
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                    mmi.ptMaxTrackSize.X = mmi.ptMaxSize.X;
                    mmi.ptMaxTrackSize.Y = mmi.ptMaxSize.Y;
                }
            }

            // Minimum boyut sınırını uygula (Pencerenin aşırı küçülmesini engeller)
            double dpiX = 1.0;
            double dpiY = 1.0;
            try
            {
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    dpiX = source.CompositionTarget.TransformToDevice.M11;
                    dpiY = source.CompositionTarget.TransformToDevice.M22;
                }
            }
            catch { }

            int minW = (int)((this.MinWidth > 0 ? this.MinWidth : 1000) * dpiX);
            int minH = (int)((this.MinHeight > 0 ? this.MinHeight : 650) * dpiY);

            mmi.ptMinTrackSize.X = minW;
            mmi.ptMinTrackSize.Y = minH;

            Marshal.StructureToPtr(mmi, lParam, true);
        }
        // ============================================================

        private void Window3_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateWindowIn();
            InitUserProfile();
            _ = InitLiveCurrencyTickerAsync();
        }

        private void InitUserProfile()
        {
            try
            {
                string adSoyad = !string.IsNullOrWhiteSpace(Class1.AktifKullanici?.AdSoyad)
                    ? Class1.AktifKullanici.AdSoyad
                    : "Kullanıcı";
                string rol = Class1.AktifKullanici?.IsAdmin == true ? "Yönetici" : "Personel";

                if (txtHeaderKullaniciAdi != null)
                    txtHeaderKullaniciAdi.Text = adSoyad;

                if (txtSidebarKullaniciAdi != null)
                    txtSidebarKullaniciAdi.Text = adSoyad;

                if (txtSidebarKullaniciRol != null)
                    txtSidebarKullaniciRol.Text = rol;

                if (txtUserAvatarInitial != null && adSoyad.Length > 0)
                    txtUserAvatarInitial.Text = adSoyad.Substring(0, 1).ToUpper();

                GuncelleAktifSubeBaslik();
            }
            catch { }
        }

        public void GuncelleAktifSubeBaslik()
        {
            try
            {
                if (txtHeaderSubeAdi != null)
                {
                    txtHeaderSubeAdi.Text = !string.IsNullOrWhiteSpace(Class1.AktifSubeAdi)
                        ? Class1.AktifSubeAdi
                        : "Tüm Şubeler (Merkez)";
                }

                _ = KontrolEtYoldakiTransferlerAsync();
            }
            catch { }
        }

        public async Task KontrolEtYoldakiTransferlerAsync()
        {
            try
            {
                var incoming = await alpsoftservistakip.Services.BranchService.GetIncomingTransfersAsync(Class1.AktifSubeId);
                if (incoming != null && incoming.Count > 0)
                {
                    if (btnHeaderYoldaTransfer != null)
                    {
                        btnHeaderYoldaTransfer.Visibility = Visibility.Visible;
                        txtHeaderYoldaTransferSayisi.Text = incoming.Count == 1 
                            ? "1 Transfer Yolda" 
                            : $"{incoming.Count} Transfer Yolda";
                    }
                }
                else
                {
                    if (btnHeaderYoldaTransfer != null)
                        btnHeaderYoldaTransfer.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                if (btnHeaderYoldaTransfer != null)
                    btnHeaderYoldaTransfer.Visibility = Visibility.Collapsed;
            }
        }

        private void btnHeaderYoldaTransfer_Click(object sender, RoutedEventArgs e)
        {
            var page = new PageSubeYonetimi();
            ShowOverlayPage(page, isRoot: true);
            page.SekmeDegistir("transferler");
        }

        private void btnHeaderSube_Click(object sender, RoutedEventArgs e)
        {
            var win = new SubeSecimWindow();
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                GuncelleAktifSubeBaslik();
            }
        }

        private void btnSubeYonetimi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageSubeYonetimi(), isRoot: true);
        }

        private async void btnCikisYap_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Oturumunuzu kapatıp çıkış yapmak istediğinize emin misiniz?",
                "Oturumu Kapat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _sessionTimer?.Stop();

                // API cikis-yap çağrısı yap
                if (!string.IsNullOrEmpty(Class1.JwtToken))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(3);
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                        await client.PostAsync($"{ApiConfig.Api}/Auth/cikis-yap", null);
                    }
                }
            }
            catch
            {
                // Ağ hatası olsa bile yerel oturumu sonlandır
            }

            // Oturum durumunu temizle
            _isLoggingOut = true;
            Class1.JwtToken = null;
            Class1.AktifKullanici = new KullaniciModel();
            SessionHelper.IsKickedOut = true; // Kapanırken tekrar cikis-yap veya shutdown tetiklenmesin

            // Giriş penceresini aç
            var loginWin = new LoginHostWindow();
            loginWin.Show();

            // Window3'ü kapat
            this.Close();
        }

        private void AnimateWindowIn()
        {
            var group = RootGrid?.RenderTransform as TransformGroup;
            if (group == null) return;

            var scaleTransform = group.Children[0] as ScaleTransform;
            var translateTransform = group.Children[1] as TranslateTransform;

            if (scaleTransform == null || translateTransform == null) return;

            // Başlangıç değerlerini ayarla
            this.Opacity = 0;
            scaleTransform.ScaleX = 0.95;
            scaleTransform.ScaleY = 0.95;
            translateTransform.Y = 12;

            var easing = new CubicEase() { EasingMode = EasingMode.EaseOut };

            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220))
                { EasingFunction = easing };

            var scaleAnim = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(260))
                { EasingFunction = easing };

            var translateAnim = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(260))
                { EasingFunction = easing };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnim);
            this.BeginAnimation(Window.OpacityProperty, opacityAnim);
        }

        private async void SessionTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync("Auth/session-check");
                    if (!response.IsSuccessStatusCode)
                    {
                        if (await alpsoftservistakip.Helpers.SessionHelper.CheckSession(response))
                        {
                            _sessionTimer?.Stop(); // Oturum düştüyse timer'ı durdur
                        }
                    }
                }
            }
            catch { /* Sessizce yut */ }
        }

        private bool _wasMinimized = false;

        private void Window3_StateChanged(object sender, EventArgs e)
        {
            UpdateMaximizeIcon();

            var chrome = WindowChrome.GetWindowChrome(this);
            if (chrome != null)
            {
                chrome.ResizeBorderThickness = this.WindowState == WindowState.Maximized
                    ? new Thickness(0)
                    : new Thickness(6);
            }

            if (this.WindowState == WindowState.Minimized)
            {
                _wasMinimized = true;
            }
            else if (_wasMinimized)
            {
                _wasMinimized = false;
                AnimateWindowIn();
            }
        }

        private void UpdateMaximizeIcon()
        {
            if (txtMaximizeIcon == null)
                return;

            txtMaximizeIcon.Text = this.WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        }



        private void btnAdminPanelAc_Click(object sender, RoutedEventArgs e)
        {
            if (Class1.AktifKullanici != null && !Class1.AktifKullanici.IsAdmin)
            {
                MessageBox.Show("Personel Yönetimi paneline yalnızca yönetici (Admin) yetkisine sahip kullanıcılar erişebilir.", "Yetki Yetersiz", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ShowOverlayPage(new PageAdminPanel(), isRoot: true);
        }

        private void kayitlar_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new Page5(), isRoot: true);
        }

        private void kayitolustur_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageKayitOlustur(), isRoot: true);
        }

        private void disserviskayitlari_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageDisServis(), isRoot: true);
        }

        private void btnRandevular_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageRandevular(), isRoot: true);
        }

        private void caritakip_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageCariListesi(), isRoot: true);
        }

        private void btnToptanciYonetimi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageToptanciListesi(), isRoot: true);
        }

        private void disserviskayitlari_Copy_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageDisServis(), isRoot: true);
        }

        private void btnStokTakibi_Click(object sender, RoutedEventArgs e)
        {
            if (Class1.AktifKullanici.HasStokTakibi)
            {
                ShowOverlayPage(new PageStokTakibi(), isRoot: true);
            }
            else
            {
                StokKilitTabakasi.Visibility = Visibility.Visible;
                txtStokKodu.Focus();
                txtStokKodu.SelectAll();
            }
        }

        private void txtStokKodu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnStokAktiflestir_Click(btnStokAktiflestir, new RoutedEventArgs());
            }
        }

        private void btnSatisGecmisi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageSatisGecmisi(), isRoot: true);
        }

        private void btnKasaTakibi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageKasaTakibi(), isRoot: true);
        }

        private void btnAylikMuhasebe_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageAylikMuhasebe(), isRoot: true);
        }

        private void btnVeresiyeDefteri_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageVeresiyeDefteri(), isRoot: true);
        }

        private void btnDovizHesaplayici_Click(object sender, RoutedEventArgs e)
        {
            var win = new DovizHesaplayiciWindow();
            win.Owner = this;
            win.ShowDialog();
            _ = InitLiveCurrencyTickerAsync();
        }

        private void btnTelefonEnvanteri_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageTelefonEnvanter(), isRoot: true);
        }

        private async Task InitLiveCurrencyTickerAsync()
        {
            try
            {
                var kurlar = await DovizKuruHelper.GetGuncelKurlarAsync();
                if (kurlar != null && kurlar.UsdSatis > 0)
                {
                    txtHeaderDovizKur.Text = string.Format(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), "USD: $ {0:N2} ₺", kurlar.UsdSatis);
                }
            }
            catch { }
        }

        private void btnAyarlar_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageAyarlar(), isRoot: true);
        }

        private void btnStokIptal_Click(object sender, RoutedEventArgs e)
        {
            StokKilitTabakasi.Visibility = Visibility.Collapsed;
            txtStokKodu.Clear();
        }

        private async void btnStokAktiflestir_Click(
    object sender,
    RoutedEventArgs e)
        {
            string girilenKod =
                txtStokKodu.Text.Trim();

            if (string.IsNullOrEmpty(girilenKod))
            {
                MessageBox.Show(
                    "Lütfen bir aktivasyon kodu girin.",
                    "Uyarı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (HttpClient client =
                    new HttpClient())
                {
                    client.BaseAddress =
                        new Uri(
                            (ApiConfig.Api + "/"));

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            Class1.JwtToken);

                    var dto =
                        new StokAktivasyonDto
                        {
                            LicenseKey =
                                girilenKod
                        };

                    string json =
                        JsonConvert.SerializeObject(dto);

                    var content =
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json");

                    var response =
                        await client.PostAsync(
                            $"{ApiConfig.Api}/Lisans/stok-aktiflestir",
                            content);

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            await response.Content
                                .ReadAsStringAsync(),
                            "Hata",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }
                }

                Class1.AktifKullanici.HasStokTakibi = true;

                MessageBox.Show(
                    "Tebrikler! Stok Takibi özelliği başarıyla aktifleştirildi.",
                    "Aktifleştirme Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StokKilitTabakasi.Visibility = Visibility.Collapsed;
                txtStokKodu.Clear();
                ShowOverlayPage(new PageStokTakibi(), isRoot: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Bağlantı hatası oluştu: " +
                    ex.Message,
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private Task FadeOutElementAsync(UIElement el, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = 0);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() => el.BeginAnimation(UIElement.OpacityProperty, da));
            return tcs.Task;
        }

        private Task FadeInElementAsync(UIElement el, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = 1);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() =>
            {
                el.Opacity = 0;
                el.BeginAnimation(UIElement.OpacityProperty, da);
            });
            return tcs.Task;
        }

        private Task FadeToElementAsync(UIElement el, double to, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(to, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = to);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() => el.BeginAnimation(UIElement.OpacityProperty, da));
            return tcs.Task;
        }

        public void ShowOverlayPage(System.Windows.Controls.Page page, bool isRoot = false)
        {
            try
            {
                if (OverlayHost != null)
                {
                    OverlayHost.ShowPage(page, isRoot);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
            }
        }

        public void GoBack()
        {
            if (OverlayHost != null)
            {
                OverlayHost.GoBack();
            }
        }

        public void HideOverlay()
        {
            if (OverlayHost != null)
            {
                OverlayHost.HideOverlay();
            }
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e) { this.Close(); }

        private void YetkiKontrolü()
        {
            // Menü öğeleri görünür kalır
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                _sessionTimer?.Stop();

                // Eğer başka bir bilgisayardan giriş yapıldığı için bu cihazdaki oturum sonlandırıldıysa (kicked out),
                // veya kullanıcı zaten Çıkış Yap butonuna basmışsa cikis-yap tekrar çağrılmaz.
                if (!_isLoggingOut && !SessionHelper.IsKickedOut && Class1.AktifKullanici != null && Class1.AktifKullanici.ID > 0 && !string.IsNullOrEmpty(Class1.JwtToken))
                {
                    string token = Class1.JwtToken;
                    Task.Run(async () =>
                    {
                        try
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromMilliseconds(1000);
                                client.DefaultRequestHeaders.Authorization =
                                    new AuthenticationHeaderValue("Bearer", token);

                                await client.PostAsync($"{ApiConfig.Api}/Auth/cikis-yap", null);
                            }
                        }
                        catch { }
                    }).Wait(1000);
                }
            }
            catch
            {
            }
            finally
            {
                // Eğer oturum düştüğü veya "Çıkış Yap" dendiği için LoginHostWindow'a dönülmüyorsa:
                // Hangi sayfada olunursa olunsun uygulama kapatıldığında arkada hiçbir kalıntı kalmadan komple kapanır.
                if (!_isLoggingOut && !SessionHelper.IsKickedOut)
                {
                    try
                    {
                        foreach (Window w in Application.Current.Windows)
                        {
                            if (w != this)
                            {
                                try { w.Close(); } catch { }
                            }
                        }

                        Application.Current.Shutdown();
                    }
                    catch { }

                    Environment.Exit(0);
                }
            }
        }
    }
}
