using System;
using System.Threading.Tasks;

namespace neural_net_1
{
    class Neural_Node: Neural_Base
    {
        #region properties

        private double _error;
        private double _bias_change;
        private double[] _weights_change;

        private double[] _weights;
        public double[] Weights 
        { 
            get { return _weights; }
            set { _weights = value; } 
        }

        private double _bias;
        public double Bias
        {
            get { return _bias; }
            set { _bias = value; }
        }

        private bool _dropped_out = false;
        public bool Dropped_Out
        {
            get { return _dropped_out; }
            set { _dropped_out = value; }
        }

        #endregion properties
        
        #region Constructors

        public Neural_Node(int expected_inputs)
        {
            //_bias_change_parallel = new double[Neural_Net.Mini_Batch_Size];
            //_weights_change_parallel = new double[expected_inputs][];
            _weights = new double[expected_inputs];
            _weights_change = new double[expected_inputs];
            for (int i = 0; i < expected_inputs; i++)
            {
                _weights[i] = Random.NextDouble()*2.0 - 1;
                //_weights_change_parallel[i] = new double[expected_inputs];

            }
            _bias = (Random.NextDouble() * 2.0 - 1.0);
        }

        public Neural_Node(int expected_inputs, double bias, double[] weights)
        {
            _bias = bias;
            _weights = weights;
        }

        #endregion Constructors

        #region public methods

        public double Process_Inputs(double[] input_array)
        {
            double i = 0;
            for (int n = 0; n < _weights.Length; n++)
            {
                i += _weights[n]*input_array[n];
            }
            i -= _bias;
            return i;
        }

        public double Process_Inputs_for_Parallel_Learning(double[] input_array)
        {
            double i = 0;
            for (int n = 0; n < _weights.Length; n++)
            {
                i += _weights[n] * input_array[n];
            }
            i -= _bias;
            return i;
        }

        public double Process_Inputs_for_Learning(double[] input_array)
        {
            double i = 0;
            for (int n = 0; n < _weights.Length; n++)
            {
                i += _weights[n] * input_array[n];
            }
            i -= _bias;
            return i;
        }

        public void Adjust_bias(Double node_error)
        {
            if (node_error.Equals(0) || _dropped_out)
            {
                _error = 0;
                return;
            }

            _bias_change += node_error;
            if (_bias_change.Equals(double.NaN))
            {
                throw new Exception("NaN detected.");
            }
            _error = node_error;
        }

        public void Adjust_Weights(double[] last_input)
        {
            if (!_error.Equals(0.0))
            {
                for (int i = 0; i < _weights.Length; i++)
                {
                    _weights_change[i] += last_input[i] * _error;
                }
            }
        }


        //public void Adjust_bias_parallel(double node_error, int loop_value)
        //{
        //    _bias_change_parallel[loop_value] = node_error;
        //}

        //public void Adjust_Weights_parallel(double[] last_input, int loop_value)
        //{
        //    if (!_bias_change_parallel[loop_value].Equals(0.0))
        //    {
        //        for (int i = 0; i < _weights.Length; i++)
        //        {
        //            _weights_change_parallel[loop_value][i] = last_input[i] * _bias_change_parallel[loop_value];
        //        }
        //    }
        //}

        public void Update_Values()
        {
            _bias -= (Neural_Net.Training_Speed / Neural_Net.Mini_Batch_Size) * _bias_change;
            if (Neural_Net.Normalization_Value.Equals(0.0))
            {
                for (int i = 0; i < _weights.Length; i++)
                {
                    _weights[i] -= (Neural_Net.Training_Speed / Neural_Net.Mini_Batch_Size) * _weights_change[i];
                }
            }
            else
            {
                for (int i = 0; i < _weights.Length; i++)
                {
                    _weights[i] = (1.0 - (Neural_Net.Training_Speed * Neural_Net.Normalization_Value / 50000.0)) * _weights[i] -
                                  (Neural_Net.Training_Speed / Neural_Net.Mini_Batch_Size) * _weights_change[i];
                }
            }
            Reset_After_Mini_Batch();
        }

        public override void Reset_After_Mini_Batch()
        {
            _bias_change = 0;
            _weights_change = new double[_weights.Length];
            _dropped_out = false;
        }

        //public void Learn_After_Parallel()
        //{
        //    foreach (double d in _bias_change_parallel)
        //    {
        //        _bias_change += d;
        //    }

        //    foreach (double[] w in _weights_change_parallel)
        //    {
        //        for (int i = 0; i < w.Length; i++)
        //            _weights_change[i] += w[i];
        //    }

        //    _bias_change_parallel = new double[_bias_change_parallel.Length];
        //    _weights_change_parallel = new double[_weights_change_parallel.Length][];
        //    for (int i = 0; i < _expected_inputs; i++)
        //    {
        //        _weights_change_parallel[i] = new double[_expected_inputs];
        //    }
        //    Update_Values();
        //}

        #endregion public methods

        #region private methods



        #endregion private methods

    }
}
