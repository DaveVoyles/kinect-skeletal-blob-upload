using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    public class CameraIO
    {
        public const string ImageBasePath = "C:\\Images\\";
        private MainWindow _mainWindow;
        private int ImagesPerZip = 200;

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>

        private string VidSegPath = null;

        private int FramesInPath = 0;
        private bool isFirstRound = true;

        public CameraIO(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        private void SaveZipToBlob(string zipPath, string blobName)
        {
            CloudStorageAccount sAccount = CloudStorageAccount.Parse(MainWindow.BlobConnString);
            CloudBlobClient blobClient = sAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("kinectstreams");
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            using (var fileStream = File.OpenRead(zipPath))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }

        public void SaveFrame()
        {
            if (_mainWindow.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new JpegBitmapEncoder();
                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(_mainWindow.colorBitmap));
                DateTime now = System.DateTime.Now;
                string nowPath = now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" +
                                 now.Hour.ToString() + "_" + now.Minute.ToString() + "_" + now.Second.ToString() + "_" + now.Millisecond.ToString();
                if (String.IsNullOrEmpty(VidSegPath) || FramesInPath > ImagesPerZip)
                {
                    if (!isFirstRound)
                    {
                        string zipPath = ImageBasePath + VidSegPath + ".zip";
                        ZipFile.CreateFromDirectory(ImageBasePath + VidSegPath, zipPath);
                        this.SaveZipToBlob(zipPath, VidSegPath);
                    }
                    VidSegPath = nowPath;
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
}