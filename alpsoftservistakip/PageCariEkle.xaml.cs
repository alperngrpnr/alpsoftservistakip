using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public partial class PageCariEkle : Page
    {
        public PageCariEkle()
        {
            InitializeComponent();
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string unvan = txtUnvan.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string email = txtEmail.Text.Trim();
            string vergiDairesi = txtVergiDairesi.Text.Trim();
            string vergiNo = txtVergiNo.Text.Trim();
            string adres = txtAdres.Text.Trim();

            if (string.IsNullOrWhiteSpace(unvan))
            {
                MessageBox.Show("Firma ünvanı / Müşteri adı boş bırakılamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"INSERT INTO Cariler (SirketID, AdSoyadUnvan, Telefon, Email, Adres, VergiDairesi, VergiNo) 
                                     VALUES (@sirketId, @unvan, @telefon, @email, @adres, @vd, @vno)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        cmd.Parameters.AddWithValue("@unvan", unvan);
                        cmd.Parameters.AddWithValue("@telefon", string.IsNullOrEmpty(telefon) ? (object)DBNull.Value : telefon);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                        cmd.Parameters.AddWithValue("@adres", string.IsNullOrEmpty(adres) ? (object)DBNull.Value : adres);
                        cmd.Parameters.AddWithValue("@vd", string.IsNullOrEmpty(vergiDairesi) ? (object)DBNull.Value : vergiDairesi);
                        cmd.Parameters.AddWithValue("@vno", string.IsNullOrEmpty(vergiNo) ? (object)DBNull.Value : vergiNo);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Cari kart başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                GeriDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cari eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            GeriDon();
        }

        private void txtTelefon_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Sadece sayılara izin ver
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void GeriDon()
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                // Mevcut overlay sayfasını CariListesi olarak güncelliyoruz
                window3.ShowOverlayPage(new PageCariListesi());
            }
        }
    }
}


