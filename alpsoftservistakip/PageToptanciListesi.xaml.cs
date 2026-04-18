using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace alpsoftservistakip
{
    public partial class PageToptanciListesi : Page
    {
        public PageToptanciListesi()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => await YukleVeriAsync();
        }

        private async Task YukleVeriAsync(string aramaMetni = "")
        {
            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"
                        SELECT ID, FirmaAdi, Telefon, IBAN, OlusturulmaTarihi
                        FROM Toptancilar
                        WHERE SirketID = @sirketId";

                    if (!string.IsNullOrWhiteSpace(aramaMetni))
                    {
                        query += " AND FirmaAdi LIKE @arama";
                    }

                    query += " ORDER BY FirmaAdi ASC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        if (!string.IsNullOrWhiteSpace(aramaMetni))
                        {
                            cmd.Parameters.AddWithValue("@arama", "%" + aramaMetni + "%");
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        await Task.Run(() => da.Fill(dt));
                        dgToptancilar.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            string metin = txtArama.Text;
            await YukleVeriAsync(metin);
        }

        private async void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            txtArama.Text = "";
            await YukleVeriAsync();
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.HideOverlay();
            }
        }

        private void btnYeniToptanci_Click(object sender, RoutedEventArgs e)
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciEkle(null, () => YukleVeriAsync()));
            }
        }

        private void dgToptancilar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToptancilar.SelectedItem is DataRowView row)
            {
                int toptanciId = Convert.ToInt32(row["ID"]);
                Window pencere = Window.GetWindow(this);
                if (pencere is Window3 window3)
                {
                    window3.ShowOverlayPage(new PageToptanciDetay(toptanciId));
                }
            }
        }
    }
}


