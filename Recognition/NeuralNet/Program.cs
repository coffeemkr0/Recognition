using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace neural_net_1
{
    class Program
    {
        static void Main(string[] args)
        {
            var neural_net = new Neural_Net(784, new []{100,60,30,10}, 25, 1.5, 15.0);
            var Tester = new Net_Test(neural_net);
            int best = -10000;

            string _continue = "y";
            while (_continue == "y" || _continue == string.Empty)
            {
                int c1 = 0;
                int c2 = 0;
                for (int i =0; i<40; i++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    Tester.Epoch_Train();

                    sw.Stop();
                    Console.WriteLine(sw.Elapsed.TotalSeconds);
                    sw.Reset();
                    sw.Start();
                    c1 = Tester.Validate();
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed.TotalSeconds);
                    if (c1 > c2)
                        neural_net.Shrink_Training_Speed();
                    if (c1 > best)
                        best = c1;
                    c2 = c1;
                }

                Console.WriteLine("The best run was correct in {0}% of validation cases.", (10000 + (float)best) / (float)100);

                Console.WriteLine("COntinue: y or n?");
                _continue = Console.ReadLine();
            }

            Console.WriteLine("test pass");
            Console.ReadLine();
        }
    }

    class Net_Test
    {
        #region properties

        private Digit_Image_Mc[] images = Digit_Image_Mc.LoadData();
        private int training_position = 0;
        private int validation_images = 50000;
        private Neural_Net _net;

        #endregion properties

        #region Constructors

        public Net_Test(Neural_Net net)
        {
            _net = net;
        }

        #endregion Constructors


        #region public methods

        public void Epoch_Train()
        {
            var ordering = rand_sort_array(images.Length-10000);
            for (int i = 0; i < ordering.Length; i++)
            {
                Train(ordering[i]);
            }
        }

        //public void Epoch_Train_parallel()
        //{
        //    var ordering = rand_sort_array(images.Length - 10000);
        //    Validate();
        //    for (int i = 0; i < ordering.Length; i++)
        //    {
        //        int[][] inputs = new int[Neural_Net.Mini_Batch_Size][];
        //        int[] targets = new int[Neural_Net.Mini_Batch_Size];
        //        for (int j = 0; j < inputs.Length; j++)
        //        {
        //            inputs[j] = prep_image(ordering[i]);
        //            targets[j] = images[ordering[i]].Label;
        //            i++;
        //        }
        //        _net.Accept_Parrallel_Input_For_Learning(inputs, targets);

        //        //Validate();
        //    }
        //}

        public void Train(int image_int)
        {
            var image = prep_image(image_int);
            var response = _net.Accept_Input_For_Learning(image, images[image_int].Label);

            //if (image_int%100 == 0)
            //{
            //    if (images[image_int].Label != response)
            //    {
            //        Console.WriteLine("Image was a {0}.  Net guessed it was {1}.", images[image_int].Label,
            //            response);
            //    }
            //    else if (images[image_int].Label == response)
            //    {
            //        Console.WriteLine("CORRECT. Image was a {0}.  Net guessed it was {1}.",
            //            images[image_int].Label, response);
            //    }
            //    Console.ReadLine();
            //}
        }


        public void Train()
        {
            var image = prep_image(training_position);
            var response = _net.Accept_Input_For_Learning(image, images[training_position].Label);

            //if (training_position%100 == 0)
            //{
            //    if (images[training_position].Label != response)
            //    {
            //        Console.WriteLine("Image was a {0}.  Net guessed it was {1}.", images[training_position].Label,
            //            response);
            //    }
            //    else if (images[training_position].Label == response)
            //    {
            //        Console.WriteLine("CORRECT. Image was a {0}.  Net guessed it was {1}.",
            //            images[training_position].Label, response);
            //    }
            //    Console.ReadLine();
            //}

            training_position++;
            if (training_position >= 50000)
                training_position = 0;
        }

        public void Process()
        {
            var image = prep_image(training_position);
            var response = _net.Process_Input(image);

            if (images[training_position].Label != response)
            {
                Console.WriteLine("Image was a {0}.  Net guessed it was {1}.", images[training_position].Label,
                    response);
            }
            else if (images[training_position].Label == response)
            {
                Console.WriteLine("CORRECT. Image was a {0}.  Net guessed it was {1}.",
                    images[training_position].Label, response);
            }
            Console.ReadLine();

            training_position++;
            if (training_position >= 50000)
                training_position = 0;
        }

        public int Validate()
        {
            int correct = 0;
            int correct_train = 0;
            var lock_object = new object();

            Parallel.For(0, 50000, loop_value =>
            {
                var image = prep_image(loop_value);
                var response = _net.Process_Input(image);
                if (response != Convert.ToInt32(images[loop_value].Label))
                {
                    lock (lock_object)
                    {
                        correct_train--;
                    }
                }
            });

            Parallel.For(50000, 60000, loop_value =>
            {
                var image = prep_image(loop_value);
                var response = _net.Process_Input(image);
                if (response != Convert.ToInt32(images[loop_value].Label))
                {
                    lock (lock_object)
                    {
                        correct--;
                    }
                }
            });

            Console.WriteLine("The net was correct in {0}% of training cases.", (50000+(float)correct_train)/(float)500);
            Console.WriteLine("The net was correct in {0}% of validation cases.", (10000+(float)correct)/(float)100);
            return correct;
        }

        #endregion public methods

        #region private methods

        protected int[] rand_sort_array(int count)
        {
            var result = new int[count];
            var _random = new Random();
            for (var i = 0; i < count; i++)
            {
                var j = _random.Next(0, i + 1);
                if (i != j)
                {
                    result[i] = result[j];
                }
                result[j] = i;
            }
            return result;
        }

        private int[] prep_image(int image_id)
        {
            var image = images[image_id].Pixels;
            var result=new int[784];
            int result_i = 0;
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    result[result_i] = Convert.ToInt32(image[i][j]);
                    result_i++;
                }
            }
            return result;
        }

        #endregion private methods

    }
}
