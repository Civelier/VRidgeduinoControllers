using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRidgeduinoControllers.MathUtilities;

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
        public Vector4 Rotation { get; private set; }


        public static ControllerUpdateInfo? FromPacketPosRot(byte[] packet)
        {
            var s = Encoding.ASCII.GetString(packet).Split(' ');
            if (s.Length < 10) return null;

            float x = VRidgeduinoMath.Clamp(float.Parse(s[3]), -1, 1);
            float y = VRidgeduinoMath.Clamp(float.Parse(s[4]), -1, 1);

            if (x < 0.06 && x > -0.06) x = 0;
            if (y < 0.06 && y > -0.06) y = 0;


            return new ControllerUpdateInfo()
            {
                Hand = s[0] == "1" ? HandType.Left : HandType.Right,
                Trig = s[1] == "1",
                AnalogTrigger = s[1] == "1" ? 1 : 0,
                AnalogX = y,
                AnalogY = x,
                Grip = s[2] == "1",
                TouchPress = s[5] == "0",
                Rotation = new Vector4(float.Parse(s[6]), float.Parse(s[7]),
                float.Parse(s[8]), float.Parse(s[9])),
            };
        }
    }
}
