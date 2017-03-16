using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace VRE.Vridge.API.DesktopTester
{
    public static class Extensions
    {
        /// <summary>
        /// Converts 4x4 matrix into flat array with column-major layout.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float[] FlattenAsColumnMajor(this Matrix3D matrix)
        {
            var array = new float[16];
            array[0] = (float)matrix.M11;
            array[1] = (float)matrix.M21;
            array[2] = (float)matrix.M31;
            array[3] = (float)matrix.OffsetX;
            array[4] = (float)matrix.M12;
            array[5] = (float)matrix.M22;
            array[6] = (float)matrix.M32;
            array[7] = (float)matrix.OffsetY;
            array[8] = (float)matrix.M13;
            array[9] = (float)matrix.M23;
            array[10] = (float)matrix.M33;
            array[11] = (float)matrix.OffsetZ;
            array[12] = (float)matrix.M14;
            array[13] = (float)matrix.M24;
            array[14] = (float)matrix.M34;
            array[15] = (float)matrix.M44;

            return array;
        }
    }
}
