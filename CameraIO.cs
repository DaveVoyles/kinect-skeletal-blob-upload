using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.WindowsAzure.Storage;


namespace Microsoft.Samples.Kinect.ColorBasics
{
    public class CameraIO
    {
        private static MainWindow _mainWindow;

        public const string ImageBasePath = "C:\\Images\\";
        private static int ImagesPerZip   = 200;
        private static string VidSegPath  = null;
        private static int FramesInPath   = 0;
        private static bool isFirstRound  = true;

        public CameraIO(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        private static async void SaveZipToBlobAsync(string zipPath, string blobName)
        {
            var sAccount      = CloudStorageAccount.Parse(MainWindow.BlobConnString);
            var blobClient    = sAccount.CreateCloudBlobClient();
            var container     = blobClient.GetContainerReference("kinectstreams");
                container.CreateIfNotExists();

            var blockBlob     = container.GetBlockBlobReference(blobName);

            using (var fileStream = File.OpenRead(zipPath))
            {
               await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }


        public static void SaveFrame()
        {
            if (_mainWindow.colorBitmap == null) return;

            // create a png bitmap encoder which knows how to save a .png file
            // create frame from the writable bitmap and add to encoder
            BitmapEncoder encoder = new JpegBitmapEncoder();
                          encoder.Frames.Add(BitmapFrame.Create(_mainWindow.colorBitmap));

            var path = GetnerateFile();

            // write the new file to disk
            try
            {
                // FileStream is IDisposable
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                    FramesInPath += 1;
                }

                _mainWindow.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                _mainWindow.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }
        }


        private static string GetnerateFile()
        {
            DateTime now = System.DateTime.Now;
            string nowPath = now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" +
                             now.Hour.ToString() + "_" + now.Minute.ToString() + "_" + now.Second.ToString() + "_" +
                             now.Millisecond.ToString();

            if (String.IsNullOrEmpty(VidSegPath) || FramesInPath > ImagesPerZip)
            {
                if (!isFirstRound)
                {
                    string zipPath = ImageBasePath + VidSegPath + ".zip";
                    ZipFile.CreateFromDirectory(ImageBasePath + VidSegPath, zipPath);
                    SaveZipToBlobAsync(zipPath, VidSegPath); // Slowest point in app
                }  
                VidSegPath   = nowPath;
                FramesInPath = 0;
                Directory.CreateDirectory(ImageBasePath + VidSegPath + "\\");
                isFirstRound = false;
            }
            string path = ImageBasePath + VidSegPath + "\\" + nowPath + ".jpg";
            return path;
        }
    }
}