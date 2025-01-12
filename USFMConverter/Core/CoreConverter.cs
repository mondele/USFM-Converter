﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using USFMConverter.Core.ConstantValue;
using USFMConverter.Core.Data;
using USFMConverter.Core.Render;
using USFMConverter.Core.Util;
using USFMConverter.UI;

namespace USFMConverter.Core
{
    public class CoreConverter
    {
        public static ICollection<string> supportedExtensions = new List<string> {
            ".usfm", ".txt", ".sfm"
        };

        public async Task ConvertAsync(ViewData viewData, Action<double> progressCallback)
        {
            FileSystem.CheckWritePermission(viewData.OutputFileLocation);

            string fileFormatName = viewData.OutputFileFormat.Tag.ToString();
            var fileFormat = Enum.Parse<FileFormat>(fileFormatName);

            Project project = BuildProject(viewData);
            Renderable renderer;
            switch (fileFormat)
            {
                case FileFormat.DOCX:
                    renderer = new DocxRenderer();
                    break;
                case FileFormat.HTML:
                    renderer = new HTMLRenderer();
                    break;
                default:
                    throw new ArgumentException("Output file format is not supported");
            }

            var usfmDocument = await FileSystem.LoadUSFMsAsync(project.Files, progressCallback);
            renderer.Render(project, usfmDocument);
        }

        private Project BuildProject(ViewData viewData)
        {
            var textSizeName = viewData.TextSize.Tag?.ToString();
            var textSize = (string.IsNullOrEmpty(textSizeName))
                ? TextSize.MEDIUM
                : Enum.Parse<TextSize>(textSizeName);

            var lineSpacingName = viewData.LineSpacing.Tag?.ToString();
            var lineSpacing = (string.IsNullOrEmpty(lineSpacingName))
                ? LineSpacing.SINGLE
                : Enum.Parse<LineSpacing>(lineSpacingName);

            var textAlignment = (viewData.Justified)
                ? TextAlignment.JUSTIFIED
                : TextAlignment.LEFT;

            var project = new Project
            {
                FormatOptions = new RenderFormat
                {
                    TextSize = textSize,
                    LineSpacing = lineSpacing,
                    TextAlign = textAlignment,
                    LeftToRight = viewData.LeftToRight,
                    ColumnCount = viewData.ColumnCount,
                    ChapterBreak = viewData.ChapterBreak,
                    VerseBreak = viewData.VerseBreak,
                    NoteTaking = viewData.NoteTaking,
                    TableOfContents = viewData.TableOfContents,
                },
                OutputFile = new FileInfo(viewData.OutputFileLocation),
            };

            project.Files.AddRange(viewData.Files);

            return project;
        }
    }
}
