using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml";
        string t = File.ReadAllText(p);

        // Update UniformGrid Columns from 5 to 7, and widen it if needed to fit everything nicely
        t = Regex.Replace(t, @"<UniformGrid Columns=""5"".*?Width=""1700"">", "<UniformGrid Columns=\"7\" VerticalAlignment=\"Top\" HorizontalAlignment=\"Center\" Margin=\"0,582,0,0\" Panel.ZIndex=\"14\" Width=\"1800\">");

        // We target the end of UniformGrid
        string search = "</UniformGrid>";
        string cariBtn = @"<Button x:Name=""btnCariTakip"" Content=""CARİ TAKİP"" FontSize=""20"" Foreground=""White"" Background=""#FF252B36"" Height=""82"" Margin=""10,0"" Click=""caritakip_Click"" FontFamily=""Arial Black"" Cursor=""Hand"">
                    <Button.RenderTransform>
                        <ScaleTransform ScaleX=""1"" ScaleY=""1""/>
                    </Button.RenderTransform>
                    <Button.Style>
                        <Style TargetType=""Button"">
                            <Setter Property=""RenderTransformOrigin"" Value=""0.5,0.5""/>
                            <Setter Property=""Template"">
                                <Setter.Value>
                                    <ControlTemplate TargetType=""Button"">
                                        <Border Background=""{TemplateBinding Background}"" CornerRadius=""15"">
                                            <ContentPresenter HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property=""IsMouseOver"" Value=""True"">
                                    <Trigger.EnterActions>
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1.05"" Duration=""0:0:0.2""/>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1.05"" Duration=""0:0:0.2""/>
                                                <ColorAnimation Storyboard.TargetProperty=""(Button.Background).(SolidColorBrush.Color)"" To=""#FF3A445D"" Duration=""0:0:0.2""/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </Trigger.EnterActions>
                                    <Trigger.ExitActions>
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1"" Duration=""0:0:0.2""/>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1"" Duration=""0:0:0.2""/>
                                                <ColorAnimation Storyboard.TargetProperty=""(Button.Background).(SolidColorBrush.Color)"" To=""#FF252B36"" Duration=""0:0:0.2""/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </Trigger.ExitActions>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>";

        string toptanciBtn = @"<Button x:Name=""btnToptanciYonetimi"" Content=""TOPTANCI"" FontSize=""20"" Foreground=""White"" Background=""#FF252B36"" Height=""82"" Margin=""10,0"" Click=""btnToptanciYonetimi_Click"" FontFamily=""Arial Black"" Cursor=""Hand"">
                    <Button.RenderTransform>
                        <ScaleTransform ScaleX=""1"" ScaleY=""1""/>
                    </Button.RenderTransform>
                    <Button.Style>
                        <Style TargetType=""Button"">
                            <Setter Property=""RenderTransformOrigin"" Value=""0.5,0.5""/>
                            <Setter Property=""Template"">
                                <Setter.Value>
                                    <ControlTemplate TargetType=""Button"">
                                        <Border Background=""{TemplateBinding Background}"" CornerRadius=""15"">
                                            <ContentPresenter HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property=""IsMouseOver"" Value=""True"">
                                    <Trigger.EnterActions>
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1.05"" Duration=""0:0:0.2""/>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1.05"" Duration=""0:0:0.2""/>
                                                <ColorAnimation Storyboard.TargetProperty=""(Button.Background).(SolidColorBrush.Color)"" To=""#FF3A445D"" Duration=""0:0:0.2""/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </Trigger.EnterActions>
                                    <Trigger.ExitActions>
                                        <BeginStoryboard>
                                            <Storyboard>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleX)"" To=""1"" Duration=""0:0:0.2""/>
                                                <DoubleAnimation Storyboard.TargetProperty=""(UIElement.RenderTransform).(ScaleTransform.ScaleY)"" To=""1"" Duration=""0:0:0.2""/>
                                                <ColorAnimation Storyboard.TargetProperty=""(Button.Background).(SolidColorBrush.Color)"" To=""#FF252B36"" Duration=""0:0:0.2""/>
                                            </Storyboard>
                                        </BeginStoryboard>
                                    </Trigger.ExitActions>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>";

        // Also replace hardcoded Width="310" so UniformGrid auto scales them
        t = t.Replace("Width=\"310\"", "Margin=\"10,0\"");
        t = t.Replace("FontSize=\"22\"", "FontSize=\"20\""); // decrease font size slightly to fit nicely

        if (!t.Contains("btnCariTakip"))
        {
            t = t.Replace(search, cariBtn + "\r\n" + toptanciBtn + "\r\n" + search);
            File.WriteAllText(p, t);
        }
    }
}
