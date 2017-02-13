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
        private bool canTrackBodies             = false;

        public KinectManager()
        {
            this.kinectSensor = KinectSensor.GetDefault();

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
            // Check for data / bodies in the frame
            if (GetAndRefreshBodyData(e, false)) { return; }
   
            // Have a frame + bodies? Start recording
            SaveCameraFrame(TrackBodies());
        }


        /// <summary>
        /// Save an array of camera frames
        /// </summary>
        /// <param name="body">The body(ies) we are attempting to track</param>
        private void SaveCameraFrame(Body body)
        {
            if (body != null && this.canTrackBodies && body.IsTracked)
            {
                // Tracking skeleton -- save video to file
                CameraIO.SaveFrame();
            }
            else
            {
                Debug.WriteLine("------STOPPED: No longer tracking-------");
            }
        }


        /// <summary>
        /// Track any bodies on screen. If we are able to, add them to the body array.
        /// </summary>
        private Body TrackBodies()
        {
            Body body = null;

            if (this.canTrackBodies)
            {
                // If we are tracking a new body.....
                if (this.bodies[this.bodyIndex].IsTracked)
                {
                    // Add body to the index
                    body = this.bodies[this.bodyIndex];
                }
                else
                {
                    this.canTrackBodies = false;
                }
            }

            // Loop through all of the bodies we are currently tracking
            for (var i = 0; i < this.bodies.Length; ++i)
            {
                // Not tracking any bodies? Keep going.
                if (!this.bodies[i].IsTracked) continue;

                this.bodyIndex   = i;
                this.canTrackBodies = true;
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
                // Not able to track get a frame, which then tracks bodies. Report no data.
                if (bodyFrame == null) return !dataReceived;

                // We can capture a frame. Grab any bodies we find in the frame and add it to the BodyCount for this frame
                if (this.bodies == null)
                {
                    this.bodies = new Body[bodyFrame.BodyCount];
                }
                // Add our bodies array to frame
                bodyFrame.GetAndRefreshBodyData(this.bodies);
            }
            return false;
        }
    }
}

