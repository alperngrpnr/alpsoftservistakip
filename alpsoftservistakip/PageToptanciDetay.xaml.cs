using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public partial class PageToptanciDetay : Page
    {
        private int _toptanciId;

        public PageToptanciDetay(int toptanciId)
        {
            InitializeComponent();
            _toptanciId = toptanciId;
            this.Loaded += async (s, e) => await YukleVeriAsync();
        }

        private async Task YukleVeriAsync()
        {
            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    // TOPTANCI BİLGİLERİNİ YÜKLEYİN
                    string toptanciQuery = @"
                        SELECT FirmaAdi, Telefon, IBAN
                        FROM Toptancilar
                        WHERE ID = @id AND SirketID = @sirketId";

                    using (SqlCommand cmd = new SqlCommand(toptanciQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _toptanciId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                txtToptanciAdi.Text = dr["FirmaAdi"]?.ToString() ?? "";
                                txtTelefon.Text = dr["Telefon"]?.ToString() ?? "-";
                                txtIBAN.Text = dr["IBAN"]?.ToString() ?? "-";
                            }
                        }
                    }

                    // PARÇA ALIMLARI TABLOSUNU YÜKLEYİN
                    string parcaQuery = @"
                        SELECT ID, ParcaAdi, Adet, BirimFiyati, ToplamTutar, OdenenTutar, BorcluTutar, IslemTarihi
                        FROM ToptanciParcaAlimlar
                        WHERE ToptanciID = @toptanciId AND SirketID = @sirketId
                        ORDER BY IslemTarihi DESC";

                    using (SqlCommand cmd = new SqlCommand(parcaQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@toptanciId", _toptanciId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        await Task.Run(() => da.Fill(dt));
                        dgParcaAlimlar.ItemsSource = dt.DefaultView;
                    }

                    // ÖZET HESAPLARINı YAP
                    await HesaplaOzetAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task HesaplaOzetAsync()
        {
            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    string query = @"
                        SELECT 
                            ISNULL(SUM(Adet), 0) AS ToplamAdet,
                            ISNULL(SUM(ToplamTutar), 0) AS ToplamHarcanan,
                            ISNULL(SUM(BorcluTutar), 0) AS ToplamBorc
                        FROM ToptanciParcaAlimlar
                        WHERE ToptanciID = @toptanciId AND SirketID = @sirketId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@toptanciId", _toptanciId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                int toplamAdet = Convert.ToInt32(dr["ToplamAdet"]);
                                decimal toplamHarcanan = Convert.ToDecimal(dr["ToplamHarcanan"]);
                                decimal toplamBorc = Convert.ToDecimal(dr["ToplamBorc"]);

                                txtToplamAdet.Text = $"{toplamAdet} ADET";
                                txtToplamTutar.Text = $"$ {toplamHarcanan:N2}";
                                txtToplamBorc.Text = $"$ {toplamBorc:N2}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hesaplamalar sırasında hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciListesi());
            }
        }
    }
}


