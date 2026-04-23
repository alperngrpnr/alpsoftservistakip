namespace alpsoftservistakip
{
    public static class Class1
    {
        public static KullaniciModel AktifKullanici { get; set; } = new KullaniciModel();
        // Bazı kodlarınızda 'Bilgi' olarak çağırdığınız için bunu da ekledik
        public static KullaniciModel Bilgi { get; set; } = AktifKullanici;
    }

    public class KullaniciModel
    {
        public int ID { get; set; }
        public int SirketID { get; set; }
        public string AdSoyad { get; set; }
        public string KullaniciAdi { get; set; }
        public string SirketAdi { get; set; }
        public bool IsAdmin { get; set; }
        public bool HasStokTakibi { get; set; }
    }
}
