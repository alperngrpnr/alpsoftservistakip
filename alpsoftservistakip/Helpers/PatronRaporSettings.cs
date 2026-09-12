using Newtonsoft.Json;
using System;
using System.IO;

namespace alpsoftservistakip.Helpers
{
    public class PatronRaporSettings
    {
        public string GoogleYorumUrl { get; set; } = "";
        public bool GunSonuRaporAktif { get; set; } = true;
        public string GunSonuRaporSaati { get; set; } = "20:00";
        public string PatronTelefon { get; set; } = "";
        public int VarsayilanGarantiGun { get; set; } = 90;

        private static string ConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AlpSoftServisTakip", "patron_settings.json");

        public static PatronRaporSettings Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<PatronRaporSettings>(json) ?? new PatronRaporSettings();
                }
            }
            catch { }
            return new PatronRaporSettings();
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
