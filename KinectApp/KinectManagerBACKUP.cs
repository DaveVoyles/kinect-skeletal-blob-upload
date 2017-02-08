/* Copyright Microsoft Cop. 2016, Dave Voyles
 * www.DaveVoyles.com | Twitter.com/DaveVoyles
 * GitHub Repository w/ Instructions: https://github.com/DaveVoyles/kinect-skeletal-blob-upload
 */
using System.Diagnostics;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class KinectManagerBACKUP
    {
        // Active Kinect sensor
        private KinectSensor kinectSensor = null;

        // Reader for _body frames
        private BodyFrameReader bodyFrameReader = null;

        Body _body = null;

        // Array for the bodies
        private Body[] bodies = null;

        // index for the currently tracked _body
        private int bodyIndex;

        public enum KinectState
        {
            notTracked,
            bodyTracked,
            isTracked,
            bodyTrackedAndIsTracking,
        }

        private KinectState kinectState { get; set; }


        // flag to asses if a _body is currently tracked
        private bool bodyTracked = false;
        public bool isTracking = true;

        public KinectManagerBACKUP()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the _body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // open the sensor
            this.kinectSensor.Open();

            // Continue to check if we can record
            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
        }


        /// <summary>
        /// Make async, check if tracked, then switch val to true / false
        /// Handles the _body frame data arriving from the sensor
        /// </summary>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            CheckBodies(sender, e);

            //bool dataReceived = false;

            //if (GetAndRefreshBodyData(e, dataReceived)) return;

            //Body _body = null;
            //     _body = TrackBodies(_body);

            //SaveCameraFrame(_body);
            //SaveCameraFrame();
        }


        private void CheckBodies(object sender, BodyFrameArrivedEventArgs e)
        {

            switch (kinectState)
            {
                case KinectState.bodyTrackedAndIsTracking:
                    SaveCameraFrame(_body);
                    break;
                case KinectState.notTracked:
                    //GetAndRefreshBodyData(e, false);
                    _body = TrackBodies(_body);
                    break;
                default:
                    // Set default state
                    kinectState = KinectState.notTracked;
                    break;
            }
        }

        /// <summary>
        /// Save an array of camera frames
        /// </summary>
        /// <param name="body">The _body(ies) we are attempting to track</param>
        private void SaveCameraFrame(Body body)
        {
            //if (_body != null && this.bodyTracked && _body.IsTracked)
            if (body != null)
            {
                // Tracking skeleton -- save video to file
                CameraIO.SaveFrame();
                Debug.WriteLine("Saving Frame");
            }
            // TODO: Can probably get rid of this
            else
            {
                //isTracking = false;
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

                this.bodyIndex = i;
                this.bodyTracked = true;
                break;
            }
            return body;
        }


        /// <summary>
        /// Attempt to get a frame & track bodies each frame
        /// </summary>
        private bool GetAndRefreshBodyData(BodyFrameArrivedEventArgs e, bool dataReceived)
        {
            // Represents a frame that contains all the computed real-time tracking information 
            // about people that are in view of the sensor.
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                // Not able to get a frame?
                //if (bodyFrame == null) return !dataReceived;
                //TODO: Can probably get rid of this
                if (bodyFrame == null)
                {
                    kinectState = KinectState.notTracked;
                    Debug.WriteLine("Cannot get Frame Reference");
                }

                // Able to get a frame, but bodies is currently null
                if (bodyFrame != null && this.bodies == null)
                {
                    this.bodies = new Body[bodyFrame.BodyCount];
                    // TODO: Determine the correct Kinect State 
                    Debug.WriteLine("Current _body count: " + bodyFrame.BodyCount);
                }
                // Return current status of bodies to Kinect 
                bodyFrame?.GetAndRefreshBodyData(this.bodies);
                kinectState = KinectState.bodyTrackedAndIsTracking;
            }
            return false;
        }


    }
}
