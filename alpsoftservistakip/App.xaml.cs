using Squirrel;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace alpsoftservistakip
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1. Splash ekranını göster
            Window11 splash = new Window11();
            splash.Show();

            // 2. Güncelleme Kontrolü (Arka planda başlar)
            // Not: Hata almamak için try-catch içine alıyoruz
            var updateTask = CheckForUpdates();

            // 3. Bar dolmasını simüle et
            await splash.StartLoadingProcess();

            // 4. Firma tablosunu kontrol et
            bool firmaVar = FirmaVarMi();

            // Güncelleme kontrolünün bitmesini bekle (opsiyonel, açılışı çok yavaşlatmaz)
            await updateTask;

            // 5. Splash ekranını kapat
            splash.Close();

            if (firmaVar)
            {
                LoginWindow login = new LoginWindow();
                login.Show();
            }
            else
            {
                WindowRegister kayit = new WindowRegister();
                kayit.Show();
            }

            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        

        private async Task CheckForUpdates()
        {
            try
            {
                // Senin sunucu adresin (Releases dosyasının olduğu klasör)
                using (var mgr = new UpdateManager("http://91.247.168.204/dowloads/AlpSoft-Setup.zip"))
                {
                    // Yeni versiyon var mı bak ve varsa indir
                    await mgr.UpdateApp();
                }
            }
            catch (Exception)
            {
                // İnternet yoksa veya sunucu kapalıysa programın açılmasını engellememek için hata fırlatmıyoruz
            }
        }

        public bool FirmaVarMi()
        {
            // Not: Buradaki Connection String'i App.config'den çekmen daha sağlıklı olur
            string connectionString = ConfigurationManager.ConnectionStrings["MyDbConn"]?.ConnectionString
                                      ?? "Data Source=.;Initial Catalog=AlpsoftDb;Integrated Security=True";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Companies", con);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}