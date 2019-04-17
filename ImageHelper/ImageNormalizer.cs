using System;
using System.Drawing;

namespace ImageHelper
{
	public static class ImageNormalizer
	{
		public static double[] GetNormalizedInput(Bitmap image, int requiredWidth, int requiredHeight)
		{
			var result = new double[requiredHeight * requiredWidth];

			var thumb = new Bitmap(image.GetThumbnailImage(requiredWidth, requiredHeight, null, IntPtr.Zero));

			for (int hp = 0; hp < requiredHeight; hp++)
				for (int wp = 0; wp < requiredWidth; wp++)
					result[hp * requiredHeight + wp] = (double)thumb.GetPixel(wp, hp).ToArgb() / 10000000;

			return result;
		}

		public static double[] GetNormalizedInput(string filename, int requiredWidth, int requiredHeight)
			=> GetNormalizedInput(new Bitmap(filename), requiredWidth, requiredHeight);
	}
}
