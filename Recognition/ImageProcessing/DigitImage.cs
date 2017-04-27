using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    /// <summary>
    /// Represents an image of a single numerical digit that is 28X28 pixels.
    /// </summary>
    public class DigitImage
    {
        #region Properties
        /// <summary>
        /// Gets or sets the pixels that belong to the image
        /// </summary>
        public byte[][] Pixels { get; set; }

        /// <summary>
        /// Gets or sets the label for the image, or more accurately what digit the image represents i.e. 0,1,2,3 etc.
        /// </summary>
        public byte Label { get; set; }

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        public int Width
        {
            get { return this.Pixels.GetLength(0); }
        }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        public int Height
        {
            get { return this.Pixels.GetLength(1); }
        }
        #endregion
        
        #region Constructors
        public DigitImage()
        {
            //Initialize the two dimentional pixel array to represent a 28X28 pixel image.
            this.Pixels = new byte[28][];
            for (int i = 0; i < this.Pixels.Length; i++)
            {
                this.Pixels[i] = new byte[28];
            }
        }

        public DigitImage(Bitmap bitmap, byte label)
            : this()
        {
            this.Label = label;

            //Assert that the bitmap is a 28X28 image
            if (bitmap.Width != 28 || bitmap.Height != 28) throw new Exception("Digit images must be 28X28 pixels.");

            //Iterate each pixel in the bitmap and assign the correct corresponding gray value to the correct pixel in the DigitImage
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    int grayScale = (int)((pixelColor.R * .3) + (pixelColor.G * .59) + (pixelColor.B * .11));
                    this.Pixels[x][y] = Convert.ToByte(255 - grayScale);
                }
            }

            bitmap.Dispose();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Generates a managed System.Drawing.Image object from the DigitImage
        /// </summary>
        /// <returns>a grayscale Image object</returns>
        public Image ToImage()
        {
            Image image = new Bitmap(28, 28);
            using (Graphics g = Graphics.FromImage(image))
            {
                //Iterate each row of pixels in the image
                for (int y = 0; y < 28; y++)
                {
                    //Iterate each column of pixels in the image
                    for (int x = 0; x < 28; x++)
                    {
                        //Fill the pixel with the color assuming that the value of the byte represents a gray color from 0 to 255
                        int grayValue = 255 - this.Pixels[x][y];
                        using(Brush solidBrush = new SolidBrush(Color.FromArgb(grayValue,grayValue,grayValue)))
                        {
                            g.FillRectangle(solidBrush, new Rectangle(x, y, 1, 1));
                        }
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Generates a managed System.Drawing.Bitmap object from the DigitImage
        /// </summary>
        /// <returns>a grayscale Bitmap</returns>
        public Bitmap ToBitmap()
        {
            return (Bitmap)this.ToImage();
        }

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="fileName">The path and filename to save the image to, the extension will control the image format.</param>
        public void SaveToFile(string fileName)
        {
            Image image = this.ToImage();
            this.ToImage().Save(fileName);
            image.Dispose();
        }
        #endregion
    }
}
