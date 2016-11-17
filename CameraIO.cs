using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
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


        /// <summary>
        /// Gets reference to blockblob.
        /// Account -> Client -> Container -> bloclblob
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="blobName">Name of the file to be saved</param>
        private static void SaveZipToBlob(string zipPath, string blobName)
        {
            var sAccount      = CloudStorageAccount.Parse(MainWindow.BlobConnString);
            var blobClient    = sAccount.CreateCloudBlobClient();
            var container     = blobClient.GetContainerReference("kinectstreams");
                container.CreateIfNotExists();

            var blockBlob     = container.GetBlockBlobReference(blobName);

            using (var fileStream = File.OpenRead(zipPath))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }

        public static void SaveFrame()
        {
            if (_mainWindow.colorBitmap == null) return;

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new JpegBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(_mainWindow.colorBitmap));

            DateTime now   = System.DateTime.Now;
            string nowPath = now.Month.ToString() + "_" + now.Day.ToString()    + "_" + now.Year.ToString()   + "_" +
                             now.Hour.ToString()  + "_" + now.Minute.ToString() + "_" + now.Second.ToString() + "_" + now.Millisecond.ToString();

            if (String.IsNullOrEmpty(VidSegPath) || FramesInPath > ImagesPerZip)
            {
                if (!isFirstRound)
                {
                    string zipPath = ImageBasePath + VidSegPath + ".zip";
                    ZipFile.CreateFromDirectory(ImageBasePath + VidSegPath, zipPath);
                    SaveZipToBlob(zipPath, VidSegPath);
                }
                VidSegPath   = nowPath;
                FramesInPath = 0;
                Directory.CreateDirectory(ImageBasePath + VidSegPath + "\\");
                isFirstRound = false;
            }

            string path = ImageBasePath + VidSegPath + "\\" + nowPath + ".jpg";

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
    }
}