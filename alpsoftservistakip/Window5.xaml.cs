using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;

namespace alpsoftservistakip
{
    public partial class Window5 : Window
    {
        private readonly string connString =
            "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";

        public Window5()
        {
            InitializeComponent();
            KayitlariListele();
            this.Closing += AltPencere_Closing;
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            if (Application.Current.MainWindow != this)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
                this.Hide();
            }
        }

        public void KayitlariListele(string aramaMetni = "")
        {
            if (dgVeriler == null) return;

            // Güvenlik kontrolü
            if (Class1.AktifKullanici == null)
            {
                MessageBox.Show("Oturum bilgisi bulunamadı.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // NOT: Eğer veritabanında 'CompanyId' sütunu yoksa ve hata alıyorsanız,
                    // SQL tablonuza şu komutu çalıştırın: ALTER TABLE kayitlicihazlar ADD CompanyId INT;
                    string sql = @"
                        SELECT * FROM kayitlicihazlar
                        WHERE CompanyId = @SirketID
                        AND (@arama = '' OR
                             IsimSoyisim LIKE '%' + @arama + '%' OR
                             BayiAdi LIKE '%' + @arama + '%' OR
                             CepTelefonu LIKE '%' + @arama + '%')
                        ORDER BY ID DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@arama", aramaMetni ?? "");
                        // Class1'den gelen şirket ID'sini kullanıyoruz
                        cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        dgVeriler.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata Invalid column name ise kullanıcıya SQL uyarısı ver
                if (ex.Message.Contains("CompanyId"))
                {
                    MessageBox.Show("Veritabanı hatası: 'CompanyId' sütunu bulunamadı. Lütfen SQL tablonuza bu sütunu ekleyin.");
                }
                else
                {
                    MessageBox.Show("Veri çekme hatası: " + ex.Message);
                }
            }
        }

        private void _MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgVeriler.SelectedItem is DataRowView row)
            {
                int id = Convert.ToInt32(row["ID"]);
                Window9 detay = new Window9(id);
                detay.ShowDialog();
                VerileriYukle();
            }
        }

        private void geridön_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void arama_GotFocus(object sender, RoutedEventArgs e)
        {
            if (arama.Text.Trim() == "Servis kaydı ara...")
            {
                arama.Text = "";
                arama.Foreground = Brushes.Black;
            }
        }

        private void arama_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(arama.Text))
            {
                arama.Text = "Servis kaydı ara...";
                arama.Foreground = Brushes.Gray;
            }
        }

        private void arama_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgVeriler == null || arama.Text == "Servis kaydı ara...") return;
            KayitlariListele(arama.Text);
        }

        public void VerileriYukle()
        {
            string mevcutArama = (arama != null && arama.Text != "Servis kaydı ara...") ? arama.Text : "";
            KayitlariListele(mevcutArama);
        }

        private void dtpServisTarihi_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVeriler == null || dtpServisTarihi == null || !dtpServisTarihi.SelectedDate.HasValue)
                return;

            DateTime secilenTarih = dtpServisTarihi.SelectedDate.Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM kayitlicihazlar 
                                   WHERE CompanyId = @SirketID 
                                   AND CAST(KayitTarihi AS DATE) = @secilenTarih
                                   ORDER BY ID DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {// Window5 içindeki parametre satırı tam olarak bu olmalı:
                        
                        cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);
                        cmd.Parameters.AddWithValue("@secilenTarih", secilenTarih.Date);

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        dgVeriler.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarih filtreleme hatası: " + ex.Message);
            }
        }

        private void btnFiltreTemizle_Click(object sender, RoutedEventArgs e)
        {
            if (dtpServisTarihi == null || dgVeriler == null || arama == null) return;

            dtpServisTarihi.SelectedDate = null;
            arama.Text = "Servis kaydı ara...";
            arama.Foreground = Brushes.Gray;
            KayitlariListele("");
        }
    }
}