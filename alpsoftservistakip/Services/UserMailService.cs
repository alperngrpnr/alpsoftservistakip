using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Authentication;

namespace alpsoftservistakip.Services
{
    public class UserMailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SenderDisplayName { get; set; }
    }

    public static class UserMailService
    {
        public static void EnsureTable()
        {
            using (SqlConnection con = global::alpsoftservistakip.alpsoftservistakip.Veritabani.BaglantiAl())
            {
                con.Open();
                string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.KullaniciMailAyarlari') AND type in (N'U'))
BEGIN
    CREATE TABLE KullaniciMailAyarlari (
        ID INT PRIMARY KEY IDENTITY(1,1),
        KullaniciID INT NOT NULL UNIQUE,
        SmtpHost NVARCHAR(255) NOT NULL,
        SmtpPort INT NOT NULL DEFAULT 587,
        EnableSsl BIT NOT NULL DEFAULT 1,
        SenderEmail NVARCHAR(255) NOT NULL,
        SenderPassword NVARCHAR(255) NOT NULL,
        SenderDisplayName NVARCHAR(255),
        GuncellemeTarihi DATETIME NOT NULL DEFAULT GETDATE()
    );
END";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static UserMailSettings GetSettings(int kullaniciId)
        {
            EnsureTable();

            using (SqlConnection con = global::alpsoftservistakip.alpsoftservistakip.Veritabani.BaglantiAl())
            {
                con.Open();
                string sql = @"SELECT TOP 1 SmtpHost, SmtpPort, EnableSsl, SenderEmail, SenderPassword, SenderDisplayName
                               FROM KullaniciMailAyarlari
                               WHERE KullaniciID = @uid";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@uid", kullaniciId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new UserMailSettings
                            {
                                SmtpHost = dr["SmtpHost"]?.ToString(),
                                SmtpPort = Convert.ToInt32(dr["SmtpPort"]),
                                EnableSsl = Convert.ToBoolean(dr["EnableSsl"]),
                                SenderEmail = dr["SenderEmail"]?.ToString(),
                                SenderPassword = dr["SenderPassword"]?.ToString(),
                                SenderDisplayName = dr["SenderDisplayName"]?.ToString()
                            };
                        }
                    }
                }
            }

            return new UserMailSettings
            {
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                EnableSsl = true,
                SenderEmail = string.Empty,
                SenderPassword = string.Empty,
                SenderDisplayName = string.Empty
            };
        }

        public static void SaveSettings(int kullaniciId, UserMailSettings settings)
        {
            EnsureTable();

            using (SqlConnection con = global::alpsoftservistakip.alpsoftservistakip.Veritabani.BaglantiAl())
            {
                con.Open();
                string sql = @"
IF EXISTS (SELECT 1 FROM KullaniciMailAyarlari WHERE KullaniciID = @uid)
BEGIN
    UPDATE KullaniciMailAyarlari
    SET SmtpHost = @host,
        SmtpPort = @port,
        EnableSsl = @ssl,
        SenderEmail = @mail,
        SenderPassword = @pass,
        SenderDisplayName = @name,
        GuncellemeTarihi = GETDATE()
    WHERE KullaniciID = @uid;
END
ELSE
BEGIN
    INSERT INTO KullaniciMailAyarlari (KullaniciID, SmtpHost, SmtpPort, EnableSsl, SenderEmail, SenderPassword, SenderDisplayName)
    VALUES (@uid, @host, @port, @ssl, @mail, @pass, @name);
END";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@uid", kullaniciId);
                    cmd.Parameters.AddWithValue("@host", (object)settings.SmtpHost ?? "");
                    cmd.Parameters.AddWithValue("@port", settings.SmtpPort);
                    cmd.Parameters.AddWithValue("@ssl", settings.EnableSsl);
                    cmd.Parameters.AddWithValue("@mail", (object)settings.SenderEmail ?? "");
                    cmd.Parameters.AddWithValue("@pass", (object)settings.SenderPassword ?? "");
                    cmd.Parameters.AddWithValue("@name", (object)settings.SenderDisplayName ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool TrySend(UserMailSettings settings, string toEmail, string subject, string body, out string error)
        {
            error = string.Empty;

            if (settings == null)
            {
                error = "Mail ayarları bulunamadı.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.SenderEmail) || string.IsNullOrWhiteSpace(settings.SenderPassword))
            {
                error = "SMTP Host, Gönderici Mail ve Şifre alanları zorunludur.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                error = "Alıcı mail adresi boş.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                error = "Mail konusu boş bırakılamaz.";
                return false;
            }

            try
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;

                using (MailMessage msg = new MailMessage())
                {
                    msg.From = new MailAddress(
                        settings.SenderEmail,
                        string.IsNullOrWhiteSpace(settings.SenderDisplayName) ? settings.SenderEmail : settings.SenderDisplayName);

                    msg.To.Add(toEmail);
                    msg.Subject = subject;

                    string formattedBody = (body ?? "").Replace("\n", "<br/>");
                    string companyName = string.IsNullOrWhiteSpace(settings.SenderDisplayName) ? settings.SenderEmail : settings.SenderDisplayName;
                    string htmlTemplate = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f6; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 40px auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: 1px solid #e1e8ed; }}
        .header {{ background-color: #2c3e50; padding: 25px 30px; text-align: left; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 24px; font-weight: 600; }}
        .content {{ padding: 35px 30px; color: #333333; line-height: 1.6; font-size: 16px; border-bottom: 2px solid #f0f4f8; }}
        .footer {{ padding: 20px 30px; background-color: #f8fafc; font-size: 13px; color: #718096; text-align: center; }}
        .footer p {{ margin: 5px 0; }}
        .highlight {{ color: #e74c3c; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚙️ {companyName}</h1>
        </div>
        <div class='content'>
            {formattedBody}
        </div>
        <div class='footer'>
            <p>Bu e-posta <strong>{companyName}</strong> tarafından bilgilendirme amacıyla gönderilmiştir.</p>
            <p>Lütfen bu e-postayı yanıtlamayınız.</p>
        </div>
    </div>
</body>
</html>";

                    msg.Body = htmlTemplate;
                    msg.IsBodyHtml = true;

                    using (SmtpClient client = new SmtpClient(settings.SmtpHost, settings.SmtpPort))
                    {
                        client.EnableSsl = settings.EnableSsl;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(settings.SenderEmail, settings.SenderPassword);
                        client.Timeout = 30000;
                        client.Send(msg);
                    }
                }

                return true;
            }
            catch (SmtpException ex)
            {
                error = "SMTP Hatası: " + ex.Message + "\nİpucu: Gmail için SMTP='smtp.gmail.com', Port=587, SSL=On ve App Password kullanın.";
                return false;
            }
            catch (AuthenticationException ex)
            {
                error = "Kimlik doğrulama/TLS hatası: " + ex.Message + "\nİpucu: SSL açık olmalı ve uygulama şifresi kullanılmalı.";
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}


