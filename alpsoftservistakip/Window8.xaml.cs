using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace alpsoftservistakip
{
    public partial class Window8 : Window
    {
        public Window8()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((Storyboard)Resources["RotateAnim"]).Begin();
            ((Storyboard)Resources["FadeAnim"]).Begin();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Kullanıcı kapatamasın
            e.Cancel = true;
        }
        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Hangi pencere olursa olsun, çarpıya basıldığında tüm uygulamayı kapatır
            Application.Current.Shutdown();
        }
    }
}
