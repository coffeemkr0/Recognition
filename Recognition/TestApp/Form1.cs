using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageProcessing;
using ImageProcessing.Distortion.Blurring;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Save training images to disk so we can view them
            ImageProcessing.DigitImageCollection images = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TrainingImages.dat", "TrainingLabels.dat", 60000);

            images.SaveImagesToFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrainingImages"));
            MessageBox.Show("Done");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Save test images to disk so we can view them
            ImageProcessing.DigitImageCollection images = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TestImages.dat", "TestLabels.dat", 10000);

            images.SaveImagesToFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestImages"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Load the training images and blur all of the zeroes, then save them to disk for viewing
            ImageProcessing.DigitImageCollection sourceImages = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TrainingImages.dat", "TrainingLabels.dat", 60000);

            int imageCount = 0;
            string imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeroesBlurred");
            if (Directory.Exists(imageFolder))
            {
                Directory.Delete(imageFolder, true);
            }
            Directory.CreateDirectory(imageFolder);
            foreach (ImageProcessing.DigitImage image in sourceImages.DigitImages.Where(i => i.Label == 0))
            {
                Bitmap blurredImage = image.ToBitmap().ImageBlurFilter(BlurringBitmapExtensions.BlurType.GaussianBlur5x5);
                blurredImage.Save(Path.Combine(imageFolder, imageCount.ToString() + ".jpg"));
                imageCount += 1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Load the training images and distort all of the zeroes, then save them to disk for viewing
            ImageProcessing.DigitImageCollection sourceImages = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TrainingImages.dat", "TrainingLabels.dat", 60000);

            int imageCount = 0;
            string imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ZeroesDistorted");
            if (Directory.Exists(imageFolder))
            {
                Directory.Delete(imageFolder, true);
            }
            Directory.CreateDirectory(imageFolder);
            foreach (ImageProcessing.DigitImage image in sourceImages.DigitImages.Where(i => i.Label == 0))
            {
                Bitmap distortedImage = image.ToBitmap().DistortionBlurFilter(25);
                distortedImage.Save(Path.Combine(imageFolder, imageCount.ToString() + ".jpg"));
                imageCount += 1;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Test loading a MNIST image, distorting it, blurring it, converting it back to a DigitImage and then saving it to disk
            ImageProcessing.DigitImageCollection sourceImages = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TrainingImages.dat", "TrainingLabels.dat", 60000);

            string imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ConvertedImages");
            if (Directory.Exists(imageFolder))
            {
                Directory.Delete(imageFolder, true);
            }
            Directory.CreateDirectory(imageFolder);

            DigitImage originalDigitImage = sourceImages.DigitImages[0];
            originalDigitImage.SaveToFile(Path.Combine(imageFolder, "Original.jpg"));

            Bitmap distortedImage = sourceImages.DigitImages[0].ToBitmap().DistortionBlurFilter(25);
            DigitImage distortedDigitImage = new DigitImage(distortedImage, sourceImages.DigitImages[0].Label);
            distortedDigitImage.SaveToFile(Path.Combine(imageFolder, "Distorted.jpg"));

            Bitmap blurredImage = sourceImages.DigitImages[0].ToBitmap().ImageBlurFilter(BlurringBitmapExtensions.BlurType.GaussianBlur5x5);
            DigitImage blurredDigitImage = new DigitImage(blurredImage, sourceImages.DigitImages[0].Label);
            blurredDigitImage.SaveToFile(Path.Combine(imageFolder, "Blurred.jpg"));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Load the original Mnist training images and create a new blurred data set from them
            ImageProcessing.DigitImageCollection sourceImages = ImageProcessing.DigitImageCollection.LoadMnistDataSet("TrainingImages.dat", "TrainingLabels.dat", 60000);

            DigitImageCollection blurredImages = new DigitImageCollection();
            blurredImages.DigitImages = new DigitImage[sourceImages.DigitImages.Length];

            Parallel.For(0, sourceImages.DigitImages.Length, i =>
            {
                int imageIndex = i;
                DigitImage blurredDigitImage = new DigitImage(sourceImages.DigitImages[imageIndex].ToBitmap().ImageBlurFilter(BlurringBitmapExtensions.BlurType.GaussianBlur5x5), sourceImages.DigitImages[imageIndex].Label);
                blurredImages.DigitImages[imageIndex] = blurredDigitImage;
            });

            string imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BlurredTrainingDataset");
            if (Directory.Exists(imageFolder))
            {
                Directory.Delete(imageFolder, true);
            }
            Directory.CreateDirectory(imageFolder);

            blurredImages.SaveToFile(Path.Combine(imageFolder, "BlurredTrainingImages.dat"), Path.Combine(imageFolder, "BlurredTrainingLabels.dat"));
            MessageBox.Show("Done");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Load the blurred data set and save the images to a folder for viewing
            ImageProcessing.DigitImageCollection sourceImages = ImageProcessing.DigitImageCollection.LoadMnistDataSet("BlurredTrainingImages.dat", "BlurredTrainingLabels.dat", 60000);
            sourceImages.SaveImagesToFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BlurredTrainingImages"));
            MessageBox.Show("Done");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //Test OpenCV
            string imageFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Test.jpg");
            ImageProcessing.Segmentation.TextSegmentation.Test(imageFile);
        }
    }
}
