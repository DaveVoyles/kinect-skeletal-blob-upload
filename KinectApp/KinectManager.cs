/* Copyright Microsoft Cop. 2016, Dave Voyles
 * www.DaveVoyles.com | Twitter.com/DaveVoyles
 * GitHub Repository w/ Instructions: https://github.com/DaveVoyles/kinect-skeletal-blob-upload
 */
using System.Diagnostics;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class KinectManager
    {
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

        public KinectManager()
        {
            this.kinectSensor    = KinectSensor.GetDefault();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // open the sensor
            this.kinectSensor.Open();

            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
        }


        /// <summary>
        /// Make async, check if tracked, then switch val to true / false
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            if (GetAndRefreshBodyData(e, dataReceived)) return;

            Body body = null;
                 body = TrackBodies(body);

            SaveCameraFrame(body);
        }


        /// <summary>
        /// Save an array of camera frames
        /// </summary>
        /// <param name="body">The body(ies) we are attempting to track</param>
        private void SaveCameraFrame(Body body)
        {
            if (body != null && this.bodyTracked && body.IsTracked)
            {
                // Tracking skeleton -- save video to file
                CameraIO.SaveFrame();
            }
            else
            {
                isTracking = false;
                Debug.WriteLine("------STOPPED: No longer tracking-------");
            }
        }


        private Body TrackBodies(Body body)
        {
            if (this.bodyTracked)
            {
                // If we are tracking a new body.....
                if (this.bodies[this.bodyIndex].IsTracked)
                {
                    // Add body to the index
                    body = this.bodies[this.bodyIndex];
                }
                else
                {
                    // Not tracking anything
                    bodyTracked = false;
                    Debug.Write("Not tracking body");
                }
            }
            // If we're able to track the body, return it.
            if (bodyTracked) return body;

            // Loop through all of the bodies we are currently tracking
            for (var i = 0; i < this.bodies.Length; ++i)
            {
                if (!this.bodies[i].IsTracked) continue;

                this.bodyIndex   = i;
                this.bodyTracked = true;
                break;
            }
            return body;
        }


        /// <summary>
        /// Attempt to track bodies each frame
        /// </summary>
        private bool GetAndRefreshBodyData(BodyFrameArrivedEventArgs e, bool dataReceived)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                // Not able to track? Exit.
                if (bodyFrame == null) return !dataReceived;

                // Trying to track a body, but cannot find any
                if (this.bodies == null)
                {
                    Debug.WriteLine("Bodies are null");
                    this.bodies = new Body[bodyFrame.BodyCount];
                }
                bodyFrame.GetAndRefreshBodyData(this.bodies);
            }
            return false;
        }
    }
}
