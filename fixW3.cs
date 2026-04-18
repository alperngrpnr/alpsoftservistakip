using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml";
        string t = File.ReadAllText(p);

        string search = "</Button>\r\n            </StackPanel>";

        string btn = @"</Button>

                <Button x:Name=""caritakip"" Content=""CARİ TAKİP"" FontSize=""22"" Foreground=""White"" Background=""#FF252B36"" Width=""310"" Height=""82"" Click=""caritakip_Click"" FontFamily=""Arial Black"" Cursor=""Hand"">
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
                </Button>
            </StackPanel>";

        if (t.Contains(search))
        {
            t = t.Replace(search, btn);
            File.WriteAllText(p, t);
        }
    }
}
