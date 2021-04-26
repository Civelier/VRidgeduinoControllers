using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRE.Vridge.API.Client.Remotes;
using VRidgeduinoControllers.Remotes;

namespace VRidgeduinoControllers.Services
{
    public class VRidgeduinoCommunicationService
    {
        private readonly int _port;
        Thread _service;

        public event EventHandler<ControllerUpdateInfo> LeftControllerUpdateAvailable;
        public event EventHandler<ControllerUpdateInfo> RightControllerUpdateAvailable;

        public VRidgeduinoCommunicationService(int port = 7000)
        {
            _service = new Thread(Routine);
            _port = port;
        }

        public void Start()
        {
            _service.Start();
        }
        
        public void Stop()
        {
            _service.Abort();
        }

        void Routine()
        {
            using (UdpClient listener = new UdpClient(_port))
            {
                    IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, _port);
                DateTime lastLeftConnection = DateTime.MinValue;
                DateTime lastRightConnection = DateTime.MinValue;
                try
                {
                    while (true)
                    {
                        ControllerUpdateInfo? info = ControllerUpdateInfo
                            .FromPacketPosRot(listener.Receive(ref groupEP));
                        if (info.HasValue)
                        {
                            if (info.Value.Hand == VRE.Vridge.API.Client.Messages.BasicTypes.HandType.Left)
                            {
                                LeftControllerUpdateAvailable?.Invoke(this, info.Value);
                            }
                            else
                            {
                                RightControllerUpdateAvailable?.Invoke(this, info.Value);
                            }
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
                catch (TaskCanceledException)
                {

                }
            }
        }
    }
}
