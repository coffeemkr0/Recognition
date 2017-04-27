using System.IO;

namespace neural_net_1
{
    public class Digit_Image_Mc
    {
        // an MNIST image of a '0' thru '9' digit
        public int Width; // 28
        public int Height; // 28
        public byte[][] Pixels; // 0(white) - 255(black)
        public byte Label; // '0' - '9'

        public Digit_Image_Mc(int width, int height, byte[][] pixels, byte label)
        {
            Width = width; Height = height;
            Pixels = new byte[height][];
            for (int i = 0; i < Pixels.Length; ++i)
                Pixels[i] = new byte[width];

            for (int i = 0; i < height; ++i)
                for (int j = 0; j < width; ++j)
                    Pixels[i][j] = pixels[i][j];

            Label = label;
        }

        public static Digit_Image_Mc[] LoadData()
        {
            // Load MNIST training set of 60,000 images into memory
            int num_images = 60000; // ugly
            Digit_Image_Mc[] result = new Digit_Image_Mc[num_images];

            byte[][] pixels = new byte[28][];
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = new byte[28];

            FileStream ifs_pixels = new FileStream(@"Images.dat", FileMode.Open);
            FileStream ifs_labels = new FileStream(@"Labels.dat", FileMode.Open);

            BinaryReader br_images = new BinaryReader(ifs_pixels);
            BinaryReader br_labels = new BinaryReader(ifs_labels);

            int magic1 = br_images.ReadInt32(); // read and discard
            int image_count = br_images.ReadInt32();
            int num_rows = br_images.ReadInt32();
            int num_cols = br_images.ReadInt32();

            int magic2 = br_labels.ReadInt32();
            int num_labels = br_labels.ReadInt32();

            // each image
            for (int di = 0; di < num_images; di++)
            {
                for (int i = 0; i < 28; i++) // get 28x28 pixel values
                {
                    for (int j = 0; j < 28; j++)
                    {
                        byte b = br_images.ReadByte();
                        pixels[i][j] = b;
                    }
                }

                byte lbl = br_labels.ReadByte(); // get the label
                Digit_Image_Mc d_image = new Digit_Image_Mc(28, 28, pixels, lbl);
                result[di] = d_image;
            } // each image

            ifs_pixels.Close();
            br_images.Close();
            ifs_labels.Close();
            br_labels.Close();

            return result;
        } // LoadData
    }
}
