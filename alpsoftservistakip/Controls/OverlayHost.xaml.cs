using System.Windows.Controls;

namespace alpsoftservistakip.Controls
{
    /// <summary>
    /// Window'lar içinde sayfa (Page) göstermek için kullanılan overlay host kontrolü.
    /// </summary>
    public partial class OverlayHost : UserControl
    {
        public OverlayHost()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overlay içinde bir sayfa gösterir.
        /// </summary>
        public void ShowPage(Page page)
        {
            if (page == null)
                return;

            Backdrop.Visibility = System.Windows.Visibility.Visible;
            Backdrop.Opacity = 0.6;

            HostFrame.Visibility = System.Windows.Visibility.Visible;
            HostFrame.Opacity = 1;
            HostFrame.Navigate(page);
        }

        /// <summary>
        /// Overlay'i kapatır.
        /// </summary>
        public void HideOverlay()
        {
            HostFrame.Content = null;
            HostFrame.Visibility = System.Windows.Visibility.Collapsed;

            Backdrop.Opacity = 0;
            Backdrop.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}



