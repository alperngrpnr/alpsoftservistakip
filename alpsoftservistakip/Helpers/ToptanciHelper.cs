using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using alpsoftservistakip.Helpers;
using Newtonsoft.Json;

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

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var payload = new
                    {
                        ToptanciID = toptanciId,
                        KayitID = kayitId,
                        ParcaAdi = parcaAdi,
                        Adet = adet,
                        BirimFiyati = birimFiyati,
                        Aciklama = aciklama
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Toptanci/parcaekle", content);
                    
                    return response.IsSuccessStatusCode;
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Toptanci/{toptanciId}/borc");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        if (decimal.TryParse(result, out decimal borc))
                            return borc;
                    }
                    return 0;
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Toptanci/{toptanciId}/tutar");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        if (decimal.TryParse(result, out decimal tutar))
                            return tutar;
                    }
                    return 0;
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var payload = new { OdenecekTutar = odenecekTutar };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Toptanci/odeme/{parcaAlimId}", content);
                    
                    return response.IsSuccessStatusCode;
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


