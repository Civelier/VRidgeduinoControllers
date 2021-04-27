using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K = Microsoft.Kinect;
using Accord.Math;
using VRidgeduinoControllers.MathUtilities;
using VRidgeduinoControllers.Remotes;
using System.Threading;

namespace VRidgeduinoControllers.Services
{
    public class KinectService : IDisposable
    {
        K.KinectSensor _sensor;
        Thread _service;
        public event EventHandler<PositionFrame> PositionFrameReady;

        public KinectService()
        {
            _service = new Thread(Routine);
            _service.Start();
        }


        public void Routine()
        {
            K.KinectSensor sensor = null;
            try
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Waiting for kinect sensor");
                        while (K.KinectSensor.KinectSensors.Count == 0)
                        {
                            Thread.Sleep(1000);
                        }
                        sensor = K.KinectSensor.KinectSensors.First();
                        Console.WriteLine("Kinect connected");
                        sensor.Start();
                        sensor.SkeletonStream.Enable(new K.TransformSmoothParameters()
                        {
                            JitterRadius = 0.1f,
                            Prediction = 0.2f,
                            Correction = 0.1f,
                            MaxDeviationRadius = 0.05f,
                            Smoothing = 0.2f
                        });
                        Console.WriteLine("Skeleton stream enabled");
                        sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
                        while (sensor.Status == K.KinectStatus.Connected)
                        {

                        }
                        Console.WriteLine("Kinect disconnected");
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                if (sensor != null)
                {
                    if (sensor.SkeletonStream.IsEnabled) sensor.SkeletonStream.Disable();
                    if (sensor.IsRunning) sensor.Stop();
                }
            }
        }

        K.Skeleton FindValidSkeleton(K.Skeleton[] skeletons)
        {
            foreach (var s in skeletons)
            {
                if (s.TrackingState == K.SkeletonTrackingState.Tracked) return s;
            }
            return null;
        }

        private void Sensor_SkeletonFrameReady(object sender, K.SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                K.Skeleton[] skeletons = new K.Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var s = FindValidSkeleton(skeletons);
                if (s == null) return;
                var rot = Matrix4x4.CreateFromYawPitchRoll(0, (float)VRE.Vridge.API.Client.Helpers.MathHelpers.DegToRad(-180), 0);
                var LeftHandRot = rot * s.BoneOrientations[K.JointType.HandLeft].AbsoluteRotation.Quaternion.ToAccord();
                var LeftHandPos = s.Joints[K.JointType.HandLeft].Position.ToAccord();
                var RightHandRot = rot * s.BoneOrientations[K.JointType.HandRight].AbsoluteRotation.Quaternion.ToAccord();
                var RightHandPos = s.Joints[K.JointType.HandRight].Position.ToAccord();
                var HeadPos = s.Joints[K.JointType.ShoulderCenter].Position.ToAccord();
                var transform = Matrix4x4.CreateTranslation(new Vector3(0, 2, 0)) *
                    Matrix4x4.CreateDiagonal(new Vector4(1.5f, 1.5f, 1.5f, 1.0f));
                PositionFrameReady?.Invoke(this, new PositionFrame(
                    LeftHandPos, LeftHandRot, RightHandPos, RightHandRot, HeadPos, transform));
            }
        }

        public void Dispose()
        {
            _service.Abort();
        }
    }
}
