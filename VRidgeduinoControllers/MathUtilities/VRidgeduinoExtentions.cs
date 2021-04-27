using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N = System.Numerics;
using Accord;
using Accord.Math;
using K = Microsoft.Kinect;

namespace VRidgeduinoControllers.MathUtilities
{
    public static class VRidgeduinoExtentions
    {
        public static Vector4 ToAccord(this K.Vector4 quaternion)
        {
            return new Vector4(quaternion.X, quaternion.Z, quaternion.Y, quaternion.W);
        }

        public static Vector3 ToAccord(this N.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static N.Vector3 ToNumerics(this Vector3 vector)
        {
            return new N.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector4[] ToRows(this N.Matrix4x4 matrix)
        {
            return new Vector4[]
            {
                new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14),
                new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24),
                new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34),
                new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44),
            };
        }

        public static Matrix4x4 ToAccord(this N.Matrix4x4 matrix)
        {
            var rows = matrix.ToRows();
            return Matrix4x4.CreateFromRows(rows[0], rows[1], rows[2], rows[3]);
        }

        public static N.Quaternion ToNumericsQuaternion(this Vector4 vector)
        {
            return new N.Quaternion(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static N.Vector4 ToNumerics(this Vector4 vector)
        {
            return new N.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static Vector3 ToAccord(this K.SkeletonPoint point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }
    }
}
