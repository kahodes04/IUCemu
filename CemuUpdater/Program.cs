using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CemuUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Temp";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else
            {
                Console.WriteLine("Temp folder already exists, clearing folder.");
                clearFolder("Temp");
            }
            string cemufolder = Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            Console.WriteLine("Downloading Cemu.");
            downloadCemu();
            Console.WriteLine("Finished downloading Cemu.");
            Console.WriteLine("Downloading Cemu Hook.");
            downloadHook();
            Console.WriteLine("Finished downloading Cemu Hook.");
            Console.WriteLine("Unzipping Cemu.");
            extractZipFile(@"Temp\\cemudownload.zip", "password", @"Temp");
            string[] filePaths = Directory.GetDirectories(path);
            string executablelocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + filePaths[0] + "\\Cemu.exe";
            string moveexecutableto = cemufolder + "\\Cemu.exe";
            Console.WriteLine("Moving Cemu.exe.");
            File.Copy(executablelocation, moveexecutableto, true);
            Console.WriteLine("Unzipping Cemu Hook.");
            extractZipFile(@"Temp\\hookdownload.zip", "password", cemufolder);
            Console.WriteLine("Clearing Temp folder.");
            clearFolder("Temp");
            Console.WriteLine("Done. Press any key to continue.");
            Console.ReadKey();
        }

        private static void downloadCemu()
        {
            HttpClient httpClient = new HttpClient();
            string html = "";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://cemu.info/#download").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    html = responseContent.ReadAsStringAsync().Result;
                }
            }

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            string downloadlink = htmlDocument.DocumentNode.SelectSingleNode("//a[@name='download']").GetAttributeValue("href","unknown");
            WebClient webclient = new WebClient();
            webclient.DownloadFile(new Uri(downloadlink), @"Temp\\cemudownload.zip");
        }

        private static void downloadHook()
        {
            HttpClient httpCliento = new HttpClient();
            string html = "";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync("https://cemuhook.sshnuke.net/#Downloads").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    html = responseContent.ReadAsStringAsync().Result;
                }
            }

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var downloadlink = htmlDocument.DocumentNode.SelectNodes(".//a")[5].GetAttributeValue("href", "unknown");
            WebClient webclient = new WebClient();
            webclient.DownloadFile(new Uri(downloadlink), @"Temp\\hookdownload.zip");
        }

        private static void clearFolder(string _path)
        {
            DirectoryInfo dir = new DirectoryInfo(_path);

            foreach (FileInfo fi in dir.GetFiles())
                fi.Delete();

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }
        public static void extractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);

                if (!String.IsNullOrEmpty(password))
                    zf.Password = password;

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                        continue;

                    String entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];
                    Stream zipStream = zf.GetInputStream(zipEntry);
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
        }
    }
}
