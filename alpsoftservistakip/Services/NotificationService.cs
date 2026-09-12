using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;

namespace alpsoftservistakip.Services
{
    public enum NotificationTemplateType
    {
        Kabul,
        FiyatOnay,
        Tamamlandi,
        TeslimEdildi,
        Ozel
    }

    public class NotificationContext
    {
        public string MusteriAdi { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string Cihaz { get; set; } = "";
        public string ServisNo { get; set; } = "";
        public string Ariza { get; set; } = "";
        public string Tutar { get; set; } = "";
        public string Durum { get; set; } = "";
        public string FirmaAdi { get; set; } = "";
        public string FirmaTelefon { get; set; } = "";
        public string TakipUrl { get; set; } = "";
        public string GoogleReviewUrl { get; set; } = "";
    }

    public static class NotificationService
    {
        public static string NormalizePhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0")) digits = digits.Substring(1);
            if (digits.StartsWith("90")) return digits;
            if (digits.Length == 10) return "90" + digits;
            return digits;
        }

        public static string FormatDisplayPhone(string raw)
        {
            string digits = new string((raw ?? "").Where(char.IsDigit).ToArray());
            if (digits.StartsWith("90")) digits = digits.Substring(2);
            if (digits.StartsWith("0")) digits = digits.Substring(1);

            if (digits.Length == 10)
            {
                return $"0{digits.Substring(0, 3)} {digits.Substring(3, 3)} {digits.Substring(6, 2)} {digits.Substring(8, 2)}";
            }
            return raw;
        }

