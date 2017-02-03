namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Threading;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const string BlobConnString = @"DefaultEndpointsProtocol=https;AccountName=medtesting;AccountKey=uIVqF3BEvIrftCXB7l0f5AEwAjVUy0s2Ab3mt7nyrufbuTKtgB0YfaWOf1gMowvbGqmRxSCpw6+rV6vtTTqTjg==";

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor        = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        public ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        public WriteableBitmap colorBitmap       = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        public string statusText                 = null;
 

        private KinectManager kinectManager;
        private readonly CameraIO _cameraIo;

        public Thread MainWindowThread;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor                      = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.colorFrameReader                  = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived    += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap                       = new WriteableBitmap(colorFrameDescription.Width, 
                                                                         colorFrameDescription.Height, 
                                                                         96.0, 96.0, 
                                                                         PixelFormats.Bgr32,
                                                                         null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            kinectManager = new KinectManager();
            CameraIo      = new CameraIO(this);
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource => this.colorBitmap;

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText == value) return;
                this.statusText = value;

                // notify any bound elements that the text has changed
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
            }
        }

        public CameraIO CameraIo { get; }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor == null) return;
            this.kinectSensor.Close();
            this.kinectSensor = null;
        }

  


        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null) return;
                FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {
                    this.colorBitmap.Lock();

                    // verify data and write the new color frame data to the display bitmap
                    if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                    {
                        colorFrame.CopyConvertedFrameDataToIntPtr(
                            this.colorBitmap.BackBuffer,
                            (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                            ColorImageFormat.Bgra);

                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                    }

                    this.colorBitmap.Unlock();
                }
                // TODO: Have KinectManager call this
                // CameraIo.SaveFrame();
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
