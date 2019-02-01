using System;
using System.Numerics;

namespace VRE.Vridge.API.Client.Helpers
{
    public static class MathHelpers
    {
        public static double RadToDeg(double rad)
        {
            return rad * (180.0 / Math.PI);
        }

        public static double DegToRad(double deg)
        {
            return Math.PI * deg / 180.0;
        }

        /// <summary>
        /// Converts 4x4 matrix into flat array with column-major layout.
        /// https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
        /// </summary>                
        public static float[] FlattenAsColumnMajor(this Matrix4x4 matrix)
        {
            var array = new float[16];
            array[0] = (float)matrix.M11;
            array[1] = (float)matrix.M21;
            array[2] = (float)matrix.M31;
            array[3] = (float)matrix.M41;
            array[4] = (float)matrix.M12;
            array[5] = (float)matrix.M22;
            array[6] = (float)matrix.M32;
            array[7] = (float)matrix.M42;
            array[8] = (float)matrix.M13;
            array[9] = (float)matrix.M23;
            array[10] = (float)matrix.M33;
            array[11] = (float)matrix.M43;
            array[12] = (float)matrix.M14;
            array[13] = (float)matrix.M24;
            array[14] = (float)matrix.M34;
            array[15] = (float)matrix.M44;

            return array;
        }

        public static float[] Flatten(this Matrix4x4 matrix)
        {
            var array = new float[16];
            array[0] = (float)matrix.M11;
            array[1] = (float)matrix.M12;
            array[2] = (float)matrix.M13;
            array[3] = (float)matrix.M14;
            array[4] = (float)matrix.M21;
            array[5] = (float)matrix.M22;
            array[6] = (float)matrix.M23;
            array[7] = (float)matrix.M24;
            array[8] = (float)matrix.M31;
            array[9] = (float)matrix.M32;
            array[10] = (float)matrix.M33;
            array[11] = (float)matrix.M34;
            array[12] = (float)matrix.M41;
            array[13] = (float)matrix.M42;
            array[14] = (float)matrix.M43;
            array[15] = (float)matrix.M44;

            return array;
        }
    }
}
