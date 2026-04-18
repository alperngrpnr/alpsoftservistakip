using System;
using System.Configuration;
using System.Data.SqlClient;

namespace alpsoftservistakip
{
    public static class Veritabani
    {
        private static readonly string _connString;

        static Veritabani()
        {
            // App.config'deki "AlpsoftConn" isimli yapılandırmayı alır
            var connectionSettings = ConfigurationManager.ConnectionStrings["AlpsoftConn"];

            if (connectionSettings == null)
            {
                throw new ConfigurationErrorsException("App.config dosyasında 'AlpsoftConn' isimli bağlantı dizesi (connection string) bulunamadı. Lütfen App.config dosyanızı kontrol edin.");
            }

            _connString = connectionSettings.ConnectionString;
        }

        public static SqlConnection BaglantiAl()
        {
            return new SqlConnection(_connString);
        }
    }
}

