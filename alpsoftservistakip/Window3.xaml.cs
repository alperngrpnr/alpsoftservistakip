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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
namespace alpsoftservistakip
{
    public partial class Window3 : Window
    {
        private System.Windows.Controls.Page _previousPage = null;
        private System.Windows.Threading.DispatcherTimer _sessionTimer;

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
            this.Closing += Window3_Closing;
            this.Loaded += Window3_Loaded;

            if (Class1.AktifKullanici.IsAdmin == false)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;
            }

            // Session kontrol zamanlayıcısını başlat (Her 5 saniyede bir)
            _sessionTimer = new System.Windows.Threading.DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromSeconds(5);
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();
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
                    var response = await client.GetAsync("Auth/liste");
                    if (!response.IsSuccessStatusCode)
                    {
                        if (await alpsoftservistakip.Helpers.SessionHelper.CheckSession(response))
                        {
                            _sessionTimer.Stop(); // Oturum düştüyse timer'ı durdur
                        }
                    }
                }
            }
            catch { /* Sessizce yut */ }
        }

        private void Window3_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    // Senkron çalıştır — pencere kapanmadan önce isteğin bitmesini bekle
                    client.PostAsync($"{ApiConfig.Api}/Auth/cikis-yap", null)
                          .GetAwaiter().GetResult();
                }
            }
            catch { /* Sunucuya ulaşılamazsa sessizce kapat */ }
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
            ShowOverlayPage(new PageAdminPanel());
        }




        private void kayitlar_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new Page5());
        }

        private void kayitolustur_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageKayitOlustur());
        }

        private void disserviskayitlari_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageDisServis());
        }

        private void caritakip_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageCariListesi());
        }

        private void btnToptanciYonetimi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageToptanciListesi());
        }

        private void disserviskayitlari_Copy_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageDisServis());
        }

        private void btnStokTakibi_Click(object sender, RoutedEventArgs e)
        {
            if (Class1.AktifKullanici.HasStokTakibi)
            {
                ShowOverlayPage(new PageStokTakibi());
            }
            else
            {
                StokKilitTabakasi.Visibility = Visibility.Visible;
            }
        }

        private void btnSatisGecmisi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageSatisGecmisi());
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

                Class1.AktifKullanici.HasStokTakibi =
                    true;

                MessageBox.Show(
                    "Tebrikler! Stok Takibi özelliği başarıyla aktifleştirildi.",
                    "Aktifleştirme Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StokKilitTabakasi.Visibility =
                    Visibility.Collapsed;

                txtStokKodu.Clear();
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

        public void ShowOverlayPage(System.Windows.Controls.Page page)
        {
            try
            {
                if (OverlayHost != null)
                {
                    OverlayHost.ShowPage(page);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
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
            if (!Class1.AktifKullanici.IsAdmin)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(
     object sender,
     CancelEventArgs e)
        {
            try
            {
                if (Class1.AktifKullanici.ID > 0)
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

                        // Await kullanılmadığı için kapanma işleminden önce tamamlanması sağlanır
                        var task = client.PostAsync(
                            $"{ApiConfig.Api}/Auth/cikis-yap",
                            null);
                        task.Wait(2000); // En fazla 2 saniye bekle
                    }
                }
            }
            catch
            {
            }

            Application.Current.Shutdown();
        }
    }
}