        public static bool SendWhatsApp(string rawPhone, string message, out string error)
        {
            error = string.Empty;
            try
            {
                string norm = NormalizePhone(rawPhone);
                if (string.IsNullOrWhiteSpace(norm) || norm.Length < 10)
                {
                    error = "Geçersiz telefon numarası.";
                    return false;
                }

                string encoded = Uri.EscapeDataString(message ?? "");
                string url = $"https://wa.me/{norm}?text={encoded}";

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static async Task<(bool success, string message)> SendSmsAsync(string rawPhone, string message)
        {
            try
            {
                string norm = NormalizePhone(rawPhone);
                if (string.IsNullOrWhiteSpace(norm) || norm.Length < 10)
                {
                    return (false, "Geçersiz telefon numarası.");
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    var req = new SendSmsRequestDto
                    {
                        Phone = norm,
                        Message = message
                    };

                    string json = JsonConvert.SerializeObject(req);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PostAsync("Settings/send-sms", content);
                    string respBody = await res.Content.ReadAsStringAsync();

                    try
                    {
                        var resObj = JsonConvert.DeserializeObject<dynamic>(respBody);
                        string userMsg = resObj?.message?.ToString() ?? respBody;
                        bool success = res.IsSuccessStatusCode && (resObj?.success == null || (bool)resObj.success);
                        return (success, userMsg);
                    }
                    catch
                    {
                        return (res.IsSuccessStatusCode, respBody);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Hata: " + ex.Message);
            }
        }

        public static async Task<SmsSettingsDto> GetSmsSettingsAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    var res = await client.GetAsync("Settings/sms");
                    if (res.IsSuccessStatusCode)
                    {
                        string json = await res.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<SmsSettingsDto>(json) ?? new SmsSettingsDto();
                    }
                }
            }
            catch { }
            return new SmsSettingsDto();
        }

        public static async Task<bool> SaveSmsSettingsAsync(SmsSettingsDto dto)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    string json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PutAsync("Settings/sms", content);
                    return res.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateMessage(NotificationTemplateType type, NotificationContext ctx)
        {
            string firma = !string.IsNullOrWhiteSpace(ctx.FirmaAdi) ? ctx.FirmaAdi : "AlpSoft Teknik Servis";
            string tel = !string.IsNullOrWhiteSpace(ctx.FirmaTelefon) ? $"\nİletişim: {ctx.FirmaTelefon}" : "";
            string musteri = !string.IsNullOrWhiteSpace(ctx.MusteriAdi) ? ctx.MusteriAdi.Trim() : "Değerli Müşterimiz";
            string cihaz = !string.IsNullOrWhiteSpace(ctx.Cihaz) ? ctx.Cihaz.Trim() : "cihazınız";
            string srvNo = !string.IsNullOrWhiteSpace(ctx.ServisNo) ? $"({ctx.ServisNo}) " : "";

            string takipLink = !string.IsNullOrWhiteSpace(ctx.TakipUrl)
                ? ctx.TakipUrl
                : (!string.IsNullOrWhiteSpace(ctx.ServisNo) ? ApiConfig.GetTakipUrl(ctx.ServisNo) : "");
            string takipBilgi = !string.IsNullOrWhiteSpace(takipLink) ? $"\n📱 Canlı Takip: {takipLink}" : "";

            switch (type)
            {
                case NotificationTemplateType.Kabul:
                    return $"Sayın {musteri}, {cihaz} {srvNo}servisimize kabul edilmiştir. Cihazınız sırasıyla inceleme ve test sürecine alınacaktır.{takipBilgi}\n{firma}{tel}";

                case NotificationTemplateType.FiyatOnay:
                    string tutarStr = !string.IsNullOrWhiteSpace(ctx.Tutar) ? ctx.Tutar : "belirlenen tutar";
                    string arizaStr = !string.IsNullOrWhiteSpace(ctx.Ariza) ? $"Arıza: {ctx.Ariza}\n" : "";
                    return $"Sayın {musteri}, {cihaz} {srvNo}cihazınızın incelemesi tamamlanmıştır.\n{arizaStr}Tahmini Onarım Bedeli: {tutarStr}\nİşleme devam edilmesi için onayınızı rica ederiz.{takipBilgi}\n{firma}{tel}";

                case NotificationTemplateType.Tamamlandi:
                    string tutarBilgi = !string.IsNullOrWhiteSpace(ctx.Tutar) ? $"\nÖdenecek Tutar: {ctx.Tutar}" : "";
                    return $"Sayın {musteri}, {cihaz} {srvNo}cihazınızın tamir ve test işlemleri başarıyla tamamlanmıştır! Cihazınızı servisimizden teslim alabilirsiniz.{tutarBilgi}{takipBilgi}\n{firma}{tel}";

                case NotificationTemplateType.TeslimEdildi:
                    string googleUrl = !string.IsNullOrWhiteSpace(ctx.GoogleReviewUrl) ? ctx.GoogleReviewUrl : PatronRaporSettings.Load().GoogleYorumUrl;
                    string googleYorum = !string.IsNullOrWhiteSpace(googleUrl)
                        ? $"\n\n⭐ Hizmetimizden memnun kaldıysanız Google'da bizi 5 yıldızla değerlendirerek destek olur musunuz? 👇\n{googleUrl}"
                        : "";
                    return $"Sayın {musteri}, {cihaz} {srvNo}cihazınız tarafınıza teslim edilmiştir. {firma} olarak bizi tercih ettiğiniz için teşekkür eder, sağlıklı günler dileriz.{googleYorum}\n{firma}{tel}";

                case NotificationTemplateType.Ozel:
                default:
                    return $"Sayın {musteri}, {cihaz} cihazınız hakkında bilgilendirme: {takipBilgi}\n\n{firma}{tel}";
            }
        }

        public static async Task<(bool success, string message)> SendGunSonuRaporuAsync(string rawPhone = null)
        {
            try
            {
                var settings = PatronRaporSettings.Load();
                string targetPhone = !string.IsNullOrWhiteSpace(rawPhone) ? rawPhone : settings.PatronTelefon;
                if (string.IsNullOrWhiteSpace(targetPhone))
                {
                    return (false, "Patron telefon numarası tanımlanmamış. Lütfen Ayarlar ekranından numara giriniz.");
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    var res = await client.GetAsync("Muhasebe/gun-sonu-patron-ozet");
                    if (!res.IsSuccessStatusCode)
                    {
                        return (false, "API'den gün sonu verileri alınamadı: " + res.StatusCode);
                    }

                    string json = await res.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    string waMsg = data?.whatsAppMesaj?.ToString() ?? "";

                    if (string.IsNullOrWhiteSpace(waMsg))
                    {
                        return (false, "Rapor metni oluşturulamadı.");
                    }

                    bool sent = SendWhatsApp(targetPhone, waMsg, out string err);
                    if (!sent)
                    {
                        return (false, "WhatsApp açılamadı: " + err);
                    }
                    return (true, "Gün sonu raporu WhatsApp üzerinden hazırlandı.");
                }
            }
            catch (Exception ex)
            {
                return (false, "Hata: " + ex.Message);
            }
        }
    }
}
