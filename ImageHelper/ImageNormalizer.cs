using System;
using System.Drawing;

namespace ImageHelper
{
	public static class ImageNormalizer
	{
		public static readonly int ARGB_MAX = 16777215;
		public static readonly int ARGB_MIN = -16777216;
		public static double[] GetNormalizedInput(Bitmap image, int requiredWidth, int requiredHeight)
		{
			double px;
			var result = new double[requiredHeight * requiredWidth];

			var thumb = new Bitmap(image.GetThumbnailImage(requiredWidth, requiredHeight, null, IntPtr.Zero));

			for (int hp = 0; hp < requiredHeight; hp++)
				for (int wp = 0; wp < requiredWidth; wp++)
				{
					px = thumb.GetPixel(wp, hp).ToArgb();
					result[hp * requiredHeight + wp] = (px - ARGB_MIN) / (ARGB_MAX - ARGB_MIN);
				}

			return result;
		}

		public static double[] GetNormalizedInput(string filename, int requiredWidth, int requiredHeight)
			=> GetNormalizedInput(new Bitmap(filename), requiredWidth, requiredHeight);

		public static double[] GetNormalizedBWInput(Bitmap image, int requiredWidth, int requiredHeight)
		{
			var result = new double[requiredWidth * requiredHeight];
			var img = new Bitmap(image.GetThumbnailImage(requiredWidth, requiredHeight, null, IntPtr.Zero));

			for (int h = 0; h < img.Height; h++)
			{
				for (int w = 0; w < img.Width; w++)
				{
					var clr = img.GetPixel(w, h);
					result[requiredHeight * h + w] = (double)(clr.R + clr.G + clr.B) / (3 * 255) ;
					System.Console.Write(result[requiredHeight * h + w] + " ");
				}
				System.Console.WriteLine();
			}

			return result;
		}

		public static double[] GetNormalizedBWInput(string filename, int requiredWidth, int requiredHeight)
			=> GetNormalizedBWInput(new Bitmap(filename), requiredWidth, requiredHeight);
	}
}
