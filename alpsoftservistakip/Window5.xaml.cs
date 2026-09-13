using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class Window5 : Window
    {
        public Window5ViewModel ViewModel { get; private set; }

        public Window5()
        {
            InitializeComponent();
            ViewModel = new Window5ViewModel();
            DataContext = ViewModel;
            this.Closing += AltPencere_Closing;
            
            if (dgVeriler != null)
            {
                dgVeriler.MouseDoubleClick += dgVeriler_MouseDoubleClick;
            }

            InitGunSonuTimer();
            InitMusteriOnayListener();
        }

        private System.Windows.Threading.DispatcherTimer _musteriOnayTimer;
        private bool _musteriOnayChecking = false;

        private void InitMusteriOnayListener()
        {
            _musteriOnayTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _musteriOnayTimer.Tick += async (s, e) => await CheckMusteriOnaylariAsync();
            _musteriOnayTimer.Start();
        }

        private async Task CheckMusteriOnaylariAsync()
        {
            if (_musteriOnayChecking) return;
            if (string.IsNullOrWhiteSpace(Class1.JwtToken)) return;

            try
            {
                _musteriOnayChecking = true;
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new Uri(Helpers.ApiConfig.BaseUrl + "/");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    client.Timeout = TimeSpan.FromSeconds(6);

                    var res = await client.GetAsync("api/takip/yeni-onay-bildirimleri");
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

                            // 🔄 Tabloyu anında otomatik yenile
                            if (ViewModel != null)
                            {
                                ViewModel.KayitlariListele(ViewModel.AramaMetni);
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

        private System.Windows.Threading.DispatcherTimer _gunSonuTimer;
        private DateTime _lastGunSonuDate = DateTime.MinValue;

        private void InitGunSonuTimer()
        {
            _gunSonuTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(45)
            };
            _gunSonuTimer.Tick += GunSonuTimer_Tick;
            _gunSonuTimer.Start();
        }

        private async void GunSonuTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var settings = Helpers.PatronRaporSettings.Load();
                if (!settings.GunSonuRaporAktif || string.IsNullOrWhiteSpace(settings.GunSonuRaporSaati)) return;

                string currentHHmm = DateTime.Now.ToString("HH:mm");
                if (currentHHmm == settings.GunSonuRaporSaati && _lastGunSonuDate.Date != DateTime.Today)
                {
                    _lastGunSonuDate = DateTime.Today;
                    var res = MessageBox.Show(
                        $"Gün sonu saatine geldiniz ({settings.GunSonuRaporSaati}).\nBugünkü hasılat ve cihaz özetini WhatsApp üzerinden patrona iletmek ister misiniz?",
                        "📊 Gün Sonu Patron Raporu",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        var result = await Services.NotificationService.SendGunSonuRaporuAsync(settings.PatronTelefon);
                        if (!result.success) MessageBox.Show(result.message, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch { }
        }

        private void dgVeriler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.KayitDetayCommand.Execute(dgVeriler.SelectedItem);
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            //if (Application.Current.MainWindow != this)
            //{
            //    e.Cancel = true;
            //    Application.Current.MainWindow.Show();
            //    Application.Current.MainWindow.Activate();
            //    this.Hide();
            //}
        }

        public void KayitlariListele(string aramaMetni = "")
        {
            ViewModel?.KayitlariListele(aramaMetni);
        }

        private void geridön_Click(object sender, RoutedEventArgs e)
        {
            // Show the main menu Page (Page3) inside the overlay frame for a smooth in-window transition
            ShowOverlayPage(new Page3());
        }

        // Smooth fade helpers 🔧
        private Task FadeOutAsync(Window window, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ms));
            da.Completed += (s, e) => tcs.SetResult(true);
            window.Dispatcher.Invoke(() => window.BeginAnimation(Window.OpacityProperty, da));
            return tcs.Task;
        }

        private Task FadeInAsync(Window window, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(ms));
            da.Completed += (s, e) => tcs.SetResult(true);
            window.Dispatcher.Invoke(() =>
            {
                window.Opacity = 0;
                window.BeginAnimation(Window.OpacityProperty, da);
            });
            return tcs.Task;
        }

        // Element-level fade helpers for smooth in-window page transitions
        private Task FadeOutElementAsync(UIElement el, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                // Ensure final value is applied
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
                // Ensure final value is applied
                el.Dispatcher.Invoke(() => el.Opacity = 1);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() =>
            {
                // Start from current opacity but enforce 0 for a visible fade-in start
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

        // Show a Page inside the overlay frame with smooth cross-fade
        
        public async void ShowOverlayPage(System.Windows.Controls.Page page)
        {
            try
            {
                // Ensure overlay is visible before navigation so layout/measure runs correctly
                OverlayBackdrop.Visibility = Visibility.Visible;
                OverlayBackdrop.Opacity = 0;

                OverlayFrame.Visibility = Visibility.Visible;
                OverlayFrame.Opacity = 0;
                
                // Sayfayı önce yükle, sonra animasyon yap
                OverlayFrame.Navigate(page);
                
                // Sayfanın yüklenmesi için kısa bir bekleme (daha hızlı)
                await Task.Delay(30);

                // Fade backdrop and frame in (keep RootGrid visible) - daha hızlı animasyon
                await Task.WhenAll(FadeToElementAsync(OverlayBackdrop, 0.6, 150), FadeInElementAsync(OverlayFrame, 150));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
            }
        }

        public async void HideOverlay()
        {
            try
            {
                // Daha hızlı animasyon
                await Task.WhenAll(FadeOutElementAsync(OverlayFrame, 120), FadeToElementAsync(OverlayBackdrop, 0, 120));
                // ensure overlay is collapsed and cleaned
                OverlayFrame.Visibility = Visibility.Collapsed;
                OverlayFrame.Content = null;
                OverlayFrame.Opacity = 1; // reset for next use

                // clean up overlay elements (RootGrid is left unchanged)
                OverlayBackdrop.Visibility = Visibility.Collapsed;
                OverlayBackdrop.Opacity = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
                // fallback: make sure UI isn't stuck invisible
                OverlayFrame.Visibility = Visibility.Collapsed;
                OverlayFrame.Content = null;
                OverlayFrame.Opacity = 1;
                RootGrid.Visibility = Visibility.Visible;
                RootGrid.Opacity = 1;
            }
        }

        public void VerileriYukle()
        {
            ViewModel?.VerileriYukle();
        }

        private void arama_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaGotFocusCommand.Execute(null);
        }

        private void arama_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaLostFocusCommand.Execute(null);
        }

        private void KAYITLAR_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}