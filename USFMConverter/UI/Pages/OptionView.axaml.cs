using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using USFMConverter.Core.Data.Serializer;
using USFMConverter.Core.Util;

namespace USFMConverter.UI.Pages
{
    public class OptionView : UserControl
    {
        private Border blurredArea;
        private UserControl optionView;
        private ComboBox outputFormatCb;

        public OptionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            blurredArea = this.FindControl<Border>("BlurredArea");
            blurredArea.AddHandler(PointerPressedEvent, OnCloseClick);

            outputFormatCb = this.FindControl<ComboBox>("OutputFormatSelector");
            int lastUsedFormatIndex = SettingManager.LoadMostRecentFormat()?.FormatIndex ?? 0;
            outputFormatCb.SelectedIndex = lastUsedFormatIndex;
            ShowFormatView();
            outputFormatCb.AddHandler(ComboBox.SelectionChangedEvent, OnOuputFormatSelect);

            
            optionView = this.FindControl<UserControl>("OptionView");
        }

        private void OnOuputFormatSelect(object? sender, SelectionChangedEventArgs e)
        {
            string selectedFormat = ((ComboBoxItem) outputFormatCb.SelectedItem).Tag.ToString();
            ShowFormatView();
            SaveOptions();
            LoadOptions(selectedFormat);
        }

        private void ShowFormatView()
        {
            string selectedFormat = ((ComboBoxItem) outputFormatCb.SelectedItem).Tag.ToString();

            foreach (ComboBoxItem comboBoxItem in outputFormatCb.Items)
            {
                var formatName = comboBoxItem.Tag.ToString();
                this.FindControl<UserControl>(formatName).IsVisible = (formatName == selectedFormat);
            }
        }
        

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            optionView.IsVisible = false;
            SaveOptions();
        }

        private void SaveOptions()
        {
            // Save config files when option view is closed
            var dataContext = (ViewData) DataContext;
            SettingManager.SaveSetting(dataContext);
        }

        private void LoadOptions(string OutputFileFormat)
        {
            var dataContext = (ViewData) DataContext;
            Setting? setting = SettingManager.LoadSetting(OutputFileFormat);

            // Don't load config if the config file does not exist
            if (setting != null)
            {
                ((Window) VisualRoot).DataContext = new ViewData
                {
                    Files = dataContext.Files,
                    SelectedTextSizeIndex = setting.TextSizeIndex,
                    SelectedLineSpacingIndex = setting.LineSpacingIndex,
                    ColumnCount = setting.ColumnCount,
                    Justified = setting.Justified,
                    LeftToRight = setting.LeftToRight,
                    ChapterBreak = setting.ChapterBreak,
                    VerseBreak = setting.VerseBreak,
                    NoteTaking = setting.NoteTaking,
                    TableOfContents = setting.TableOfContents
                };
            }
        }

        //private int GetLastUsedFormatIndex(int defaultIndex)
        //{
        //    string lastUsedFormat = SettingManager.LoadMostRecentFormat();
        //    int lastUsedFormatIndex = defaultIndex; // default index is 0

        //    if (Enum.TryParse(lastUsedFormat, out FileFormat fileFormat))
        //    {
        //        lastUsedFormatIndex = (int) fileFormat;
        //    }

        //    return lastUsedFormatIndex;
        //}
    }
}