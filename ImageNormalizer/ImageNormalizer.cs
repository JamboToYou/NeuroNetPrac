using System;
using System.Drawing;

namespace ImageNormalizer
{
	public static class ImageNormalizer
	{
		public static double[] GetNormalizedInputs(Bitmap image, int requiredWidth, int requiredHeight)
		{
			var result = new double[requiredHeight * requiredWidth];

			var thumb = new Bitmap(image.GetThumbnailImage(requiredWidth, requiredHeight, null, IntPtr.Zero));

			for (int hp = 0; hp < requiredHeight; hp++)
				for (int wp = 0; wp < requiredWidth; wp++)
					result[hp * requiredHeight + wp] = thumb.GetPixel(wp, hp).ToArgb();

			return result;
		}

		public static double[] GetNormalizedInputs(string filename, int requiredWidth, int requiredHeight)
			=> GetNormalizedInputs(new Bitmap(filename), requiredWidth, requiredHeight);
	}
}
