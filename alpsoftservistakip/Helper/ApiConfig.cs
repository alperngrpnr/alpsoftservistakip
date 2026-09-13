namespace alpsoftservistakip.Helpers
{
    public static class ApiConfig
    {
        public static string BaseUrl = "http://131.222.224.197:5000";
        public static string WebTakipUrl = "https://alpsoftservistakip.com";

        public static string Api => $"{BaseUrl}/api";

        public static string GetTakipUrl(int kayitId) => $"{WebTakipUrl}/takip/SRV-{kayitId:D6}";
        public static string GetTakipUrl(string srvNo) => $"{WebTakipUrl}/takip/{srvNo}";
    }


}
