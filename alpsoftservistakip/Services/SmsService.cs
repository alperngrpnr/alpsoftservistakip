using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace alpsoftservistakip.Services
{
    public static class SmsService
    {
        public static bool SendRepairCompletedSms(string phone, string customerName, string deviceInfo, out string responseMessage)
        {
            responseMessage = string.Empty;

            string apiUrlTemplate = ConfigurationManager.AppSettings["SmsApiUrlTemplate"];
            if (string.IsNullOrWhiteSpace(apiUrlTemplate))
            {
                responseMessage = "SMS ayarları yapılmamış. App.config içine SmsApiUrlTemplate ekleyin.";
                return false;
            }

            string normalizedPhone = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                responseMessage = "Telefon numarası geçersiz.";
                return false;
            }

            string text = string.Format(
                "Sayin {0}, {1} cihazinizin tamiri tamamlanmistir. Cihazinizi teslim alabilirsiniz. Tesekkurler.",
                (customerName ?? string.Empty).Trim(),
                (deviceInfo ?? "cihaz").Trim());

            string url = apiUrlTemplate
                .Replace("{to}", Uri.EscapeDataString(normalizedPhone))
                .Replace("{message}", Uri.EscapeDataString(text));

            string user = ConfigurationManager.AppSettings["SmsUser"];
            string pass = ConfigurationManager.AppSettings["SmsPass"];
            string header = ConfigurationManager.AppSettings["SmsHeader"];

            if (!string.IsNullOrWhiteSpace(user))
                url = url.Replace("{user}", Uri.EscapeDataString(user));
            if (!string.IsNullOrWhiteSpace(pass))
                url = url.Replace("{pass}", Uri.EscapeDataString(pass));
            if (!string.IsNullOrWhiteSpace(header))
                url = url.Replace("{header}", Uri.EscapeDataString(header));

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string response = client.DownloadString(url);
                    responseMessage = response;
                    return true;
                }
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
                return false;
            }
        }

        private static string NormalizePhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0")) digits = digits.Substring(1);
            if (digits.StartsWith("90")) return digits;
            if (digits.Length == 10) return "90" + digits;
            return digits;
        }
    }
}


