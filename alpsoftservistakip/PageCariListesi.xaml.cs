using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace alpsoftservistakip
{
    public partial class PageCariListesi : Page
    {
        public PageCariListesi()
        {
            InitializeComponent();
            this.Loaded += PageCariListesi_Loaded;
        }

        private async void PageCariListesi_Loaded(object sender, RoutedEventArgs e)
        {
            await CarileriGetirAsync();
        }

        private async Task CarileriGetirAsync(string aramaMetni = "")
        {
            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = "SELECT CariID, AdSoyadUnvan, Telefon, VergiDairesi, ISNULL(GuncelBakiye,0) as GuncelBakiye, " +
                                   "CASE " +
                                   "  WHEN ISNULL(GuncelBakiye,0) > 0 THEN 'Borclu' " +
                                   "  WHEN ISNULL(GuncelBakiye,0) < 0 THEN 'Alacakli' " +
                                   "  ELSE 'Sifir' " +
                                   "END AS BakiyeDurumu " +
                                   "FROM Cariler WHERE SirketID = @sirketId";

                    if (!string.IsNullOrWhiteSpace(aramaMetni))
                    {
                        query += " AND AdSoyadUnvan LIKE @arama";
                    }

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
                        dgCariler.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cariler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            string metin = txtArama.Text;
            await CarileriGetirAsync(metin);
        }

        private async void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            txtArama.Text = "";
            await CarileriGetirAsync();
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.HideOverlay();
            }
        }

        private void btnYeniCari_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageCariEkle());
            }
        }

        private void dgCariler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCariler.SelectedItem is DataRowView row)
            {
                int cariId = Convert.ToInt32(row["CariID"]);
                string adSoyad = row["AdSoyadUnvan"].ToString();

                Window mevcutPencere = Window.GetWindow(this);
                if (mevcutPencere is Window3 window3)
                {
                    window3.ShowOverlayPage(new PageCariDetay(cariId));
                }
            }
        }
    }
}


