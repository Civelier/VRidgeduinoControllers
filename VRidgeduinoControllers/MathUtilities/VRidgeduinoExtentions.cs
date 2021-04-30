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
    public delegate Vector4 Vector4Swap(float x, float y, float z, float w);
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

        public static Vector4 ToAccord(this N.Quaternion quaternion)
        {
            return new Vector4(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
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

        public static Vector3 GetEulerAngles(this Vector4 quaternion)
        {
            // roll (x-axis rotation)
            float sinr_cosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            float cosr_cosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            float pitch;
            // pitch (y-axis rotation)
            float sinp = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            if (Math.Abs(sinp) >= 1)
                pitch = (float)(Math.PI / 2) * sinp.GetSign(); // use 90 degrees if out of range
            else
                pitch = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosy_cosp = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(yaw, pitch, roll);
        }

        public static Matrix3x3 To3X3RotationMatrixInhomo(this Vector4 quaternion)
        {
            float q0 = quaternion.W, q1 = quaternion.X, q2 = quaternion.Y, q3 = quaternion.Z;

            return new Matrix3x3()
            {
                V00 = 1 - 2 * (q2 * q2 + q3 * q3),
                V01 = 2 * (q1 * q2 - q0 * q3),
                V02 = 2 * (q0 * q2 + q1 * q3),
                V10 = 2 * (q1 * q2 + q0 * q3),
                V11 = 1 - 2 * (q1 * q1 + q3 * q3),
                V12 = 2 * (q2 * q3 - q0 * q1),
                V20 = 2 * (q1 * q3 - q0 * q2),
                V21 = 2 * (q0 * q1 + q2 * q3),
                V22 = 1 - 2 * (q1 * q1 + q2 * q2)
            };
        }

        public static N.Matrix4x4 ToNumericsRotationMatrix(this Vector4 quaternion)
        {
            return N.Matrix4x4.CreateFromQuaternion(quaternion.ToNumericsQuaternion());
        }

        public static Matrix3x3 To3X3RotationMatrixHomo(this Vector4 quaternion)
        {
            quaternion.Normalize();
            float q0 = quaternion.W, q1 = quaternion.X, q2 = quaternion.Y, q3 = quaternion.Z;

            return new Matrix3x3()
            {
                V00 = q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3,
                V01 = 2 * (q1 * q2 - q0 * q3),
                V02 = 2 * (q0 * q2 + q1 * q3),
                V10 = 2 * (q1 * q2 + q0 * q3),
                V11 = q0 * q0 - q1 * q1 + q2 * q2 - q3 * q3,
                V12 = 2 * (q2 * q3 - q0 * q1),
                V20 = 2 * (q1 * q3 - q0 * q2),
                V21 = 2 * (q0 * q1 + q2 * q3),
                V22 = q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3,
            };
        }

        public static Matrix4x4 ToRotationMatrix(this Vector4 quaternion)
        {
            return quaternion.ToNumericsRotationMatrix().ToAccord();
        }

        public static Vector4 SwapComponents(this Vector4 quaternion, Vector4Swap swap)
        {
            return swap(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static Vector4 ToQuaternion(this Matrix3x3 rotationMatrix)
        {
            float qw = (float)Math.Sqrt(1 + rotationMatrix.V00 + rotationMatrix.V11 + rotationMatrix.V22) / 2;
            float qx = (rotationMatrix.V21 - rotationMatrix.V12) / (4 * qw);
            float qy = (rotationMatrix.V02 - rotationMatrix.V20) / (4 * qw);
            float qz = (rotationMatrix.V10 - rotationMatrix.V01) / (4 * qw);
            return new Vector4(qx, qy, qz, qw);
        }

        public static IEnumerable<Vector3> TransformPoints(this IEnumerable<Vector3> vectors, Matrix4x4 matrix)
        {
            foreach (var v in vectors)
            {
                yield return (matrix * v.ToVector4()).ToVector3();
            }
        }

        public static Vector3 Opposite(this Vector3 vector)
        {
            return new Vector3() - vector;
        }

        public static Vector4 Opposite(this Vector4 vector)
        {
            return new Vector4() - vector;
        }
    }
}
