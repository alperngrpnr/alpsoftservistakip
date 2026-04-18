using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Window3.xaml";
        string t = File.ReadAllText(p);

        int gridStart = t.IndexOf("<!-- Custom Title Bar -->");
        int gridEnd = t.IndexOf("</Grid>", gridStart) + "</Grid>".Length;

        if (gridStart == -1 || gridEnd == -1) return;

        string oldGrid = t.Substring(gridStart, gridEnd - gridStart);

        string newTitleBar = @"<!-- Custom Title Bar -->
            <Grid Height=""35"" VerticalAlignment=""Top"" Panel.ZIndex=""999"" Background=""#FF181B21"" MouseLeftButtonDown=""Window_MouseDown"">
                <!-- App Logo & Title -->
                <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"" Margin=""15,0,0,0"">
                    <!-- Elegant dot for logo -->
                    <Ellipse Width=""10"" Height=""10"" Fill=""#FFF5C721"" Margin=""0,0,10,0""/>
                    <TextBlock Text=""AlpSoft Servis Takip v1.6.2"" Foreground=""#FFCCCCCC"" FontWeight=""SemiBold"" FontFamily=""Segoe UI"" FontSize=""13"" VerticalAlignment=""Center""/>
                </StackPanel>

                <!-- Window Control Buttons Area -->
                <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                    <!-- Minimize Button -->
                    <Button Width=""46"" Background=""Transparent"" BorderThickness=""0"" Click=""btnMinimize_Click"" Foreground=""#FFCCCCCC"">
                        <Button.Template>
                            <ControlTemplate TargetType=""Button"">
                                <Border x:Name=""bg"" Background=""{TemplateBinding Background}"">
                                    <Path Data=""M 0,5 H 10"" Stroke=""{TemplateBinding Foreground}"" StrokeThickness=""1"" SnapsToDevicePixels=""True"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <ControlTemplate.Triggers>
                                    <Trigger Property=""IsMouseOver"" Value=""True"">
                                        <Setter TargetName=""bg"" Property=""Background"" Value=""#22FFFFFF""/>
                                        <Setter Property=""Foreground"" Value=""White""/>
                                    </Trigger>
                                </ControlTemplate.Triggers>
                            </ControlTemplate>
                        </Button.Template>
                    </Button>

                    <!-- Maximize/Restore Button -->
                    <Button Width=""46"" Background=""Transparent"" BorderThickness=""0"" Click=""btnMaximize_Click"" Foreground=""#FFCCCCCC"">
                        <Button.Template>
                            <ControlTemplate TargetType=""Button"">
                                <Border x:Name=""bg"" Background=""{TemplateBinding Background}"">
                                    <Path Data=""M 1,1 H 9 V 9 H 1 Z"" Stroke=""{TemplateBinding Foreground}"" StrokeThickness=""1"" Fill=""Transparent"" SnapsToDevicePixels=""True"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <ControlTemplate.Triggers>
                                    <Trigger Property=""IsMouseOver"" Value=""True"">
                                        <Setter TargetName=""bg"" Property=""Background"" Value=""#22FFFFFF""/>
                                        <Setter Property=""Foreground"" Value=""White""/>
                                    </Trigger>
                                </ControlTemplate.Triggers>
                            </ControlTemplate>
                        </Button.Template>
                    </Button>

                    <!-- Close Button -->
                    <Button Width=""46"" Background=""Transparent"" BorderThickness=""0"" Click=""btnClose_Click"" Foreground=""#FFCCCCCC"">
                        <Button.Template>
                            <ControlTemplate TargetType=""Button"">
                                <Border x:Name=""bg"" Background=""{TemplateBinding Background}"">
                                    <Path Data=""M 0,0 L 10,10 M 0,10 L 10,0"" Stroke=""{TemplateBinding Foreground}"" StrokeThickness=""1"" SnapsToDevicePixels=""True"" HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
                                </Border>
                                <ControlTemplate.Triggers>
                                    <Trigger Property=""IsMouseOver"" Value=""True"">
                                        <Setter TargetName=""bg"" Property=""Background"" Value=""#FFE81123""/>
                                        <Setter Property=""Foreground"" Value=""White""/>
                                    </Trigger>
                                </ControlTemplate.Triggers>
                            </ControlTemplate>
                        </Button.Template>
                    </Button>
                </StackPanel>
            </Grid>";

        t = t.Replace(oldGrid, newTitleBar);
        File.WriteAllText(p, t);
    }
}
