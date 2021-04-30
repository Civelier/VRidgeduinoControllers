using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRE.Vridge.API.Client.Remotes;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using Accord.Math;
using VRidgeduinoControllers.MathUtilities;
using VRE.Vridge.API.Client.Messages.v3.Controller;

namespace VRidgeduinoControllers.Remotes
{
    public class SafeControllerRemote
    {
        private static int _id = 0;
        private readonly VridgeRemote _remote;
        public float RotationOffset = 0;
        public Vector3 Position { get; set; }
        public Vector3 Offset { get; set; }
        public Vector4 Rotation => Info.Rotation;
        public Vector4 ConvertedRotation { get; set; }
        public Vector3 EulerRotation { get; set; }
        public ControllerUpdateInfo Info { get; set; }
        public readonly int ID;

        private readonly HandType Hand;

        public SafeControllerRemote(VridgeRemote remote, HandType hand)
        {
            ID = _id++;
            _remote = remote;
            Hand = hand;
        }

        private bool TryGetController(out ControllerRemote controller)
        {
            try
            {
                if (_remote.Controller != null && !_remote.Controller.IsDisposed)
                {
                    controller = _remote.Controller;
                    return true;
                }
            }
            catch (ObjectDisposedException) { }
            controller = null;
            return false;
        }

        public bool TryUpdateController()
        {
            try
            {
                if (TryGetController(out ControllerRemote controller))
                {
                    ConvertedRotation = Matrix4x4.CreateFromYawPitchRoll(VRidgeduinoMath.ToRadians(90), 0, 0)
                        //* Matrix4x4.CreateFromYawPitchRoll(-RotationOffset, 0, 0)
                            * Info.Rotation.SwapComponents((y, x, z, w) => new Vector4(x, z, y, w));
                    //ConvertedRotation = Matrix4x4.CreateFromYawPitchRoll(0, VRidgeduinoMath.ToRadians(90), 0) *
                    //    Rotation.SwapComponents((x, y, z, w) => new Vector4(y, z, x, w));
                    EulerRotation = ConvertedRotation.GetEulerAngles();

                    //ConvertedRotation = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(Offset.X, Offset.Y, Offset.Z).ToAccord() * 
                    //    ConvertedRotation;

                    controller.SetControllerState(
                        (int)Info.Hand - 1,
                        HeadRelation.Unrelated,
                        Info.Hand,
                        ConvertedRotation.ToNumericsQuaternion(),
                        Position.ToNumerics(),
                        Info.AnalogX,
                        Info.AnalogY,
                        Info.AnalogTrigger,
                        Info.Menu,
                        Info.System,
                        Info.Trig,
                        Info.Grip,
                        Info.TouchPress,
                        Info.TouchPress);
                    return true;
                }
            }
            catch (ObjectDisposedException) { }
            return false;
        }

        public void ResetRotation()
        {
            RotationOffset += EulerRotation.X;
        }
    }
}
