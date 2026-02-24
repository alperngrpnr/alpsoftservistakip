using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace alpsoftservistakip
{
    public partial class App : Application
    {
        public async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Splash ekranı
            Window11 splash = new Window11();
            splash.Show();

            // Firma kontrolü
            bool firmaVar = FirmaVarMi();

            // Splash kendi updaterını çalıştırır
            await splash.CheckForUpdates();

            splash.Close();

            if (firmaVar)
            {
                new LoginWindow().Show();
            }
            else
            {
                new WindowRegister().Show();
            }

            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        public bool FirmaVarMi()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["MyDbConn"]?.ConnectionString
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
