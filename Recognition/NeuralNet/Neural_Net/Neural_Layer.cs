using System;
using System.Collections.Generic;
using System.Linq;

namespace neural_net_1
{
    class Neural_Layer : Neural_Base
    {
        #region properties

        private readonly int _expected_inputs;
        private readonly Neural_Node[] _nodes;
        private Double[] _last_input;
        private Double[][] _last_inputs_parallel;
        private Double[] _output_pre_sigmoid;
        private Double[][] _output_pre_sigmoid_parallel;
        private int _mini_batch_counter;
        private int _drop_outs;
        private readonly bool _is_first_layer_after_input;
        private bool _is_output_layer;

        #endregion properties

        #region Constructors

        public Neural_Layer(int nodes, int inputs, int layer_index, bool is_output_layer)
        {
            _expected_inputs = inputs;
            _is_output_layer = is_output_layer;
            _nodes = new Neural_Node[nodes];
            for (int i = 0; i < nodes; i++)
            {
                _nodes[i] = new Neural_Node(inputs);
            }
            _drop_outs = _nodes.Length/15;
            _output_pre_sigmoid = new double[nodes];
            if (layer_index == 0)
                _is_first_layer_after_input = true;
            _last_inputs_parallel = new double[Neural_Net.Mini_Batch_Size][];
            _output_pre_sigmoid_parallel = new double[Neural_Net.Mini_Batch_Size][];
        }

        #endregion Constructors

        #region public methods

        public override void Reset_After_Mini_Batch()
        {
            _mini_batch_counter = 0;
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].Reset_After_Mini_Batch();
            }
            for (int i = 0; i < _drop_outs; i++)
            {
                _nodes[Neural_Base.Random.Next(0, _nodes.Length)].Dropped_Out = true;
            }
        }

        public double[] Process_Inputs(double[] input_array)
        {
            var output = new double[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                output[i] = _nodes[i].Process_Inputs(input_array);
            }
            return Sigmoid(output);
        }

        public double[] Process_Inputs_For_Parallel_Learning(double[] input_array, int loop_value)
        {
            if (!input_array.Length.Equals(_expected_inputs))
                throw new Exception("Input contained incorrect number of values");

            var output = new double[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                output[i] = _nodes[i].Process_Inputs_for_Parallel_Learning(input_array);
            }
            _last_inputs_parallel[loop_value] = input_array;
            _output_pre_sigmoid_parallel[loop_value] = output;
            return Sigmoid(output);
        }

        public double[] Process_Inputs_For_Learning(double[] input_array)
        {
            if (!input_array.Length.Equals(_expected_inputs))
                throw new Exception("Input contained incorrect number of values");

            var output = new double[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                output[i] = _nodes[i].Process_Inputs_for_Learning(input_array);
            }
            _last_input = input_array;
            _output_pre_sigmoid=output;
            return Sigmoid(output);
        }

        public Double[][] Get_Weight_Matrix()
        {
            var matrix = new double[_nodes.Length][];
            for (int i = 0; i < _nodes.Length; i++)
            {
                matrix[i] = _nodes[i].Weights;
            }
            return matrix;
        }

        public Double[] Get_Bias_Array()
        {
            var biases = new double[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            {
                biases[i] = _nodes[i].Bias;
            }
            return biases;
        }

        //public Double[] BackPropogate_Parrallel(Double[] gradient_array, int loop_value)
        //{
        //    //find layer's cost array
        //    var cost_array = Hadamard_Product(gradient_array, sigmoid_prime(_output_pre_sigmoid_parallel[loop_value]));

        //    //send cost array to nodes
        //    Adjust_biases_parallel(cost_array, loop_value);
        //    Adjust_weights_parallel(loop_value);

        //    double[] forward_gradient_array;
        //    if (!_is_first_layer_after_input)
        //    {
        //        forward_gradient_array = new double[_expected_inputs];
        //        for (int i = 0; i < _nodes[0].Weights.Length; i++)
        //        {
        //            for (int j = 0; j < _nodes.Length; j++)
        //            {
        //                forward_gradient_array[i] += cost_array[j] * _nodes[j].Weights[i];
        //            }
        //        }
        //    }
        //    else
        //    {
        //        forward_gradient_array = new double[0];
        //    }

        //    return forward_gradient_array;
        //}

        public Double[] BackPropogate(Double[] gradient_array)
        {
            double[] cost_array;
            //find layer's cost array
            if (!_is_output_layer)
            {
                cost_array = Hadamard_Product(gradient_array, sigmoid_prime(_output_pre_sigmoid));
            }
            else
            {
                cost_array = gradient_array;
            }

            //send cost array to nodes
            Adjust_biases(cost_array);
            Adjust_weights();

            double[] forward_gradient_array;
            if (!_is_first_layer_after_input)
            {
                forward_gradient_array = new double[_expected_inputs];
                for (int i = 0; i < _nodes[0].Weights.Length; i++)
                {
                    for (int j = 0; j < _nodes.Length; j++)
                    {
                        forward_gradient_array[i] += cost_array[j]*_nodes[j].Weights[i];
                    }
                }
            }
            else
            {
                forward_gradient_array = new double[0];
            }

            _mini_batch_counter++;
            if (_mini_batch_counter == Neural_Net.Mini_Batch_Size)
            {
                Update_Values();
                Reset_After_Mini_Batch();
            }

            return forward_gradient_array;
        }

        //public void Learn_After_Parallel()
        //{
        //    foreach (Neural_Node node in _nodes)
        //    {
        //        node.Learn_After_Parallel();
        //    }
        //    Reset_After_Mini_Batch();
        //    _last_inputs_parallel = new double[_last_inputs_parallel.Length][];
        //    _output_pre_sigmoid_parallel = new double[_output_pre_sigmoid_parallel.Length][];
        //}

        #endregion public methods

        #region private methods

        private void Adjust_biases(Double[] node_error_array)
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].Adjust_bias(node_error_array[i]);
            }
        }

        private void Adjust_weights()
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].Adjust_Weights(_last_input);
            }
        }

        //private void Adjust_biases_parallel(Double[] node_error_array, int loop_value)
        //{
        //    for (int i = 0; i < _nodes.Length; i++)
        //    {
        //        _nodes[i].Adjust_bias_parallel(node_error_array[i], loop_value);
        //    }
        //}

        //private void Adjust_weights_parallel(int loop_value)
        //{
        //    for (int i = 0; i < _nodes.Length; i++)
        //    {
        //        _nodes[i].Adjust_Weights_parallel(_last_inputs_parallel[loop_value], loop_value);
        //    }
        //}

        private void Update_Values()
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].Update_Values();
            }
        }

        #endregion private methods
    }
}
