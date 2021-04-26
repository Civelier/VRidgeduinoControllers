using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;

namespace VRidgeduinoControllers.Remotes
{
    public struct PositionFrame
    {
        public PositionFrame(Vector3 leftPos,
            Vector4 leftRot, 
            Vector3 rightPos,
            Vector4 rightRot,
            Vector3 headPos,
            Matrix4x4 transform)
        {
            LeftPos = (transform * leftPos.ToVector4()).ToVector3();
            LeftRot = leftRot;
            RightPos = (transform * rightPos.ToVector4()).ToVector3();
            RightRot = rightRot;
            HeadPos = (transform * headPos.ToVector4()).ToVector3();
        }

        public Vector3 LeftPos { get; }
        public Vector4 LeftRot { get; }
        public Vector3 RightPos { get; }
        public Vector4 RightRot { get; }
        public Vector3 HeadPos { get; }
    }
}
