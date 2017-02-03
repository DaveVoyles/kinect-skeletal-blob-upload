# kinect-skeletal-blob-upload

### Author: Dave Voyles
### Site: [DaveVoyles.com](http://www.davevoyles.com)
### Twitter: [@DaveVoyles](http://www.Twitter.com/DaveVoyles)

--------------------------

## Directions

You must have a [Kinect 2 sensor](https://www.amazon.com/Microsoft-Kinect-for-Windows-V2/dp/B00KZIVEXO) attached to a PC to run this.
Simply fork this application and run.

This application works as follows:

1. Track a user's skeleton
2. Save the video as .jpg frames
3. Zip up the frames
4. Upload to Azure blob storage

Stand back from the Kinect camera, and watch as it tracks your skeleton. You'll know it works when the debug message appears at the 
bottom of the screen, notifying you that it is recording frames and then uploading to blob storage. 

### Azure Requirements

You need to replace your *BlobConnString* in MainWindow.xaml.cs with your own 
[Azure Blob Storage Connection String.](https://docs.microsoft.com/en-us/azure/storage/storage-configure-connection-string)
in order to connect to your Blob Storage account, where the zipped up frames will remain. 
