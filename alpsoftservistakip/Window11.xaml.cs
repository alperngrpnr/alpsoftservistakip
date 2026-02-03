using System;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows;

namespace alpsoftservistakip
{
    public partial class Window11 : Window
    {
        public Window11()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await StartLoadingProcess();
        }

        public async Task StartLoadingProcess()
        {
            try
            {
                StatusLabel.Text = "Sistem başlatılıyor...";
                UpdateBar.Value = 10;

                // Güncelleme kontrolü
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    if (ad.CheckForUpdate())
                    {
                        StatusLabel.Text = "Güncelleme indiriliyor...";
                        UpdateBar.Value = 50;
                        ad.Update();
                        MessageBox.Show("Güncelleme tamamlandı. Uygulama yeniden başlatılıyor.");
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                        return;
                    }
                }

                // Yükleme animasyonu
                for (int i = 20; i <= 100; i++)
                {
                    UpdateBar.Value = i;
                    StatusLabel.Text = $"Modüller yükleniyor... %{i}";
                    await Task.Delay(20);
                }
            }
            catch
            {
                // sessiz geç
            }
            finally
            {
                LoginWindow win = new LoginWindow();
                win.Show();
                Close();
            }
        }
    }
}
