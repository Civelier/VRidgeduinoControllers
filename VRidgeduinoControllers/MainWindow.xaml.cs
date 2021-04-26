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
using Microsoft.Kinect;

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
        public MainWindow()
        {
            InitializeComponent();
            UDPListener = new Thread(ListenerRoutine);
            UDPListener.Start();
            RemoteHandler = new Thread(RemoteRoutine);
            RemoteHandler.Start();
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
            KinectSensor sensor = null;
            try
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Waiting for kinect sensor");
                        while (KinectSensor.KinectSensors.Count == 0)
                        {
                            Thread.Sleep(1000);
                        }
                        sensor = KinectSensor.KinectSensors.First();
                        Console.WriteLine("Kinect connected");
                        sensor.Start();
                        sensor.SkeletonStream.Enable();
                        Console.WriteLine("Skeleton stream enabled");
                        sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
                        while (sensor.Status == KinectStatus.Connected)
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

        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame.SkeletonArrayLength == 0) return;
                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var s = skeletons.First();
                s.BoneOrientations[JointType.HandLeft].AbsoluteRotation.
            }
        }

        void UpdateController(int type, int btn1)
        {
            if (Remote.Controller == null) return;
            Remote.Controller.SetControllerState(
                controllerId: type - 1,
                headRelation: VRidge.Messages.v3.Controller.HeadRelation.Unrelated,
                suggestedHand: type == 1 ? VRidge.Messages.BasicTypes.HandType.Left :
                VRidge.Messages.BasicTypes.HandType.Right,
                orientation: System.Numerics.Quaternion.CreateFromYawPitchRoll(0, 0, 0),
                position: new System.Numerics.Vector3(0, 0, 0),
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
                using (Remote = new VRidge.Remotes.VridgeRemote("localhost", "VRidgeduinoControllers", VRidge.Remotes.Capabilities.Controllers))
                {
                    while (true)
                    {
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
