using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public partial class PageToptanciEkle : Page
    {
        private int? _toptanciId = null;
        private Func<Task> _yenilemeFonksiyonu;

        public PageToptanciEkle(int? toptanciId = null, Func<Task> yenilemeFonksiyonu = null)
        {
            InitializeComponent();
            _toptanciId = toptanciId;
            _yenilemeFonksiyonu = yenilemeFonksiyonu;

            if (_toptanciId.HasValue)
            {
                txtUstBaslik.Text = "Toptancı Düzenle";
                btnKaydet.Content = "GÜNCELLE";
                _ = YukleVeriAsync();
            }
        }

        private async Task YukleVeriAsync()
        {
            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"
                        SELECT FirmaAdi, Telefon, IBAN, Aciklama
                        FROM Toptancilar
                        WHERE ID = @id AND SirketID = @sirketId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _toptanciId.Value);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                txtFirmaAdi.Text = dr["FirmaAdi"]?.ToString() ?? "";
                                txtTelefon.Text = dr["Telefon"]?.ToString() ?? "";
                                txtIBAN.Text = dr["IBAN"]?.ToString() ?? "";
                                txtAciklama.Text = dr["Aciklama"]?.ToString() ?? "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string firmaAdi = txtFirmaAdi.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string iban = txtIBAN.Text.Trim();
            string aciklama = txtAciklama.Text.Trim();

            if (string.IsNullOrWhiteSpace(firmaAdi))
            {
                MessageBox.Show("Toptancı adı boş bırakılamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    if (_toptanciId.HasValue)
                    {
                        // GÜNCELLE
                        string query = @"
                            UPDATE Toptancilar
                            SET FirmaAdi = @firmaAdi, Telefon = @telefon, IBAN = @iban, 
                                Aciklama = @aciklama, GuncellemeTarihi = GETDATE()
                            WHERE ID = @id AND SirketID = @sirketId";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@id", _toptanciId.Value);
                            cmd.Parameters.AddWithValue("@firmaAdi", firmaAdi);
                            cmd.Parameters.AddWithValue("@telefon", string.IsNullOrEmpty(telefon) ? (object)DBNull.Value : telefon);
                            cmd.Parameters.AddWithValue("@iban", string.IsNullOrEmpty(iban) ? (object)DBNull.Value : iban);
                            cmd.Parameters.AddWithValue("@aciklama", string.IsNullOrEmpty(aciklama) ? (object)DBNull.Value : aciklama);
                            cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        MessageBox.Show("Toptancı başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // YENİ EKLE
                        string query = @"
                            INSERT INTO Toptancilar (SirketID, FirmaAdi, Telefon, IBAN, Aciklama)
                            VALUES (@sirketId, @firmaAdi, @telefon, @iban, @aciklama)";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                            cmd.Parameters.AddWithValue("@firmaAdi", firmaAdi);
                            cmd.Parameters.AddWithValue("@telefon", string.IsNullOrEmpty(telefon) ? (object)DBNull.Value : telefon);
                            cmd.Parameters.AddWithValue("@iban", string.IsNullOrEmpty(iban) ? (object)DBNull.Value : iban);
                            cmd.Parameters.AddWithValue("@aciklama", string.IsNullOrEmpty(aciklama) ? (object)DBNull.Value : aciklama);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        MessageBox.Show("Toptancı başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (_yenilemeFonksiyonu != null)
                {
                    await _yenilemeFonksiyonu();
                }

                GeriDon();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Bu toptancı adı zaten mevcut!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            GeriDon();
        }

        private void GeriDon()
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciListesi());
            }
        }
    }
}


