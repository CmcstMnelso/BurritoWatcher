using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BurritoWatcher
{
    public class Setup
    {
        //directory to save downloaded files
        public static string ADBDirectory = "ADB";
        //google link for requirements to download
        private const string ADBDownload = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
        //temporary file name for downloaded file
        private const string downloadedFileName = "ADBDownload.zip";
        //sentinel value for main thread to wait for download completion
        private bool _downloadCompleted = false;

        //entrypoint to check for files
        public Setup()
        {
            if (!Directory.Exists(ADBDirectory))
            {
                Directory.CreateDirectory(ADBDirectory);
                //download required files
                WebClient wc = new WebClient();
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                Console.WriteLine("Downloading required files...");

                wc.DownloadFileAsync(new Uri(ADBDownload), downloadedFileName);
                //wait on async completion
                while (!_downloadCompleted) Thread.Sleep(100);
            }
            //check to see if files are in expected locations
            else if (!File.Exists(ADBDirectory + "\\platform-tools\\adb.exe"))
            {
                throw new FileNotFoundException("Files may be misconfigured. Please delete your ADB directory.");
            }
        }

        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("\nDownload completed!");
            //handle zip extraction
            using (var target = File.OpenRead(downloadedFileName))
            {
                using (var archive = new ZipArchive(target, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(ADBDirectory, true);
                }
            }
            //delete temporary file
            File.Delete(downloadedFileName);
            //copy batch files to proper locations
            File.Copy("manageADB.bat", AppDomain.CurrentDomain.BaseDirectory + "\\" + ADBDirectory + "\\platform-tools\\" + "manageADB.bat");
            File.Copy("sendSMS.bat", AppDomain.CurrentDomain.BaseDirectory + "\\" + ADBDirectory + "\\platform-tools\\" + "sendSMS.bat");
            //trigger main thread to continue
            _downloadCompleted = true;
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\rDownloading: ");
            DrawProgressBar(e.ProgressPercentage, 50);
        }

        //attempts to draw progressbar to console (WIP)
        private static void DrawProgressBar(int progress, int progressBarWidth)
        {
            Console.CursorVisible = false;
            int width = progressBarWidth;
            Console.Write("[");
            int progressOnBar = (int)(progress / (double)100 * width);
            for (int i = 0; i < width; i++)
            {
                if (i < progressOnBar) Console.Write("=");
                else if (i == progressOnBar) Console.Write(">");
                else Console.Write(" ");
            }
            Console.Write($"] {progress}%");
            Console.CursorVisible = true;
        }
    }
}
