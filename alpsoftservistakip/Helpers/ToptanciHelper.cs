using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace alpsoftservistakip
{
    /// <summary>
    /// Toptancı işlemleri için yardımcı sınıf
    /// </summary>
    public static class ToptanciHelper
    {
        /// <summary>
        /// Servis kaydında parça ekleme işlemi sırasında otomatik olarak toptancıdan alım kaydı oluşturur
        /// </summary>
        public static async Task<bool> ToptanciParcasiEkleAsync(
            int sirketId, 
            int toptanciId, 
            int? kayitId,
            string parcaAdi, 
            int adet, 
            decimal birimFiyati,
            string aciklama = "")
        {
            try
            {
                if (toptanciId <= 0 || adet <= 0 || birimFiyati < 0)
                    return false;

                decimal toplamTutar = adet * birimFiyati;

                using (SqlConnection con = alpsoftservistakip.Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    string query = @"
                        INSERT INTO ToptanciParcaAlimlar 
                            (SirketID, ToptanciID, KayitID, ParcaAdi, Adet, BirimFiyati, ToplamTutar, BorcluTutar, Aciklama, IslemTarihi)
                        VALUES 
                            (@sirketId, @toptanciId, @kayitId, @parcaAdi, @adet, @birimFiyati, @toplamTutar, @toplamTutar, @aciklama, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@sirketId", sirketId);
                        cmd.Parameters.AddWithValue("@toptanciId", toptanciId);
                        cmd.Parameters.AddWithValue("@kayitId", kayitId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@parcaAdi", parcaAdi);
                        cmd.Parameters.AddWithValue("@adet", adet);
                        cmd.Parameters.AddWithValue("@birimFiyati", birimFiyati);
                        cmd.Parameters.AddWithValue("@toplamTutar", toplamTutar);
                        cmd.Parameters.AddWithValue("@aciklama", string.IsNullOrEmpty(aciklama) ? (object)DBNull.Value : aciklama);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Toptancı parçası ekleme hatası: {ex.Message}", "Hata", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Toptancıya olan toplam borcu hesaplar
        /// </summary>
        public static async Task<decimal> ToplamBorcHesaplaAsync(int sirketId, int toptanciId)
        {
            try
            {
                using (SqlConnection con = alpsoftservistakip.Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"
                        SELECT ISNULL(SUM(BorcluTutar), 0)
                        FROM ToptanciParcaAlimlar
                        WHERE SirketID = @sirketId AND ToptanciID = @toptanciId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@sirketId", sirketId);
                        cmd.Parameters.AddWithValue("@toptanciId", toptanciId);

                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToDecimal(result ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Borç hesaplama hatası: {ex.Message}", "Hata", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return 0;
            }
        }

        /// <summary>
        /// Toptancıdan alınan toplam tutarı hesaplar
        /// </summary>
        public static async Task<decimal> ToplamTutarHesaplaAsync(int sirketId, int toptanciId)
        {
            try
            {
                using (SqlConnection con = alpsoftservistakip.Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"
                        SELECT ISNULL(SUM(ToplamTutar), 0)
                        FROM ToptanciParcaAlimlar
                        WHERE SirketID = @sirketId AND ToptanciID = @toptanciId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@sirketId", sirketId);
                        cmd.Parameters.AddWithValue("@toptanciId", toptanciId);

                        object result = await cmd.ExecuteScalarAsync();
                        return Convert.ToDecimal(result ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Tutar hesaplama hatası: {ex.Message}", "Hata", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return 0;
            }
        }

        /// <summary>
        /// Ödeme kaydı yapıldığında borçlu tutarı günceller
        /// </summary>
        public static async Task<bool> OdemeKaydeAsync(int parcaAlimId, decimal odenecekTutar, int sirketId)
        {
            try
            {
                using (SqlConnection con = alpsoftservistakip.Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    // Mevcut tutarları getir
                    string selectQuery = @"
                        SELECT OdenenTutar, ToplamTutar
                        FROM ToptanciParcaAlimlar
                        WHERE ID = @id AND SirketID = @sirketId";

                    decimal odenenTutar = 0;
                    decimal toplamTutar = 0;

                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", parcaAlimId);
                        cmd.Parameters.AddWithValue("@sirketId", sirketId);

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                odenenTutar = Convert.ToDecimal(dr["OdenenTutar"]);
                                toplamTutar = Convert.ToDecimal(dr["ToplamTutar"]);
                            }
                        }
                    }

                    // Yeni ödenen tutar
                    decimal yeniOdenenTutar = odenenTutar + odenecekTutar;
                    if (yeniOdenenTutar > toplamTutar)
                        yeniOdenenTutar = toplamTutar;

                    decimal yeniBorcluTutar = toplamTutar - yeniOdenenTutar;

                    // Güncelle
                    string updateQuery = @"
                        UPDATE ToptanciParcaAlimlar
                        SET OdenenTutar = @odenenTutar, BorcluTutar = @borcluTutar, GuncellemeTarihi = GETDATE()
                        WHERE ID = @id AND SirketID = @sirketId";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", parcaAlimId);
                        cmd.Parameters.AddWithValue("@odenenTutar", yeniOdenenTutar);
                        cmd.Parameters.AddWithValue("@borcluTutar", yeniBorcluTutar);
                        cmd.Parameters.AddWithValue("@sirketId", sirketId);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ödeme kaydı hatası: {ex.Message}", "Hata", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}


