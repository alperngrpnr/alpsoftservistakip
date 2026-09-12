using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Deployment.Application;


namespace alpsoftservistakip
{
    /// <summary>
    /// Window10.xaml etkileşim mantığı
    /// </summary>
    public partial class Window10 : Window
    {

        
        public Window10()
        {
            InitializeComponent();
            Loaded += Window10_Loaded;
        }

        private async void Window10_Loaded(object sender, RoutedEventArgs e)
        {
            
            await Task.Delay(2500); // 2.5 saniye splash

            LoginHostWindow login = new LoginHostWindow();
            login.Show();

            this.Close();


        }
        
    }
}
