﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USFMConverter.Core.ConstantValue;

namespace USFMConverter.UI
{
    public class ViewData
    {
        public List<string> Files { get; set; } = new();

        public ComboBoxItem OutputFileFormat { get; set; }

        public ComboBoxItem TextSize { get; set; }

        public ComboBoxItem LineSpacing { get; set; }

        public int ColumnCount { get; set; } = 1;

        public bool Justified { get; set; } = false;
        public bool LeftToRight { get; set; } = true;
        public bool ChapterBreak { get; set; } = false;
        public bool VerseBreak { get; set; } = false;
        public bool NoteTaking { get; set; } = false;
        public bool TableOfContents { get; set; } = false;
    }
}