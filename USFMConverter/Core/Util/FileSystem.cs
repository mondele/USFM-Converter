﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using USFMConverter.UI;
using USFMToolsSharp;
using USFMToolsSharp.Models.Markers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using USFMConverter.Core.Data;

namespace USFMConverter.Core.Util
{
    public static class FileSystem
    {
        private static string appDir;

        static FileSystem()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appDir = Path.Combine(localAppData, Assembly.GetExecutingAssembly().GetName().Name);
            Directory.CreateDirectory(appDir); // Create directory if doesn't exist. Ignore if it exists.
        }
        
        public static ICollection<FileInfo> GetFilesInDir(FileInfo dir, IEnumerable<string> extensions)
        {
            List<FileInfo> files = new();
            var dirInfo = new DirectoryInfo(dir.FullName);
            var allFiles = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                if (extensions.Contains(file.Extension))
                {
                    files.Add(file);
                }
            }

            return files;
        }

        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        public static bool CheckWritePermission(string path)
        {
            File.OpenWrite(path).Close();
            return true;
        }

        public static void OpenFileLocation(string path)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException(
                    "Could not find the specified path: " + path
                    );
            }

            var dir = file.DirectoryName;
            dir = $"\"{dir}\""; // preserve spaces with wrapping double quotes

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", @"/select," + dir);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", "-R " + dir);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var processInfo = new ProcessStartInfo("xdg-open", dir);
                var process = new Process { StartInfo = processInfo };
                process.Start();
            }
        }

        public static void OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Could not find the specified file at: " + path
                    );
            }

            path = $"\"{path}\""; // preserve spaces with wrapping double quotes

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                });
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", path);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var processInfo = new ProcessStartInfo("xdg-open", path);
                var process = new Process { StartInfo = processInfo };
                process.Start();
            }
        }

        /// <summary>
        /// Parses the given text files into one USFM Document asynchronously.
        /// </summary>
        /// <param name="files">Text files with USFM format.</param>
        /// <param name="progressCallback">Call back for progress bar update.</param>
        /// <returns>A USFM Document</returns>
        public static async Task<USFMDocument> LoadUSFMsAsync(
            IEnumerable<string> files,
            Action<double> progressCallback
        )
        {
            var usfmDoc = new USFMDocument();
            List<string> fileList = files.ToList();

            var parser = new USFMParser(new List<string> { "s5" });
            int totalFiles = fileList.Count;

            for (int i = 0; i < totalFiles; i++)
            {
                await Task.Run(() => {
                    var text = File.ReadAllText(fileList[i]);
                    usfmDoc.Insert(parser.ParseFromString(text));
                });

                // update progress bar
                var percent = (double)i / totalFiles * 100;
                progressCallback(percent);
            }

            return usfmDoc;
        }

        public static Setting? LoadOptionConfig(string OutputFileFormat)
        {
            string path = Path.Combine(appDir, $"appsettings_{OutputFileFormat}.json");
            return File.Exists(path) ? JsonConvert.DeserializeObject<Setting>(File.ReadAllText(path)) : null;
        }

        public static void SaveOptionConfig(ViewData? dataContext)
        {
            Setting setting = new (dataContext);
            string path = Path.Combine(appDir, $"appsettings_{dataContext?.OutputFileFormat.Tag}.json");

            // If file doesn't exist, create the file
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}");
            }
            
            File.WriteAllText(path, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        public static string LoadLastUsedFormat()
        {
            string path = Path.Combine(appDir, "appsettings_format.json");
            string lastUsedFormat = "";

            if (!File.Exists(path))
            {
                string content = "{\"LastUsedFormat\": \"\"}";
                File.WriteAllText(path, content);
            }
            else
            {
                string jsonFile = File.ReadAllText(path);
                JObject jsonObj = JObject.Parse(jsonFile);

                lastUsedFormat = (string) jsonObj["LastUsedFormat"];
            }

            return lastUsedFormat;
        }

        public static void SaveLastUsedFormat(ViewData? dataContext)
        {
            string path = Path.Combine(appDir, "appsettings_format.json");
            string lastUsedFormat = dataContext.OutputFileFormat.Tag.ToString();
            
            string jsonFile = File.ReadAllText(path);
            JObject jsonObj = JObject.Parse(jsonFile);

            jsonObj["LastUsedFormat"] = lastUsedFormat;
            
            File.WriteAllText(path, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
        }
    }
}
