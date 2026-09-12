using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Net;

namespace alpsoftservistakip
{
    public partial class Window11 : Window
    {
        private const string CURRENT_VERSION = "1.0.5";
        private const string VERSION_URL = "https://github.com/alperngrpnr/alpsoftupdates/releases/latest/download/version.txt";
        private const string SETUP_URL = "https://github.com/alperngrpnr/alpsoftupdates/releases/latest/download/AlpsoftSetup.exe";

        private double _currentProgress = 0;

        public Window11()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void BarContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualBar(_currentProgress);
        }

        private void SetProgress(double value, string message)
        {
            _currentProgress = Math.Max(0, Math.Min(100, value));
            StatusLabel.Text = message;
            UpdateVisualBar(_currentProgress);
        }

        private void UpdateVisualBar(double percent)
        {
            double totalWidth = BarContainer.ActualWidth;
            if (totalWidth > 0)
                VisualBar.Width = totalWidth * (percent / 100.0);
        }

        public async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            await Task.Delay(100);
            await CheckForUpdates();
        }

        public async Task CheckForUpdates()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                    SetProgress(10, "Güncellemeler kontrol ediliyor..."));

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "AlpsoftUpdater");

                    // GitHub'ın eski sürümü(önbelleği) göndermesini engellemek için linkin sonuna rastgele bir değer ekliyoruz
                    string cacheBusterUrl = $"{VERSION_URL}?t={DateTime.Now.Ticks}";

                    string latestVersion = (await client.GetStringAsync(cacheBusterUrl)).Trim();

                    await Dispatcher.InvokeAsync(() => SetProgress(20, "Sürüm kontrol edildi..."));

                    if (Version.TryParse(latestVersion, out Version parsedLatestVersion) && 
                        parsedLatestVersion > new Version(CURRENT_VERSION))
                    {
                        await DownloadAndInstallUpdate(latestVersion);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Sorunun ne olduğunu görebilmemiz için log yerine ekranda gösterin
                MessageBox.Show("Güncelleme denetlenirken hata oluştu:\n" + ex.Message, "Güncelleme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            await ContinueStartup();
        }

        private async Task DownloadAndInstallUpdate(string version)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "AlpsoftSetup.exe");
            if (File.Exists(tempFile)) File.Delete(tempFile);

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "AlpsoftUpdater");

                webClient.DownloadProgressChanged += (s, e) =>
                {
                    Dispatcher.Invoke(() =>
                        SetProgress(e.ProgressPercentage,
                            $"Yeni sürüm indiriliyor: %{e.ProgressPercentage}"));
                };

                await webClient.DownloadFileTaskAsync(new Uri(SETUP_URL), tempFile);
            }

            await Dispatcher.InvokeAsync(() =>
                SetProgress(100, "Kurulum tamamlandı, yeniden başlatılıyor..."));

            await Task.Delay(500);

            // Mevcut uygulamanın çalıştırılabilir dosyasının yolu
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Kurulum sessizce yapılsın, bitince uygulama otomatik başlasın
            // /VERYSILENT: Hiç pencere göstermez
            // /SUPPRESSMSGBOXES: Soru sormaz
            // /NORESTART: Bilgisayarı yeniden başlatmaz
            // CMD: 1 sn bekle (uygulama kapansın), kur, bitince uygulamayı başlat
            string arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-";
            string cmd = $"/C timeout /t 1 > nul & start \"\" \"{tempFile}\" {arguments} & timeout /t 5 > nul & start \"\" \"{appPath}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = cmd,
                CreateNoWindow = true,
                UseShellExecute = false
            });

            Environment.Exit(0);
        }

        private async Task ContinueStartup()
        {
            int startValue = (int)_currentProgress;

            for (int i = startValue; i <= 100; i++)
            {
                await Dispatcher.InvokeAsync(() =>
                    SetProgress(i, $"Sistem hazırlanıyor... %{i}"));

                await Task.Delay(20);
            }

            new LoginHostWindow().Show();
            Close();
        }
    }
}
