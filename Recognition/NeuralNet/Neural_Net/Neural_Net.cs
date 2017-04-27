using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace neural_net_1
{
    class Neural_Net : Neural_Base
    {

        #region properties

        //does not include input layer
        private readonly int _layers_count;
        private readonly Neural_Layer[] _layers;

        private static Double _training_speed;
        public static Double Training_Speed
        {
            get { return _training_speed; }
            set { _training_speed = value; }
        }

        private static int _mini_batch_size;
        public static int Mini_Batch_Size
        {
            get { return _mini_batch_size; }
            set { _mini_batch_size = value; }
        }

        private static Double _normalization_value;
        public static Double Normalization_Value
        {
            get { return _normalization_value; }
            set { _normalization_value = value; }
        }
        
        #endregion properties

        #region constructors

        public Neural_Net(int input_count, int[] layers_after_first, int mini_batch_size, double training_speed, double normalization_value)
        {
            _mini_batch_size = mini_batch_size;
            _training_speed = training_speed;
            _normalization_value = normalization_value;
            _layers_count = layers_after_first.Length;
            _layers = new Neural_Layer[_layers_count];
            for (int i = 0; i < _layers_count; i++)
            {
                if (i==0)
                {
                    _layers[i]=new Neural_Layer(layers_after_first[i], input_count, i, false);
                }
                else if (i.Equals(layers_after_first.Length - 1))
                {
                    _layers[i] = new Neural_Layer(layers_after_first[i], layers_after_first[i-1], i, true);
                }
                else
                {
                    _layers[i] = new Neural_Layer(layers_after_first[i], layers_after_first[i-1], i, false);
                }
            }
        }

        #endregion constructors

        #region public methods

        public int Process_Input(int[] inputs)
        {
            //takes imputs and sets to values on (0,1)
            var input_array = new double[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                input_array[i] = Input_Shrink(inputs[i]);
            }

            //passes inputs to first layer and then each layer's outputs to next layer
            for (int i = 0; i < _layers_count; i++)
            {
                input_array = _layers[i].Process_Inputs(input_array);
            }

            //compairs outputs of last layer to find largest.
            //input_array now corrisponds to the output
            int decision = 0;
            for (int i = 1; i < input_array.Length; i++)
            {
                if (input_array[i] > input_array[decision])
                {
                    decision = i;
                }
            }

            //returns index of highest output neuron
            return decision;
        }

        //public void Accept_Parrallel_Input_For_Learning(int[][] inputs, int[] target)
        //{
        //    Parallel.For(0, inputs.Length, loop_value =>
        //    {
        //        //takes imputs and sets to values on (0,1)
        //        var input_array = new double[inputs[loop_value].Length];
        //        for (int i = 0; i < inputs.Length; i++)
        //        {
        //            input_array[i] = Input_Shrink(inputs[loop_value][i]);
        //        }

        //        //passes inputs to first layer and then each layer's outputs to next layer
        //        for (int i = 0; i < _layers_count; i++)
        //        {
        //            input_array = _layers[i].Process_Inputs_For_Parallel_Learning(input_array, loop_value);
        //        }

        //        //create error array
        //        input_array[target[loop_value]] -= 1.0;

        //        BackPropogate_parallel(input_array,loop_value);
        //    });
        //    foreach (Neural_Layer layer in _layers)
        //    {
        //        layer.Learn_After_Parallel();
        //    }
        //}

        public int Accept_Input_For_Learning(int[] inputs, int target)
        {
            //takes imputs and sets to values on (0,1)
            var input_array = new double[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                input_array[i] = Input_Shrink(inputs[i]);
            }

            //passes inputs to first layer and then each layer's outputs to next layer
            for (int i=0; i < _layers_count; i++)
            {
                input_array=_layers[i].Process_Inputs_For_Learning(input_array);
            }

            //compairs outputs of last layer to find largest.
            //input_array now corrisponds to the output
            int decision = 0;
            for (int i = 1; i < input_array.Length; i++)
            {
                if (input_array[i] > input_array[decision])
                {
                    decision = i;
                }
            }

            //create error array
            input_array[target] -= 1.0;

            BackPropogate(input_array);

            //returns index of highest output neuron
            return decision;
        }

        public override void Reset_After_Mini_Batch()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i].Reset_After_Mini_Batch();
            }
        }

        public void Shrink_Training_Speed()
        {
            Training_Speed = Training_Speed*0.8;
        }

        public static Neural_Net FromXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Neural_Net));
            Neural_Net obj;

            using (var str_reader = new StringReader(xml))
            using (var s = XmlReader.Create(str_reader))
            {
                obj = (Neural_Net)serializer.Deserialize(s);
            }

            return obj;
        }

        public string ToXml()
        {
            var empty_namepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(GetType());
            var settings = new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true };
            var xml = string.Empty;

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, this, empty_namepsaces);
                xml = stream.ToString();
            }

            return xml;
        }

        #endregion public methods

        #region private methods

        private void BackPropogate(double[] error_array)
        {
            double[] costs = _layers[_layers.Length - 1].BackPropogate(error_array);

            for (int i = 1; i < _layers_count; i++)
            {
                costs = _layers[_layers.Length - 1 - i].BackPropogate(costs);
            }
        }

        //private void BackPropogate_parallel(double[] error_array, int loop_value)
        //{
        //    double[] costs = _layers[_layers.Length - 1].BackPropogate_Parrallel(error_array, loop_value);

        //    for (int i = 1; i < _layers_count; i++)
        //    {
        //        costs = _layers[_layers.Length - 1 - i].BackPropogate_Parrallel(costs, loop_value);
        //    }
        //}

        private Double[] Create_Answer_Array(int answer, int possibilities)
        {
            var array = new Double[possibilities];
            array[answer] = 1;
            return array;
        }

        #endregion private methods
    }
}
