using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml";
        string t = File.ReadAllText(p);

        string search = "Title=\"AlpSoft Servis Takip v1.6.2\" \r\n        ResizeMode=\"CanResize\" \r\n        WindowState=\"Maximized\" \r\n        WindowStartupLocation=\"CenterScreen\"\r\n        MinHeight=\"720\"\r\n        MinWidth=\"1080\" Closing=\"Window_Closing\">";

        string replace = "Title=\"AlpSoft Servis Takip v1.6.2\" \r\n        WindowStyle=\"None\" AllowsTransparency=\"True\" Background=\"Transparent\" \r\n        ResizeMode=\"CanResize\" \r\n        WindowState=\"Maximized\" \r\n        WindowStartupLocation=\"CenterScreen\"\r\n        MinHeight=\"720\"\r\n        MinWidth=\"1080\" Closing=\"Window_Closing\">";

        if (t.Contains(search))
        {
            t = t.Replace(search, replace);
        }
        else
        {
            Console.WriteLine("Search not found! Trying flexible fallback...");
            t = t.Replace("Title=\"AlpSoft Servis Takip v1.6.2\"", "Title=\"AlpSoft Servis Takip v1.6.2\" WindowStyle=\"None\" AllowsTransparency=\"True\" Background=\"Transparent\"");
        }

        // Add custom Title Bar to the Grid inside Viewbox
        string viewboxGridStart = "<Grid HorizontalAlignment=\"Center\" Width=\"1920\" Height=\"1080\" VerticalAlignment=\"Center\">";
        string customMenu = @"<Grid HorizontalAlignment=""Center"" Width=""1920"" Height=""1080"" VerticalAlignment=""Center"">
            <!-- Custom Title Bar -->
            <Grid Height=""40"" VerticalAlignment=""Top"" Panel.ZIndex=""999"" Background=""#FF1A1C24"" MouseLeftButtonDown=""Window_MouseDown"">
                <TextBlock Text=""AlpSoft Servis Takip v1.6.2"" Foreground=""White"" FontWeight=""Bold"" FontFamily=""Segoe UI"" FontSize=""16"" VerticalAlignment=""Center"" Margin=""20,0,0,0""/>
                <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"" Margin=""0,0,10,0"">
                    <Button Content=""—"" Foreground=""White"" Background=""Transparent"" BorderThickness=""0"" FontSize=""18"" FontWeight=""Bold"" Width=""45"" Click=""btnMinimize_Click"" Cursor=""Hand""/>
                    <Button Content=""🗗"" Foreground=""White"" Background=""Transparent"" BorderThickness=""0"" FontSize=""16"" Width=""45"" Click=""btnMaximize_Click"" Cursor=""Hand""/>
                    <Button Content=""X"" Foreground=""White"" Background=""Transparent"" BorderThickness=""0"" FontSize=""18"" FontWeight=""Bold"" Width=""45"" Click=""btnClose_Click"" Cursor=""Hand"">
                        <Button.Style>
                            <Style TargetType=""Button"">
                                <Style.Triggers>
                                    <Trigger Property=""IsMouseOver"" Value=""True"">
                                        <Setter Property=""Background"" Value=""#FFE81123""/>
                                    </Trigger>
                                </Style.Triggers>
                            </Style>
                        </Button.Style>
                    </Button>
                </StackPanel>
            </Grid>";

        if (t.Contains(viewboxGridStart) && !t.Contains("Window_MouseDown"))
        {
            t = t.Replace(viewboxGridStart, customMenu);
        }

        File.WriteAllText(p, t);
        Console.WriteLine("Done in XAML");

        string pcs = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml.cs";
        string tcs = File.ReadAllText(pcs);

        string codesToInject = @"        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }";

        if (!tcs.Contains("btnMinimize_Click"))
        {
            tcs = tcs.Replace("public partial class Window3 : Window\r\n    {", "public partial class Window3 : Window\r\n    {\r\n" + codesToInject + "\r\n");
            File.WriteAllText(pcs, tcs);
        }
    }
}
