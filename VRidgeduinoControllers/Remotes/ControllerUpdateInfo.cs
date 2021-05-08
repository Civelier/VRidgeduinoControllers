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
        public float Battery { get; private set; }
        public bool Option1 { get; private set; }
        public bool Option2 { get; private set; }


        public static ControllerUpdateInfo? FromPacket(byte[] packet)
        {
            var s = Encoding.ASCII.GetString(packet).Split(' ');
            if (s.Length < 15) return null;

            float x = VRidgeduinoMath.Clamp(float.Parse(s[8]), -1, 1);
            float y = VRidgeduinoMath.Clamp(float.Parse(s[9]), -1, 1);

            if (x < 0.1 && x > -0.1) x = 0;
            if (y < 0.1 && y > -0.1) y = 0;


            return new ControllerUpdateInfo()
            {
                Hand = s[0] == "1" ? HandType.Left : HandType.Right,
                TouchPress = s[1] == "1",
                Grip = s[2] == "1",
                Trig = s[3] == "1",
                AnalogTrigger = s[3] == "1" ? 1 : 0,
                Menu = s[4] == "1",
                System = s[5] == "1",
                Option1 = s[6] == "1",
                Option2 = s[7] == "1",
                AnalogX = y,
                AnalogY = x,
                Battery = float.Parse(s[10]),
                Rotation = new Vector4(float.Parse(s[11]), float.Parse(s[12]),
                float.Parse(s[13]), float.Parse(s[14])),
            };
        }
    }
}
