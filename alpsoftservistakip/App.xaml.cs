using System;
using System.Windows;

namespace alpsoftservistakip
{
    public partial class App : Application
    {
        public static int AktifSirketId { get; set; }
        
        public async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Splash ekranı
            Window11 splash = new Window11();
            splash.Show();

            // Splash kendi updaterını çalıştırır
            await splash.CheckForUpdates();

            splash.Close();

            // API üzerinden giriş yaptıktan sonra şirket belirleniyor
            new LoginHostWindow().Show();

            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }
}
