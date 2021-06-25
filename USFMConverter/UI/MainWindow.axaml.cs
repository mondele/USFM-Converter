using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;


using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using USFMConverter.UI.Pages;

namespace USFMConverter
{
    public partial class MainWindow : Window
    {
        private ProjectDetailScreen projectDetailScreen;
        
        Dictionary<string, IControl> Screens = new Dictionary<string, IControl>();

        public MainWindow()
        {
            InitializeComponent();
            SetCurrentScreen(nameof(ProjectDetailScreen));
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void SetCurrentScreen(string screen)
        {
            foreach (var i in this.Screens)
            {
                i.Value.IsVisible = i.Key == screen;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.projectDetailScreen = this.FindControl<ProjectDetailScreen>("ProjectDetailScreen");
            this.Screens.Add(nameof(ProjectDetailScreen), this.projectDetailScreen);
        }
    }
}