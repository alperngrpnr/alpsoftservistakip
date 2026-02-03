using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace alpsoftservistakip
{
    public partial class WindowRegister : Window
    {
        private readonly string connString =
            "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";

        public WindowRegister()
        {
            InitializeComponent();
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Hangi pencere olursa olsun, çarpıya basıldığında tüm uygulamayı kapatır
            Application.Current.Shutdown();
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1. Girdileri Al ve Temizle
            string name = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim().ToLower();
            string p1 = txtPassword.Password;
            string p2 = txtPasswordRepeat.Password;
            string company = txtCompanyName.Text.Trim();

            // Hem tireyi hem boşluğu siliyoruz, böylece her türlü giriş kabul edilir
            string activation = txtActivationCode.Text.Trim()
                                .Replace("-", "")
                                .Replace(" ", "")
                                .ToUpper();

            // 2. Basit Kontroller
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(activation))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı");
                return;
            }

            if (p1 != p2)
            {
                MessageBox.Show("Şifreler uyuşmuyor.", "Hata");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using (SqlTransaction trans = con.BeginTransaction())
                    {
                        try
                        {
                            // --- ADIM A: LİSANS KONTROLÜ ---
                            // SQL'de de temizleyerek arama yapıyoruz (Garantili yöntem)
                            string licenseQuery = @"
                                SELECT Id, ExpireDate, IsUsed 
                                FROM Licenses 
                                WHERE REPLACE(REPLACE(UPPER(LTRIM(RTRIM(LicenseKey))), '-', ''), ' ', '') = @k";

                            SqlCommand licenseCmd = new SqlCommand(licenseQuery, con, trans);
                            licenseCmd.Parameters.AddWithValue("@k", activation);

                            int licenseId = 0;
                            using (SqlDataReader dr = licenseCmd.ExecuteReader())
                            {
                                if (!dr.Read())
                                {
                                    MessageBox.Show("Lisans anahtarı geçersiz!");
                                    return;
                                }

                                if (Convert.ToBoolean(dr["IsUsed"]))
                                {
                                    MessageBox.Show("Bu lisans zaten kullanılmış.");
                                    return;
                                }

                                if (Convert.ToDateTime(dr["ExpireDate"]) < DateTime.Now)
                                {
                                    MessageBox.Show("Lisansın süresi dolmuş.");
                                    return;
                                }

                                licenseId = Convert.ToInt32(dr["Id"]);
                            }

                            // --- ADIM B: ŞİRKET EKLE (IDENTITY UYUMLU) ---
                            // Id göndermiyoruz, SQL'in oluşturduğu Id'yi SCOPE_IDENTITY ile geri alıyoruz
                            string companyQuery = "INSERT INTO Companies (CompanyName) VALUES (@c); SELECT SCOPE_IDENTITY();";
                            SqlCommand cmdComp = new SqlCommand(companyQuery, con, trans);
                            cmdComp.Parameters.AddWithValue("@c", company);

                            // Yeni oluşan Id'yi yakalıyoruz
                            int companyId = Convert.ToInt32(cmdComp.ExecuteScalar());

                            // --- ADIM C: KULLANICI EKLE ---
                            // --- ADIM C: KULLANICI EKLE ---
                            // Tablo adını [Users] şeklinde yazarak SQL'in bunu tablo olarak algılamasını sağlıyoruz
                            string userQuery = @"INSERT INTO [Users] (CompanyId, Email, FullName, PasswordHash, Role, IsActive, LicenseId) 
                                VALUES (@cid, @em, @fn, @pw, 'Admin', 1, @lid)";

                            SqlCommand cmdUser = new SqlCommand(userQuery, con, trans);
                            cmdUser.Parameters.AddWithValue("@cid", companyId);
                            cmdUser.Parameters.AddWithValue("@em", email);
                            cmdUser.Parameters.AddWithValue("@fn", name);
                            cmdUser.Parameters.AddWithValue("@pw", HashPassword(p1));
                            cmdUser.Parameters.AddWithValue("@lid", licenseId);
                            cmdUser.ExecuteNonQuery();

                            // --- ADIM D: LİSANSI KAPAT ---
                            SqlCommand cmdUpdateLic = new SqlCommand("UPDATE Licenses SET IsUsed = 1 WHERE Id = @id", con, trans);
                            cmdUpdateLic.Parameters.AddWithValue("@id", licenseId);
                            cmdUpdateLic.ExecuteNonQuery();

                            trans.Commit();
                            MessageBox.Show("Kayıt Başarılı! 🎉");

                            new LoginWindow().Show();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("İşlem hatası: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message);
            }
        }

        private string HashPassword(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void BtnLogin_Click(object sender, MouseButtonEventArgs e)
        {
            // Giriş penceresinden bir örnek oluşturuyoruz
            LoginWindow loginWin = new LoginWindow();

            // Yeni pencereyi gösteriyoruz
            loginWin.Show();

            // Mevcut Kayıt Ol (WindowRegister) penceresini kapatıyoruz
            this.Close();
        }
    }
}