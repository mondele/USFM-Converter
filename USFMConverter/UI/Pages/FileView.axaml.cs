using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using NPOI.OpenXmlFormats.Vml.Office;
using USFMConverter.Core;
using USFMConverter.Core.Util;
using USFMConverter.Core.ConstantValue;
using USFMConverter.UI.Pages.PartialView;

namespace USFMConverter.UI.Pages
{
    public class FileView : UserControl
    {
        private ProjectReady projectReadySection;
        private ProjectNotReady projectNotReadySection;
        
        private Border dragDropArea;
        private ListBox filesContainer;
        private TextBlock selectedCount;
        private Button browseBtn;
        private Button convertBtn;
        private Button removeFileBtn;
        private Dictionary<FileFormat, FileDialogFilter> fileFilter = new()
        {
            [FileFormat.DOCX] = new FileDialogFilter { 
                Name = "Word Document", 
                Extensions = new() { "docx" } 
            },
            [FileFormat.HTML] = new FileDialogFilter
            {
                Name = "HTML Document",
                Extensions = new() { "html" }
            }
        };

        public event EventHandler<RoutedEventArgs> ConvertStart
        {
            add
            {
                AddHandler(StartConvertEvent, value);
            }
            remove
            {
                RemoveHandler(StartConvertEvent, value);
            }
        }

        public static readonly RoutedEvent<RoutedEventArgs> StartConvertEvent = 
            RoutedEvent.Register<FileView, RoutedEventArgs>(
                nameof(ConvertStart), 
                RoutingStrategies.Bubble
            );

        public FileView()
        {
            InitializeComponent();
        }

        private List<string> Items { get; set; } = new();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            SetLinuxText();

            projectReadySection = this.FindControl<ProjectReady>("ProjectReady");
            projectNotReadySection = this.FindControl<ProjectNotReady>("ProjectNotReady");

            convertBtn = projectReadySection.FindControl<Button>("ConvertBtn");
            convertBtn.AddHandler(Button.ClickEvent, OnConvertClick);
            
            browseBtn = this.FindControl<Button>("BrowseBtn");
            browseBtn.AddHandler(Button.ClickEvent, OnBrowseClick);
            
            removeFileBtn = this.FindControl<Button>("RemoveFileBtn");
            removeFileBtn.AddHandler(Button.ClickEvent, OnRemoveClick);

            filesContainer = this.FindControl<ListBox>("FilesListBox");
            filesContainer.AddHandler(ListBox.SelectionChangedEvent, OnFileSelect);
            filesContainer.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            filesContainer.AddHandler(DragDrop.DropEvent, OnDrop);

            dragDropArea = this.FindControl<Border>("DragDropArea");
            dragDropArea.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            dragDropArea.AddHandler(DragDrop.DropEvent, OnDrop);

            selectedCount = this.FindControl<TextBlock>("SelectedCount");
        }

        private async void OnBrowseClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync((Window)this.VisualRoot);
            if (!string.IsNullOrEmpty(result))
            {
                var dir = new FileInfo(result);
                var filesInDir = FileSystem.GetFilesInDir(
                    dir, CoreConverter.supportedExtensions
                ).Select(f => f.FullName);
                
                var newList = Items.Concat(filesInDir).ToList();
                Items = newList;

                filesContainer.Items = newList; // changes to the UI will bind to DataContext
                UpdateProjectStatus();
            }
        }

        private async void OnConvertClick(object? sender, RoutedEventArgs e)
        {
            var fileFormatName = ((ViewData)DataContext).OutputFileFormat.Tag?.ToString();

            if (string.IsNullOrEmpty(fileFormatName))
            {
                return; // invalid output format
            }

            FileFormat fileFormat = Enum.Parse<FileFormat>(fileFormatName);

            var dialog = new SaveFileDialog();
            dialog.Title = "Save Project As";
            dialog.InitialFileName = "out";
            dialog.Filters.Add(fileFilter[fileFormat]);

            var result = await dialog.ShowAsync((Window)this.VisualRoot);
            if (!string.IsNullOrEmpty(result))
            {
                // propagate event to parent
                ((ViewData)DataContext).OutputFileLocation = result;
                RaiseEvent(new RoutedEventArgs(StartConvertEvent));
            }
        }

        private void OnRemoveClick(object? sender, RoutedEventArgs e)
        {
            var list = filesContainer.Items.Cast<string>().ToList();
            
            foreach(var item in filesContainer.SelectedItems)
            {
                list.Remove(item.ToString());
            }

            Items = list;
            filesContainer.Items = list;

            UpdateProjectStatus();
            UpdateCounter();
        }

        private void OnFileSelect(object? sender, SelectionChangedEventArgs e)
        {
            UpdateCounter();
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var filesToAdd = new List<string>();
                var selectedFiles = e.Data.GetFileNames()
                        .Select(name => new FileInfo(name));

                foreach (var file in selectedFiles)
                {
                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        var filesInDir = FileSystem.GetFilesInDir(
                            file, CoreConverter.supportedExtensions
                        ).Select(f => f.FullName);

                        filesToAdd.AddRange(filesInDir);
                    }
                    else if (CoreConverter.supportedExtensions.Contains(file.Extension))
                    {
                        filesToAdd.Add(file.FullName);
                    }
                }

                var newList = Items.Concat(filesToAdd).ToList();
                Items = newList;

                filesContainer.Items = newList; // changes to the UI will bind to DataContext
                UpdateProjectStatus();
            }
        }

        /// <summary>
        /// Avalonia currently does not support drag and drop.
        /// Please remove this method once the framework implemented it.
        /// <see cref="https://github.com/AvaloniaUI/Avalonia/issues/5273"/>
        /// </summary>
        private void SetLinuxText()
        {
            var platform = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem;
            if (platform == OperatingSystemType.Linux)
            {
                TextBlock dndText = this.FindControl<TextBlock>("DragDropText");
                dndText.Text = "Browse for folder that contains USFM files";
            }
        }

        public void UpdateProjectStatus()
        {
            if (filesContainer.ItemCount > 0)
            {
                projectNotReadySection.IsVisible = false;
                projectReadySection.IsVisible = true;
            }
            else
            {
                projectNotReadySection.IsVisible = true;
                projectReadySection.IsVisible = false;
            }
        }

        public void UpdateCounter()
        {
            selectedCount.Text = filesContainer.SelectedItems.Count.ToString();
        }
    }
}