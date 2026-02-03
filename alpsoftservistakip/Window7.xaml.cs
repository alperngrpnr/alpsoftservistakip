using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace alpsoftservistakip
{
    public partial class Window7 : Window
    {
        private readonly string ConnectionString =
             "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        public Window7()
        {
            InitializeComponent();
            Loaded += Window7_Loaded;

            // ✅ DataGrid double-click eventini kod ile bağla
            dgdisservisVeriler.MouseDoubleClick += disMouseDoubleClick;
        }

        private void Window7_Loaded(object sender, RoutedEventArgs e)
        {
            VerileriGetir();
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Hangi pencere olursa olsun, çarpıya basıldığında tüm uygulamayı kapatır
            Environment.Exit(0);
        }


        // =========================
        // VERİ ÇEKME + FİLTRELEME
        // =========================
        private void VerileriGetir(string arama = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = @"SELECT * FROM disserviskayitlarnew 
                                   WHERE KullaniciID = @KullaniciID
                                   AND (@arama = '' 
                                   OR IsimSoyisim LIKE '%' + @arama + '%' 
                                   OR Marka LIKE '%' + @arama + '%'
                                   OR YetkiliBayi LIKE '%' + @arama + '%')
                                   ORDER BY KayitID DESC"; // ✅ En yeni kayıtlar üstte

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                    cmd.Parameters.AddWithValue("@arama", arama);

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    dgdisservisVeriler.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen hata: " + ex.Message);
            }
        }

        // =========================
        // TEXTBOX FİLTRELEME
        // =========================
        private void aramadisservis1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || dgdisservisVeriler == null) return;

            string arama = aramadisservis1.Text;

            if (arama == "İsim veya Model Ara...")
                arama = "";

            VerileriGetir(arama);
        }

        // =========================
        // DATAGRID DOUBLE CLICK
        // =========================
        private void disMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgdisservisVeriler.SelectedItem is DataRowView row)
            {
                int id = Convert.ToInt32(row["KayitID"]);

                Window6 detay = new Window6(id);

                // Window6 kapandığında ne olursa olsun yenileme yapması için 
                // ShowDialog'dan hemen sonra fonksiyonu çağırıyoruz.
                detay.ShowDialog();

                // Placeholder metnini kontrol ederek temiz bir arama parametresi gönderiyoruz
                string aramaParametresi = (aramadisservis1.Text == "İsim veya Model Ara..." || aramadisservis1.Text == "Arama")
                                          ? ""
                                          : aramadisservis1.Text;

                VerileriGetir(aramaParametresi);
            }
        }

        // =========================
        // WINDOW6'DAN GERİ DÖNÜŞ
        // =========================
        public void Yenile()
        {
            string arama = aramadisservis1.Text == "Arama" ? "" : aramadisservis1.Text;
            VerileriGetir(arama);

            if (!this.IsVisible)
            {
                this.Show();
            }
            this.Activate();
        }

        // =========================
        // GERİ BUTONU
        // =========================
        private void geridöndisservis_Click(object sender, RoutedEventArgs e)
        {
            Window3 window3 = new Window3();
            window3.Show();
            this.Hide();
        }

        // =========================
        // PLACEHOLDER
        // =========================
        private void aramadisservis1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (aramadisservis1.Text == "İsim veya Model Ara...")
            {
                aramadisservis1.Text = "";
                aramadisservis1.Foreground = Brushes.Black;
            }
        }

        private void aramadisservis1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(aramadisservis1.Text))
            {
                aramadisservis1.Text = "İsim veya Model Ara...";
                aramadisservis1.Foreground = Brushes.Gray;
            }
        }

        private void dtpServisTarihi_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgdisservisVeriler == null || dtpServisTarihi == null || !dtpServisTarihi.SelectedDate.HasValue)
                return;

            DateTime secilenTarih = dtpServisTarihi.SelectedDate.Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM disserviskayitlarnew 
                                   WHERE KullaniciID = @KullaniciID 
                                   AND CAST(KayitTarihi AS DATE) = @secilenTarih
                                   ORDER BY KayitID DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                    cmd.Parameters.AddWithValue("@secilenTarih", secilenTarih.Date);

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dgdisservisVeriler.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarih filtreleme hatası: " + ex.Message);
            }
        }

        private void btnFiltreTemizle_Click(object sender, RoutedEventArgs e)
        {
            if (dtpServisTarihi == null || dgdisservisVeriler == null || aramadisservis1 == null)
                return;

            dtpServisTarihi.SelectedDate = null;
            aramadisservis1.Text = "İsim veya Model Ara...";
            aramadisservis1.Foreground = Brushes.Gray;
            VerileriGetir("");
        }
    }
}