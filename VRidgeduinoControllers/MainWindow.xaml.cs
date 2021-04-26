using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VRidge = VRE.Vridge.API.Client;
using VRE.Vridge.API.Client;
//using Microsoft.Kinect;
using Accord;
using Accord.Math;
using K = Microsoft.Kinect;
using VRidgeduinoControllers.MathUtilities;

namespace VRidgeduinoControllers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VRidge.Remotes.VridgeRemote Remote;
        private const int listenPort = 7000;
        Thread UDPListener;
        Thread RemoteHandler;
        Thread KinectHandler;
        Vector3 LeftHandPos;
        Vector3 RightHandPos;
        Vector3 HeadPos;
        Vector4 LeftHandRot;
        Vector4 RightHandRot;

        bool VRidgeHeadOK
        {
            get
            {
                try
                {
                    return Remote != null && Remote.Head != null &&
                        !Remote.Head.IsDisposed;

                }
                catch (ObjectDisposedException)
                {

                }
                return false;
            }
        }

        bool VRidgeControllersOK
        {
            get
            {
                try
                {
                    return Remote != null && Remote.Controller != null &&
                        !Remote.Controller.IsDisposed;

                }
                catch (ObjectDisposedException)
                {

                }
                return false;
            }
        }
         

        public MainWindow()
        {
            InitializeComponent();
            UDPListener = new Thread(ListenerRoutine);
            UDPListener.Start();
            RemoteHandler = new Thread(RemoteRoutine);
            RemoteHandler.Start();
            KinectHandler = new Thread(KinectRoutine);
            KinectHandler.Start();
        }

        void ListenerRoutine()
        {
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            DateTime lastLeftConnection = DateTime.MinValue;
            DateTime lastRightConnection = DateTime.MinValue;

            try
            {
                while (true)
                {
                    if (listener.Available > 0)
                    {
                        byte[] bytes = listener.Receive(ref groupEP);

                        string[] s = Encoding.ASCII.GetString(bytes).Split(' ');
                        int type = 0;
                        int btn = 0;
                        if (int.TryParse(s[0], out type))
                        {
                        }
                        if (int.TryParse(s[1], out btn))
                        {
                            
                        }
                        if (Remote != null && type != 0)
                        {
                            UpdateController(type, btn);
                        }
                        Dispatcher.Invoke(
                            () =>
                            {
                                if (type == 1) lastLeftConnection = DateTime.Now;
                                if (type == 2) lastRightConnection = DateTime.Now;
                                ButtonLabel.Content = btn;
                                if (btn == 1) BtnRectangle.Visibility = Visibility.Visible;
                                else BtnRectangle.Visibility = Visibility.Hidden;
                                //if (float.TryParse(s[2], out float x))
                                //{
                                //    GyroX.Content = x;
                                //}
                                //if (float.TryParse(s[3], out float y))
                                //{
                                //    GyroY.Content = y;
                                //}
                                //if (float.TryParse(s[4], out float z))
                                //{
                                //    GyroZ.Content = z;
                                //}

                            });
                    }
                    Dispatcher.Invoke(() =>
                    {
                        if (DateTime.Now - lastLeftConnection > TimeSpan.FromSeconds(3))
                        {
                            LeftConnectionLabel.Content = "Disconnected";
                        }
                        else
                        {
                            LeftConnectionLabel.Content = "Connected";
                        }
                        if (DateTime.Now - lastRightConnection > TimeSpan.FromSeconds(3))
                        {

                        }
                        else
                        {

                        }
                    });
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                listener.Close();
            }
        }

        void KinectRoutine()
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
                        sensor.SkeletonStream.Enable();
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
                if (frame.FrameNumber % 10 == 0) Console.WriteLine($"Skeletons found: {frame.SkeletonArrayLength}");
                K.Skeleton[] skeletons = new K.Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var s = FindValidSkeleton(skeletons);
                if (s == null) return;
                LeftHandRot = s.BoneOrientations[K.JointType.HandLeft].AbsoluteRotation.Quaternion.ToAccord();
                LeftHandPos = s.Joints[K.JointType.HandLeft].Position.ToAccord();
                RightHandRot = s.BoneOrientations[K.JointType.HandRight].AbsoluteRotation.Quaternion.ToAccord();
                RightHandPos = s.Joints[K.JointType.HandRight].Position.ToAccord();
                HeadPos = s.Joints[K.JointType.ShoulderCenter].Position.ToAccord();
                Dispatcher.Invoke(() =>
                {
                    LeftHandPosLabel.Content = $"Left hand pos: {LeftHandPos}";
                    LeftHandRotLabel.Content = $"Left hand rot: {LeftHandRot}";
                });
                if (!VRidgeHeadOK) return;
                Remote.Head.SetPosition(HeadPos.X, HeadPos.Y, HeadPos.Z);
            }
        }

        void VRidgeStateDisplay()
        {
            Dispatcher.Invoke(() => 
            VRidgeStateLabel.Content = !VRidgeHeadOK && !VRidgeControllersOK ? "VRidge off" :
                VRidgeHeadOK && VRidgeControllersOK ? "VRidge on" :
                !VRidgeControllersOK ? "VRidge controllers off" :
                "VRidge head off");
        }

        void UpdateController(int type, int btn1)
        {
            if (!VRidgeControllersOK) return;
            Remote.Controller.SetControllerState(
                controllerId: type - 1,
                headRelation: VRidge.Messages.v3.Controller.HeadRelation.Unrelated,
                suggestedHand: type == 1 ? VRidge.Messages.BasicTypes.HandType.Left :
                VRidge.Messages.BasicTypes.HandType.Right,
                orientation: type == 1 ? LeftHandRot.ToNumericsQuaternion() : RightHandRot.ToNumericsQuaternion(),
                position: type == 1 ? LeftHandPos.ToNumerics() : RightHandPos.ToNumerics(),
                analogX: 0,
                analogY: 0,
                analogTrigger: 0,
                isMenuPressed: false,
                isSystemPressed: false,
                isTriggerPressed: false,
                isGripPressed: btn1 == 1,
                isTouchpadPressed: false,
                isTouchpadTouched: false);
        }

        void RemoteRoutine()
        {
            try
            {
                while (true)
                {
                    using (Remote = 
                        new VRidge.Remotes.VridgeRemote("localhost", 
                        "VRidgeduinoControllers", 
                        VRidge.Remotes.Capabilities.Controllers | 
                        VRidge.Remotes.Capabilities.HeadTracking))
                    {
                        VRidgeStateDisplay();
                        Thread.Sleep(500);
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UDPListener.Abort();
            RemoteHandler.Abort();
            KinectHandler.Abort();
        }
    }
}
