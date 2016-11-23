using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.Threading;
using System.Timers;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class KinectManager
    {
        // skeletal tracking --------------------------------------

        // Active Kinect sensor
        private KinectSensor kinectSensor       = null;

        // Reader for body frames
        private BodyFrameReader bodyFrameReader = null;

        // Array for the bodies
        private Body[] bodies                   = null;

        // index for the currently tracked body
        private int bodyIndex;

        // flag to asses if a body is currently tracked
        private bool bodyTracked                = false;

        public bool isTracking                  = true;
        private bool bStartTimer                = false;

        private MainWindow mainWindow;

        public Thread oThread;

        public KinectManager()
        {

            this.kinectSensor    = KinectSensor.GetDefault();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // open the sensor
            this.kinectSensor.Open();

            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;

            Debug.WriteLine("KinectManager Init");
        }


        // Make async, check if tracked, then switch val to true / false
        // Handles the body frame data arriving from the sensor
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        Debug.WriteLine("Bodies are null");
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived) return;

            Body body = null;
            if (this.bodyTracked)
            {
                //Debug.Write("Tracking body");
                if (this.bodies[this.bodyIndex].IsTracked)
                {
                    body = this.bodies[this.bodyIndex];
                }
                else
                {
                    bodyTracked = false;
                    Debug.Write("Not tracking body");
                }
            }
            if (!bodyTracked)
            {
                for (var i = 0; i < this.bodies.Length; ++i)
                {
                    if (!this.bodies[i].IsTracked) continue;

                    this.bodyIndex   = i;
                    this.bodyTracked = true;
                    break;
                }
            }

            if (body != null && this.bodyTracked && body.IsTracked)
            {
                // Tracking skeleton -- save video to file
                CameraIO.SaveFrame();
            }
            else
            {
                isTracking = false;
                Debug.WriteLine("------STOPPED-------");
            }
        }
    
        
    }
}
