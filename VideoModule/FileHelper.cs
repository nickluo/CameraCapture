using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoModule
{
    internal static class FileHelper
    {
        public static string SavePath { get; }
        static FileHelper()
        {
            SavePath = ConfigurationManager.AppSettings["filepath"];
            if (string.IsNullOrEmpty(SavePath))
                SavePath = string.Empty;
            else
            {
                SavePath = SavePath.Trim();
                if (SavePath.Last() != '\\')
                    SavePath += @"\";
                try
                {
                    if (!Directory.Exists(SavePath))
                        Directory.CreateDirectory(SavePath);
                }
                catch (IOException)
                {
                    SavePath = string.Empty;
                }

            }
        }
    }
}
