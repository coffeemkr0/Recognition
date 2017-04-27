using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    /// <summary>
    /// Represents a collection of numeric digit images.
    /// </summary>
    public class DigitImageCollection
    {
        #region Properties
        /// <summary>
        /// Gets or sets the digit images in the collection
        /// </summary>
        public DigitImage[] DigitImages { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a DigitImageCollection from image and label files that are in the Mnist dataset format
        /// </summary>
        /// <param name="imageFile">A path to the file that contains the image data.</param>
        /// <param name="labelFile">A path to thefile that contains labels for each image.</param>
        /// <returns>An instance of DigitImageColleciton with all images and labels loaded from the files.</returns>
        public static DigitImageCollection LoadMnistDataSet(string imageFile, string labelFile, int imageCount)
        {
            //Use this link for information about the file formats of the Mnist dataset - http://yann.lecun.com/exdb/mnist/

            DigitImageCollection imageCollection = new DigitImageCollection();

            //Create file streams and binary readers for the images and labels.
            using(FileStream imagesFileStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader imagesReader = new BinaryReader(imagesFileStream))
                {
                    using (FileStream labelsFileStream = new FileStream(labelFile, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader labelsReader = new BinaryReader(labelsFileStream))
                        {
                            //The image file header should be in the following format
                            //[offset] [type]          [value]          [description] 
                            //0000     32 bit integer  0x00000803(2051) magic number 
                            //0004     32 bit integer  60000            number of images 
                            //0008     32 bit integer  28               number of rows 
                            //0012     32 bit integer  28               number of columns

                            //Read and discard the "magic number" (not sure why we don't care about this)
                            imagesReader.ReadInt32();

                            //Get the number of images in the dataset and initialize the image collections DigitImage array
                            int numberOfImages = imageCount;   //I do not know why reading the number of images does not work, but it is way off
                            imagesReader.ReadInt32();
                            imageCollection.DigitImages = new DigitImage[numberOfImages];

                            //Read and discard the number of rows and columns (not sure why we don't care about these)
                            imagesReader.ReadInt32();
                            imagesReader.ReadInt32();

                            //The label file header is similar, it has a magic number and the number of labels but no need for the rows or columns counts
                            //Read and discard the magic number
                            labelsReader.ReadInt32();

                            //Get the number of labels
                            int numberOfLabels = imageCount;  //I do not know why reading the number of labels does not work, but it is way off
                            labelsReader.ReadInt32();

                            //Make sure the number of labels and images match
                            if (numberOfImages != numberOfLabels) throw new Exception("The number of images does not match the number of labels.");

                            //Start reading pixels and creating 28X28 DigitImages
                            for (int imageIndex = 0; imageIndex < numberOfImages; imageIndex++)
                            {
                                DigitImage digitImage = new DigitImage();
                                imageCollection.DigitImages[imageIndex] = digitImage;
                                for (int y = 0; y < 28; y++)
                                {
                                    for (int x = 0; x < 28; x++)
                                    {
                                        digitImage.Pixels[x][y] = imagesReader.ReadByte();
                                    }
                                }

                                //Get the label for the image
                                digitImage.Label = labelsReader.ReadByte();
                            }
                        }
                    }
                }
            }

            return imageCollection;
        }

        /// <summary>
        /// Saves the DigitImageCollection to an image and label file that matches the format of the Mnist dataset files.
        /// </summary>
        public void SaveToFile(string imagesFile, string labelsFile)
        {
            //Create file streams and binary writers for the images and labels.
            using (FileStream imagesFileStream = new FileStream(imagesFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (BinaryWriter imagesWriter = new BinaryWriter(imagesFileStream))
                {
                    using (FileStream labelsFileStream = new FileStream(labelsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (BinaryWriter labelsWriter = new BinaryWriter(labelsFileStream))
                        {
                            //The image file header should be in the following format
                            //[offset] [type]          [value]          [description] 
                            //0000     32 bit integer  0x00000803(2051) magic number 
                            //0004     32 bit integer  60000            number of images 
                            //0008     32 bit integer  28               number of rows 
                            //0012     32 bit integer  28               number of columns

                            //Write the "magic number" (not sure why we don't care about this)
                            imagesWriter.Write(0);

                            //Set the number of images in the collection
                            imagesWriter.Write(this.DigitImages.Length);

                            //Write the number of rows and columns (not sure why we don't care about these and I am not sure what they are supposed to correspond to)
                            imagesWriter.Write(0);
                            imagesWriter.Write(0);

                            //The label file header is similar, it has a magic number and the number of labels but no need for the rows or columns counts
                            //Writethe magic number
                            labelsWriter.Write(0);

                            //Set the number of labels
                            labelsWriter.Write(this.DigitImages.Length);

                            //Start writing pixel data
                            for (int imageIndex = 0; imageIndex < this.DigitImages.Length; imageIndex++)
                            {
                                DigitImage digitImage = this.DigitImages[imageIndex];

                                for (int y = 0; y < 28; y++)
                                {
                                    for (int x = 0; x < 28; x++)
                                    {
                                        imagesWriter.Write(digitImage.Pixels[x][y]);
                                    }
                                }

                                //Write the label for the image
                                labelsWriter.Write(digitImage.Label);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves all of the images to jpg format in a folder.  Sub folders are created for each digit, i.e 0, 1, 2 etc.
        /// </summary>
        /// <param name="folder"></param>
        public void SaveImagesToFolder(string folder)
        {
            //Cleanup and re-create the folder first
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            int[] imageCounts = new int[10];

            foreach(DigitImage digitImage in this.DigitImages)
            {
                if (!Directory.Exists(Path.Combine(folder, digitImage.Label.ToString())))
                {
                    Directory.CreateDirectory(Path.Combine(folder, digitImage.Label.ToString()));
                }

                int labelIndex = Convert.ToInt32(digitImage.Label);
                digitImage.SaveToFile(Path.Combine(folder, digitImage.Label.ToString(), imageCounts[labelIndex].ToString() + ".jpg"));
                imageCounts[labelIndex]++;
            }
        }
        #endregion
    }
}
