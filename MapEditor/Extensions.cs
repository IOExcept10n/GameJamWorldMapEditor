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

        public static Color GetContrast(this Color background, Color foreground = default)
        {
            if (foreground == default)
                foreground = Colors.White;
            double backgroundLuminance = background.R * 0.2126 + background.G * 0.7152 + background.B * 0.0722;
            double foregroundLuminance = foreground.R * 0.2126 + foreground.G * 0.7152 + foreground.B * 0.0722;

            double contrastRatio = (foregroundLuminance + 0.05) / (backgroundLuminance + 0.05);

            if (contrastRatio >= 4.5)
            {
                // The contrast ratio meets the AA level requirement
                // No need to change the foreground color
            }
            else if (contrastRatio >= 3.0)
            {
                // The contrast ratio meets the AA level requirement for large text
                // Change the foreground color to a darker shade
                foreground = Color.FromArgb(255, (byte)(foreground.R - 50), (byte)(foreground.G - 50), (byte)(foreground.B - 50));
            }
            else
            {
                // The contrast ratio does not meet the AA level requirement
                // Change the foreground color to a much darker shade
                foreground = Color.FromArgb(255, (byte)(foreground.R - 100), (byte)(foreground.G - 100), (byte)(foreground.B - 100));
            }
            return foreground;
        }
    }
}
