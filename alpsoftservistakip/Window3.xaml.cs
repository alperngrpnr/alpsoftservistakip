using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; // Buton, TextBox ve DataGrid için gereklidir.

namespace alpsoftservistakip
{
    public partial class Window3 : Window
    {
        // GÖRSELDEKİ VERİTABANI ADINA GÖRE GÜNCELLENDİ
        string connectionString = "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";


        public Window3()
        {
            InitializeComponent();

            // YETKİ KONTROLÜ: Giriş yapan kişi Admin değilse butonu gizle
            if (Class1.AktifKullanici.IsAdmin == false)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;

                // Opsiyonel: İşçinin lisans durumunu görmesini de istemeyebilirsin
                // txtLisansDurum.Visibility = Visibility.Collapsed; 
            }

            try { TabloyuGuncelle(); } catch { }
        }

        private void TabloyuGuncelle()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Sadece giriş yapan kullanıcının şirketine ait işçileri getir
                    string query = "SELECT FullName, Email, Role FROM Users WHERE CompanyId = @sirketId AND IsActive = 1";

                    SqlCommand cmd = new SqlCommand(query, con);
                    // LoginWindow'da atadığımız Şirket ID'sini buraya parametre olarak gönderiyoruz
                    cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgIsciler.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
        }
        // Önce metodun aynısını buraya ekle (LoginWindow ile aynı olmalı)
        private string HashSifre(string sifre)
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sifre));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void IsciKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Boş alan kontrolü
            if (string.IsNullOrEmpty(txtYeniAd.Text) ||
                string.IsNullOrEmpty(txtYeniEmail.Text) ||
                string.IsNullOrEmpty(txtYeniSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları (Ad, Email, Şifre) doldurun!");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Artık şifreyi kutudan alıyoruz ve hashliyoruz
                    string girilenSifre = txtYeniSifre.Text.Trim();
                    string hashliSifre = HashSifre(girilenSifre);

                    string query = "INSERT INTO Users (CompanyId, FullName, Email, PasswordHash, Role, IsActive, LicenseId) " +
                                   "VALUES (3, @p1, @p2, @p3, 'Worker', 1, 3)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@p1", txtYeniAd.Text.Trim());
                    cmd.Parameters.AddWithValue("@p2", txtYeniEmail.Text.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@p3", hashliSifre);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"İşçi başarıyla oluşturuldu!\nEmail: {txtYeniEmail.Text}\nŞifre: {girilenSifre}");

                    // Temizlik
                    txtYeniAd.Clear();
                    txtYeniEmail.Clear();
                    txtYeniSifre.Clear();
                    TabloyuGuncelle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message);
            }
        }

        private void IsciSil_Click(object sender, RoutedEventArgs e)
        {
            // Butonun Tag özelliğine bağladığımız Email bilgisini alıyoruz
            var email = (sender as Button).Tag.ToString();

            MessageBoxResult result = MessageBox.Show($"{email} adresli işçiyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        // Veritabanından tamamen silmek yerine IsActive = 0 yapabilirsin veya direkt DELETE kullanabilirsin
                        string query = "DELETE FROM Users WHERE Email = @p1 AND Role = 'Worker'";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@p1", email);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("İşçi başarıyla silindi.");
                        TabloyuGuncelle(); // Listeyi yenile
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }
        private void btnAdminPanelAc_Click(object sender, RoutedEventArgs e)
        {
            AdminPanelTabakasi.Visibility = Visibility.Visible;
            TabloyuGuncelle();
        }

        private void AdminPanelKapat_Click(object sender, RoutedEventArgs e) => AdminPanelTabakasi.Visibility = Visibility.Collapsed;

        // Diğer buton yönlendirmeleri (Window5, Window9 vb.) sende olduğu gibi kalabilir.
        private void kayitlar_Click(object sender, RoutedEventArgs e) { Window5 win = new Window5(); win.Show(); this.Hide(); }
        private void kayitolustur_Click(object sender, RoutedEventArgs e) { Window9 win = new Window9(); win.Show(); this.Hide(); }
        private void disserviskayitlari_Click(object sender, RoutedEventArgs e) { Window7 win = new Window7(); win.Show(); this.Hide(); }
        private void AltPencere_Closing(object sender, CancelEventArgs e) { Application.Current.Shutdown(); }

        private void YetkiKontrolü()
        {
            // Eğer giriş yapan kişi Admin değilse Admin Paneli butonunu gizle
            if (!Class1.AktifKullanici.IsAdmin)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;
            }
        }
    }
}