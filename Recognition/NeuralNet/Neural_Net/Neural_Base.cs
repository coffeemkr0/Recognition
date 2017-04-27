using System;

namespace neural_net_1
{
    class Neural_Base
    {
        #region private members
        protected static Random Random = new Random();
        #endregion private members


        #region public methods

        public virtual void Reset_After_Mini_Batch()
        {

        }

        #endregion public methods

        #region private methods

        protected double[] sigmoid_prime(double[] layer_outputs)
        {
            var response = new double[layer_outputs.Length];

            for (int i = 0; i < layer_outputs.Length; i++)
            {
                response[i] = sigmoid_prime(layer_outputs[i]);
            }

            return response;
        }

        protected double sigmoid_prime(double output)
        {
            return (Sigmoid(output))*(1 - Sigmoid(output));

            //logistic
            //return -1/(Math.Exp(output) + Math.Exp(-output) - 2);
        }

        protected double[] Sigmoid(double[] array)
        {
            var response = new double[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                response[i] = Sigmoid(array[i]);
            }

            return response;
        }

        protected double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));

            //rectafied linear
            //if(x<0) return 0;
            //else return x;

            //other sigmoids
            if (x < -45.0)
            {
                return 0.0;
            }
            else if (x > 45.0)
            {
                return 1.0;
            }
            else
            {
                //logistic sigmoid
                return 1.0/(1.0 + Math.Exp(-x));
                //hyperbolic tangent sigmoid
                //return ((1 + Math.Tanh(x/2))/2);
            }
        }

        protected double Input_Shrink(int input)
        {
            return ((double) input/256.0);
        }

        protected int[] rand_sort_array(int count)
        {
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                var j = Random.Next(0, i + 1);
                if (i != j)
                {
                    result[i] = result[j];
                }
                result[j] = i;
            }
            return result;
        }

        protected Double[] array_of_means(Double[][] input)
        {
            if (input.Length.Equals(0))
                throw new Exception("Attempt to devide by 0.  Improper array length.");

            Double[] output = new double[input[0].Length];
            for (int i = 0; i < output.Length; i++)
            {
                double n = 0;
                for (int x = 0; x < input.Length; x++)
                {
                    n += input[x][i];
                }
                output[i] = n / input.Length;
            }
            return output;
        }

        protected Double[] Hadamard_Product(Double[] array1, Double[] array2)
        {
            if (!array2.Length.Equals(array1.Length))
                throw new Exception("Hadamard requires equal length arrays");

            var result = new double[array1.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array1[i]*array2[i];
            }
            return result;
        }

        protected double[][] Get_Random_Subset(int subset_size, double[][] array)
        {
            var result = new double[subset_size][];
            for (int i = 0; i < subset_size; i++)
            {
                result[i] = array[Random.Next(0, array.Length - 1)];
            }
            return result;
        }

        #endregion private methods
    }
}
