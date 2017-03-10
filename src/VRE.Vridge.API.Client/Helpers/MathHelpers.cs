using System;

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
    }
}
