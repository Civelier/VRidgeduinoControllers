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
using VRidgeduinoControllers.Services;
using VRidgeduinoControllers.Remotes;

namespace VRidgeduinoControllers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        struct DebugState
        {
            private bool _rightRemoteBtn1;
            private bool _rightRemoteBtn2;
            private bool _leftRemoteBtn1;
            private bool _leftRemoteBtn2;
            private bool _bothBtn2Down;

            public event EventHandler RightBtn1Pressed;
            public event EventHandler RightBtn2Pressed;
            public event EventHandler LeftBtn1Pressed;
            public event EventHandler LeftBtn2Pressed;
            public event EventHandler BothBtn2Pressed;

            public void UpdateRemotes(SafeControllerRemote right, SafeControllerRemote left)
            {
                if (_bothBtn2Down && (right.Info.Option2 && left.Info.Option2)) return;
                if (_bothBtn2Down)
                {
                    _bothBtn2Down = false;
                    BothBtn2Pressed?.Invoke(this, new EventArgs());
                    _rightRemoteBtn2 = false;
                    _leftRemoteBtn2 = false;

                }
                
                if (right.Info.Option2 && left.Info.Option2)
                {
                    _bothBtn2Down = true;
                    _rightRemoteBtn2 = false;
                    _leftRemoteBtn2 = false;
                }

                if (_rightRemoteBtn1 && !right.Info.Option1) RightBtn1Pressed?.Invoke(this, new EventArgs());
                if (_rightRemoteBtn2 && !right.Info.Option2) RightBtn2Pressed?.Invoke(this, new EventArgs());
                
                if (_leftRemoteBtn1 && !left.Info.Option1) LeftBtn1Pressed?.Invoke(this, new EventArgs());
                if (_leftRemoteBtn2 && !left.Info.Option2) LeftBtn2Pressed?.Invoke(this, new EventArgs());
                

                _rightRemoteBtn1 = right.Info.Option1;
                _rightRemoteBtn2 = right.Info.Option2;
                
                _leftRemoteBtn1 = left.Info.Option1;
                _leftRemoteBtn2 = left.Info.Option2;
            }
        }

        DebugState _debug = new DebugState();
        VRidge.Remotes.VridgeRemote Remote;
        SafeControllerRemote LeftRemote;
        SafeControllerRemote RightRemote;
        SafeHeadRemote Head;
        private const int listenPort = 7000;
        VRidgeduinoCommunicationService CommunicationService;
        KinectService Kinect;
        Thread RemoteHandler;
        bool _trackLeft = true;
        bool _trackRight = true;

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
            CommunicationService = new VRidgeduinoCommunicationService();
            CommunicationService.Start();
            Kinect = new KinectService();
            RemoteHandler = new Thread(RemoteRoutine);
            RemoteHandler.Start();
            _debug.LeftBtn2Pressed += _debug_LeftBtn2Pressed;
            _debug.RightBtn2Pressed += _debug_RightBtn2Pressed;
            _debug.BothBtn2Pressed += _debug_BothBtn2Pressed;
        }

        private void _debug_BothBtn2Pressed(object sender, EventArgs e)
        {
            Head.TryResetPosition();
        }

        private void _debug_RightBtn2Pressed(object sender, EventArgs e)
        {
            _trackRight = !_trackRight;
        }

        private void _debug_LeftBtn2Pressed(object sender, EventArgs e)
        {
            _trackLeft = !_trackLeft;
        }

        private void CommunicationService_LeftControllerUpdateAvailable(object sender, ControllerUpdateInfo e)
        {
            LeftRemote.Info = e;
        }


        void VRidgeStateDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                VRidgeStateLabel.Content = !VRidgeHeadOK && !VRidgeControllersOK ? "VRidge off" :
                    VRidgeHeadOK && VRidgeControllersOK ? "VRidge on" :
                    !VRidgeControllersOK ? "VRidge controllers off" :
                    "VRidge head off";

                RightTrigLabel.Content = $"Trig: {RightRemote.Info.Trig}";
                RightGripLabel.Content = $"Grip: {RightRemote.Info.Grip}";
                RightAnalogLabel.Content = $"X: {RightRemote.Info.AnalogX} Y: {RightRemote.Info.AnalogY}";
                RightStickLabel.Content = $"Stick: {RightRemote.Info.TouchPress}";
                RightQuaternionLabel.Content = $"Quaternion: {RightRemote.Info.Rotation}";
                RightBatteryVoltageLabel.Content = $"Battery: {RightRemote.Info.Battery}";
                ROption1.Content = $"Option1: {RightRemote.Info.Option1}";
                ROption2.Content = $"Option2: {RightRemote.Info.Option2}";

                LeftTrigLabel.Content = $"Trig: {LeftRemote.Info.Trig}";
                LeftGripLabel.Content = $"Grip: {LeftRemote.Info.Grip}";
                LeftAnalogLabel.Content = $"X: {LeftRemote.Info.AnalogX} Y: {LeftRemote.Info.AnalogY}";
                LeftStickLabel.Content = $"Stick: {LeftRemote.Info.TouchPress}";
                LeftQuaternionLabel.Content = $"Quaternion: {LeftRemote.Info.Rotation}";
                LeftBatteryVoltageLabel.Content = $"Battery: {LeftRemote.Info.Battery}";
                LOption1.Content = $"Option1: {LeftRemote.Info.Option1}";
                LOption2.Content = $"Option2: {LeftRemote.Info.Option2}";
            });
        }

        void RemoteRoutine()
        {
            try
            {
                using (Remote = 
                    new VRidge.Remotes.VridgeRemote("localhost", 
                    "VRidgeduinoControllers", 
                    VRidge.Remotes.Capabilities.Controllers | 
                    VRidge.Remotes.Capabilities.HeadTracking))
                {
                    LeftRemote = new SafeControllerRemote(Remote, 
                        VRidge.Messages.BasicTypes.HandType.Left);
                    RightRemote = new SafeControllerRemote(Remote, 
                        VRidge.Messages.BasicTypes.HandType.Right);
                    Head = new SafeHeadRemote(Remote);

                    CommunicationService.LeftControllerUpdateAvailable += 
                        CommunicationService_LeftControllerUpdateAvailable;
                    CommunicationService.RightControllerUpdateAvailable += 
                        CommunicationService_RightControllerUpdateAvailable;
                    Kinect.PositionFrameReady += Kinect_PositionFrameReady;

                    Head.Position = new Vector3(0f, 2.5f, 1.3f);
                    LeftRemote.Position = new Vector3(-0.2f, 2.2f, 1f);
                    RightRemote.Position = new Vector3(0.2f, 2.2f, 1f);
                    int i = 0;
                    while (true)
                    {
                        if (i++ == 10)
                        {
                            VRidgeStateDisplay();
                            Dispatcher.Invoke(() =>
                            {
                                LeftConvertedQuaternionLabel.Content = LeftRemote.ConvertedRotation;
                                LeftEuler.Content = LeftRemote.EulerRotation;
                            });
                            Dispatcher.Invoke(() =>
                            {
                                RightConvertedQuaternionLabel.Content = RightRemote.ConvertedRotation;
                                RightEuler.Content = RightRemote.EulerRotation;
                            });
                            i = 0;
                        }
                        if (LeftRemote.TryUpdateController())
                        {
                            
                        }
                        if (RightRemote.TryUpdateController())
                        {
                            
                        }
                        Head.TryUpdateHead();
                        DebugFeatures();
                        Thread.Sleep(16);
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        private void Kinect_PositionFrameReady(object sender, PositionFrame e)
        {
            if (_trackLeft) LeftRemote.Position = e.LeftPos;
            if (_trackRight) RightRemote.Position = e.RightPos;
            Head.Position = e.HeadPos;
        }

        
        private void DebugFeatures()
        {
            _debug.UpdateRemotes(RightRemote, LeftRemote);
        }

        private void CommunicationService_RightControllerUpdateAvailable(object sender, ControllerUpdateInfo e)
        {
            RightRemote.Info = e;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            CommunicationService.Stop();
            Kinect.Dispose();
            RemoteHandler.Abort();
        }

        //private Vector4 CreateRotation(double x, double y, double z)
        //{
        //    var euler = VRidgeduinoMath.ToRadiansEuler(new Vector3((float)x, (float)y, (float)z));

        //    return System.Numerics.Quaternion.CreateFromYawPitchRoll(euler.X, euler.Y, euler.Z).ToAccord();
        //}

        private Vector3 CreateOffset(double x, double y, double z)
        {
            return VRidgeduinoMath.ToRadiansEuler(new Vector3((float)x, (float)y, (float)z));
        }

        private void RightXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //RightRemote.ConvertedRotation = CreateRotation(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);
            RightRemote.Offset = CreateOffset(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);
        }

        private void RightYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //RightRemote.ConvertedRotation = CreateRotation(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);
            RightRemote.Offset = CreateOffset(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);

        }

        private void RightZSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //RightRemote.ConvertedRotation = CreateRotation(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);
            RightRemote.Offset = CreateOffset(RightXSlider.Value, RightYSlider.Value, RightZSlider.Value);
        }
    }
}
