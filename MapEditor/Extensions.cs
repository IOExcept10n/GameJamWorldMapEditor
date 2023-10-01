using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapEditor
{
    internal static class Extensions
    {
        public static int ToInt(this Version version)
        {
            return version == null ? 0 : 
                version.Major << 24 +
                version.Minor << 16 +
                version.Build << 8 +
                version.Revision;
        }

        public static Color GetContrast(this Color background, int grayscaleThreshold = 30)
        {
            int delta = background.GetRgbDelta();
            if (delta <= grayscaleThreshold) return Colors.Red;
            return Color.FromArgb(255, (byte)(255 - background.R), (byte)(255 - background.G), (byte)(255 - background.B));
        }

        /// <summary>
        /// Utility method that encapsulates the RGB Delta calculation:
        /// delta = abs(R-G) + abs(G-B) + abs(B-R) 
        /// So, between the color RGB(50,100,50) and RGB(128,128,128)
        /// The first would be the higher delta with a value of 100 as compared
        /// to the second color which, being grayscale, would have a delta of 0
        /// </summary>
        /// <param name="color">The color for which to calculate the delta</param>
        /// <returns>An integer in the range 0 to 510 indicating the difference
        /// in the RGB values that comprise the color</returns>
        private static int GetRgbDelta(this Color color)
        {
            return
                Math.Abs(color.R - color.G) +
                Math.Abs(color.G - color.B) +
                Math.Abs(color.B - color.R);
        }
    }
}
