using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRidgeduinoControllers.MathUtilities
{
    public static class VRidgeduinoMath
    {
        public const float DegToRad = 0.01745329252f;
        public static Vector4 ToQuaternion(float yaw, float pitch, float roll)
        {
            //float cy = (float)Math.Cos(yaw * 0.5f);
            //float sy = (float)Math.Sin(yaw * 0.5f);
            //float cp = (float)Math.Cos(pitch * 0.5f);
            //float sp = (float)Math.Sin(pitch * 0.5f);
            //float cr = (float)Math.Cos(roll * 0.5f);
            //float sr = (float)Math.Sin(roll * 0.5f);

            //return new Vector4(
            //    cr * cp * cy + sr * sp * sy,
            //    sr * cp * cy - cr * sp * sy,
            //    cr * sp * cy + sr * cp * sy,
            //    cr * cp * sy - sr * sp * cy);
            return Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * new Vector4(1, 0, 0, 0);
        }

        public static Vector4 ToQuaternion(Vector3 eulerAngles)
        {
            return ToQuaternion(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        }

        public static float ToRadians(float degrees)
        {
            return degrees * DegToRad;
        }

        public static float ToDegrees(float radians)
        {
            return radians / DegToRad;
        }

        public static Vector3 ToRadiansEuler(Vector3 eulerDegrees)
        {
            return eulerDegrees * DegToRad;
        }

        public static Vector3 ToDegreesEuler(Vector3 eulerRadians)
        {
            return eulerRadians / DegToRad;
        }

        public static Matrix4x4 CreateScaleMatrix(Vector3 scale)
        {
            return Matrix4x4.CreateDiagonal(scale.ToVector4());
        }

        public static Matrix4x4 Multiply(params Matrix4x4[] matricies)
        {
            Matrix4x4 result = matricies[0];
            for (int i = 1; i < matricies.Length; i++)
            {
                result *= matricies[i];
            }
            return result;
        }

        public static Matrix4x4 CreateProjectionMatrix(float angleOfView, float near, float far)
        {
            float scale = 1 / (float)Math.Tan(ToRadians(angleOfView) / 2);
            return new Matrix4x4()
            {
                V00 = scale,
                V11 = scale,
                V22 = -far / (far - near),
                V32 = -far * near / (far - near),
                V23 = -1,
                V33 = 0
            };
        }

        public static Vector3 EulerAngleBetween(Vector3 a, Vector3 b)
        {
            a.Normalize();
            b.Normalize();
            return a - b;
        }
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float GetSign(this float value)
        {
            return Math.Abs(value) / value;
        }

        public static int GetSign(this int value)
        {
            return Math.Abs(value) / value;
        }

        public static double GetSign(this double value)
        {
            return Math.Abs(value) / value;
        }

        public static bool IsNear(this int v, int other, int treshold)
        {
            return Math.Abs(v - other) <= treshold;
        }

        public static bool IsNear(this double v, double other, double treshold)
        {
            return Math.Abs(v - other) <= treshold;
        }

        public static bool IsNear(this float v, float other, float treshold)
        {
            return Math.Abs(v - other) <= treshold;
        }

        public static TValue Middle<TValue>(this IEnumerable<TValue> values)
        {
            return values.ElementAt(values.Count() / 2);
        }

        public static Vector3 ToVector3(this Point3 point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        public static float DistanceToPoint(this Point3 point, Point3 other)
        {
            return (point.ToVector3() - other.ToVector3()).Norm;
        }
    }
}
