using System.Threading.Tasks;
using System.Windows;

namespace alpsoftservistakip.Services
{
    /// <summary>
    /// Basit pencere geçişleri için yardımcı servis.
    /// Mevcut kodunuzdaki <c>Services.NavigationService.ShowWindowAsync</c> çağrılarını karşılar.
    /// </summary>
    public static class NavigationService
    {
        /// <summary>
        /// Verilen pencereyi asenkron olarak gösterir ve etkinleştirir.
        /// Şu an için animasyon içermez; ihtiyaç halinde genişletilebilir.
        /// </summary>
        public static Task ShowWindowAsync(Window window)
        {
            if (window == null)
                return Task.CompletedTask;

            if (!window.IsVisible)
            {
                window.Show();
            }

            window.Activate();
            return Task.CompletedTask;
        }
    }
}



