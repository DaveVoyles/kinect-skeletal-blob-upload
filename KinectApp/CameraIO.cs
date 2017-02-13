/* Copyright Microsoft Cop. 2016, Dave Voyles
 * www.DaveVoyles.com | Twitter.com/DaveVoyles
 * GitHub Repository w/ Instructions: https://github.com/DaveVoyles/kinect-skeletal-blob-upload
 */
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using Microsoft.Samples.Kinect.ColorBasics.Properties;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    public class CameraIO
    {
        private static MainWindow _mainWindow;

        /// <summary>
        /// Directory where images will be stored
        /// </summary>
        public const string ImageBasePath = "C:\\Images\\";

        /// <summary>
        /// TODO: Replace with your connection string
        /// </summary>
        public const string BlobConnString = @"DefaultEndpointsProtocol=https;AccountName=davevblobtest;AccountKey=xTxO/Yd9gErWkfkd0Gaa9PlPb9if2KWD4iA3iRjcdpio3RAhwctMgS6hzcCECmivlBwucKCQSqaqRwyfq3IkBQ==";


        /// <summary>
        /// This builds a connection string
        /// </summary>
        /// <param name="accountName">EX: "davevblobtest"</param>
        /// <param name="accountKey">EX: "xTxO/Yd9gErWkfkd0Gaa9PlPb9if2KWD4iA3iRjcdpio3RAhwctMgS6hzcCECmivlBwucKCQSqaqRwyfq3IkBQ==" </param>
        /// <returns></returns>
        private static string GetBlobConnString(string accountName, string accountKey)
        {
            var accountString = @"DefaultEndpointsProtocol=https;AccountName=";
                accountString += accountName + ";";

            var accountKeyString = @"AccountKey=";
                accountKeyString += accountKey;

            var connString = accountString + accountKeyString;

            Debug.WriteLine(connString);
            return connString;
        }

        /// <summary>
        /// Name of container where blobs willl be stored
        /// </summary>
        private static string containerName = "kinectstreams";

        private static int ImagesPerZip   = 200;
        private static string VidSegPath  = null;
        private static int FramesInPath   = 0;
        private static bool isFirstRound  = true;

        public CameraIO(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Connect to blob storage account and create container if it does not exist. 
        /// Afterwards, upload it to Azure blob storage
        /// </summary>
        private static async void SaveZipToBlobAsync(string zipPath, string blobName)
        {
            var sAccount      = CloudStorageAccount.Parse(BlobConnString);
            //var sAccount = CloudStorageAccount.Parse(
            //    GetBlobConnString("davevblobtest",
            //                      "xTxO/Yd9gErWkfkd0Gaa9PlPb9if2KWD4iA3iRjcdpio3RAhwctMgS6hzcCECmivlBwucKCQSqaqRwyfq3IkBQ=="));
                            
            var blobClient    = sAccount.CreateCloudBlobClient();
            var container     = blobClient.GetContainerReference(containerName);
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

            var path = GenerateFileAndUpload();

            // write the new file to disk
            try
            {
                // FileStream is IDisposable
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                    FramesInPath += 1;
                }

                _mainWindow.StatusText = String.Format(Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                _mainWindow.StatusText = String.Format(Resources.FailedScreenshotStatusTextFormat, path);
            }
        }


        /// <summary>
        /// Generate unique path / name each time we save the kinect zip files. Makes it easy to sort / sift through later
        /// </summary>
        /// <returns>A unique name/path for storage</returns>
        private static string GenerateFileAndUpload()
        {
            // Generating unique name
            DateTime now   = DateTime.Now;
            string nowPath = now.Month.ToString() + "_" + now.Day.ToString()    + "_" + now.Year.ToString()   + "_" +
                             now.Hour.ToString()  + "_" + now.Minute.ToString() + "_" + now.Second.ToString() + "_" +
                             now.Millisecond.ToString();

            // Have frames (images) and we path: Zip it up and store it in the cloud
            if (String.IsNullOrEmpty(VidSegPath) || FramesInPath > ImagesPerZip)
            {
                if (!isFirstRound)
                {
                    string zipPath = ImageBasePath + VidSegPath + ".zip";
                    ZipFile.CreateFromDirectory(ImageBasePath + VidSegPath, zipPath);
                    SaveZipToBlobAsync(zipPath, VidSegPath); // Slowest point in app, as it is uploading to Azure 
                }  
                VidSegPath   = nowPath;
                FramesInPath = 0;
                // Create local directory for our images
                Directory.CreateDirectory(ImageBasePath + VidSegPath + "\\");
                isFirstRound = false;
            }
            string path = ImageBasePath + VidSegPath + "\\" + nowPath + ".jpg";
            return path;
        }
    }
}