using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRE.Vridge.API.Client.Messages.BasicTypes;

namespace VRidgeduinoControllers.Remotes
{
    public struct ControllerUpdateInfo
    {
        public HandType Hand { get; private set; }
        public float AnalogX { get; private set; }
        public float AnalogY { get; private set; }
        public float AnalogTrigger { get; private set; }
        public bool Grip { get; private set; }
        public bool Trig { get; private set; }
        public bool Menu { get; private set; }
        public bool System { get; private set; }
        public bool TouchPress { get; private set; }
        


        public static ControllerUpdateInfo? FromPacketPosRot(byte[] packet)
        {
            if (packet.Length < 2) return null;
            var s = Encoding.ASCII.GetString(packet).Split(' ');
            return new ControllerUpdateInfo()
            {
                Hand = s[0] == "1" ? HandType.Left : HandType.Right,
                Trig = s[1] == "1",
                AnalogTrigger = s[1] == "1" ? 1 : 0,
            };
        }
    }
}
