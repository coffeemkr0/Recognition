using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Distortion.Blurring
{
    /// <summary>
    /// Extension methods for the Bitmap class that help with blurring images.
    /// </summary>
    public static class BlurringBitmapExtensions
    {
        #region Enums
        /// <summary>
        /// An enum that represents all of the supported blurring methods in the BlurringBitmapExtensions class.
        /// </summary>
        public enum BlurType
        {
            Mean3x3,
            Mean5x5,
            Mean7x7,
            Mean9x9,
            GaussianBlur3x3,
            GaussianBlur5x5,
            MotionBlur5x5,
            MotionBlur5x5At45Degrees,
            MotionBlur5x5At135Degrees,
            MotionBlur7x7,
            MotionBlur7x7At45Degrees,
            MotionBlur7x7At135Degrees,
            MotionBlur9x9,
            MotionBlur9x9At45Degrees,
            MotionBlur9x9At135Degrees,
            Median3x3,
            Median5x5,
            Median7x7,
            Median9x9,
            Median11x11
        }
        #endregion

        #region Private Methods
        private static Bitmap GetImage(this byte[] resultBuffer, int width, int height)
        {
            Bitmap resultBitmap = new Bitmap(width, height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                    resultBitmap.Width, resultBitmap.Height),
                                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        private static byte[] GetByteArray(this Bitmap sourceBitmap)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);

            byte[] sourceBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0,
                                       sourceBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            return sourceBuffer;
        }

        private static Bitmap ConvolutionFilter(this Bitmap sourceBitmap, double[,] filterBlurringMatrices, double factor = 1, int bias = 0)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;

            int filterWidth = filterBlurringMatrices.GetLength(1);
            int filterHeight = filterBlurringMatrices.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += (double)(pixelBuffer[calcOffset]) * filterBlurringMatrices[filterY + filterOffset, filterX + filterOffset];

                            green += (double)(pixelBuffer[calcOffset + 1]) * filterBlurringMatrices[filterY + filterOffset, filterX + filterOffset];

                            red += (double)(pixelBuffer[calcOffset + 2]) * filterBlurringMatrices[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;

                    blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));

                    green = (green > 255 ? 255 : (green < 0 ? 0 : green));

                    red = (red > 255 ? 255 : (red < 0 ? 0 : red));

                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        private static Bitmap MedianFilter(this Bitmap sourceBitmap, int BlurringMatricesSize)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];

            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            int filterOffset = (BlurringMatricesSize - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    neighbourPixels.Clear();

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {

                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            neighbourPixels.Add(BitConverter.ToInt32(pixelBuffer, calcOffset));
                        }
                    }

                    neighbourPixels.Sort();

                    middlePixel = BitConverter.GetBytes(neighbourPixels[filterOffset]);

                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Blurs a bitmap image using a specific blurring method.
        /// </summary>
        /// <param name="sourceBitmap">The bitmap to apply the blur to.</param>
        /// <param name="blurType">The type of blur to apply.</param>
        /// <returns>A bitmap that has had the blur applied to it.</returns>
        public static Bitmap ImageBlurFilter(this Bitmap sourceBitmap, BlurType blurType)
        {
            switch (blurType)
            {
                case BlurType.Mean3x3:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.Mean3x3, 1.0 / 9.0, 0);

                case BlurType.Mean5x5:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.Mean5x5, 1.0 / 25.0, 0);

                case BlurType.Mean7x7:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.Mean7x7, 1.0 / 49.0, 0);

                case BlurType.Mean9x9:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.Mean9x9, 1.0 / 81.0, 0);

                case BlurType.GaussianBlur3x3:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.GaussianBlur3x3, 1.0 / 16.0, 0);

                case BlurType.GaussianBlur5x5:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.GaussianBlur5x5, 1.0 / 159.0, 0);

                case BlurType.MotionBlur5x5:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur5x5, 1.0 / 10.0, 0);

                case BlurType.MotionBlur5x5At45Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur5x5At45Degrees, 1.0 / 5.0, 0);

                case BlurType.MotionBlur5x5At135Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur5x5At135Degrees, 1.0 / 5.0, 0);

                case BlurType.MotionBlur7x7:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur7x7, 1.0 / 14.0, 0);

                case BlurType.MotionBlur7x7At45Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur7x7At45Degrees, 1.0 / 7.0, 0);

                case BlurType.MotionBlur7x7At135Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur7x7At135Degrees, 1.0 / 7.0, 0);

                case BlurType.MotionBlur9x9:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur9x9, 1.0 / 18.0, 0);

                case BlurType.MotionBlur9x9At45Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur9x9At45Degrees, 1.0 / 9.0, 0);

                case BlurType.MotionBlur9x9At135Degrees:

                    return sourceBitmap.ConvolutionFilter(BlurringMatrices.MotionBlur9x9At135Degrees, 1.0 / 9.0, 0);

                case BlurType.Median3x3:

                    return sourceBitmap.MedianFilter(3);

                case BlurType.Median5x5:

                    return sourceBitmap.MedianFilter(5);

                case BlurType.Median7x7:

                    return sourceBitmap.MedianFilter(7);

                case BlurType.Median9x9:

                    return sourceBitmap.MedianFilter(9);

                case BlurType.Median11x11:

                    return sourceBitmap.MedianFilter(11);

                default:

                    throw new Exception(string.Format("Unsupported blur type {0}", blurType.ToString()));
            }
        }

        /// <summary>
        /// Distorts a bitmap by randomly moving pixels by a certain factor and then smoothes it by applying a median blur filter.
        /// </summary>
        /// <param name="sourceBitmap">The bitmap to distort</param>
        /// <param name="distortFactor">The distortion factor to apply, the lower the factor, the more the image will be distorted.</param>
        /// <returns>a distorted Bitmap object.</returns>
        public static Bitmap DistortionBlurFilter(this Bitmap sourceBitmap, int distortFactor)
        {
            byte[] pixelBuffer = sourceBitmap.GetByteArray();
            byte[] resultBuffer = sourceBitmap.GetByteArray();

            int imageStride = sourceBitmap.Width * 4;
            int calcOffset = 0, filterY = 0, filterX = 0;
            int factorMax = (distortFactor + 1) * 2;
            Random rand = new Random();

            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                filterY = distortFactor - rand.Next(0, factorMax);
                filterX = distortFactor - rand.Next(0, factorMax);

                if (filterX * 4 + (k % imageStride) < imageStride && filterX * 4 + (k % imageStride) > 0)
                {
                    calcOffset = k + filterY * imageStride + 4 * filterX;

                    if (calcOffset >= 0 && calcOffset + 4 < resultBuffer.Length)
                    {
                        resultBuffer[calcOffset] = pixelBuffer[k];
                        resultBuffer[calcOffset + 1] = pixelBuffer[k + 1];
                        resultBuffer[calcOffset + 2] = pixelBuffer[k + 2];
                    }
                }
            }

            return resultBuffer.GetImage(sourceBitmap.Width, sourceBitmap.Height).MedianFilter(3);
        }
        #endregion
    }
}
