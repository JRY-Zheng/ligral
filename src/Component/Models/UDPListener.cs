/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;
using Ligral.Tools;

namespace Ligral.Component.Models
{
    class UDPListener : OutPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model listens udp data as input";
            }
        }
        private List<(int, int)> size;
        private List<string> names;
        private bool useDefinedName = false;
        private List<ControlInputHandle> handles;
        private List<Matrix<double>> inputs;
        private string address = null;
        private int port = 0;
        private Subscriber subscriber;
        private PacketLabel packetLabel;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"size", new Parameter(ParameterType.Signal , value=>
                {
                    size = new List<(int, int)>();
                    Matrix<double> sizeMat;
                    try
                    {
                        sizeMat = value.ToMatrix();
                        if (sizeMat.ColumnCount != 2)
                        {
                            throw logger.Error(new ModelException(this, $"size must be a nx2 int matrix, but column number is {sizeMat.ColumnCount}"));
                        }
                        foreach (var row in sizeMat.ToRowArrays())
                        {
                            size.Add((row[0].ToInt(), row[1].ToInt()));
                        }
                    }
                    catch (System.ArgumentException e)
                    {
                        throw logger.Error(new ModelException(this, $"size must be a nx2 int matrix, but {e.Message}"));
                    }
                }, ()=>{})},
                {"name", new Parameter(ParameterType.String , value=>
                {
                    names = new List<string>(value.ToString().Split(";"));
                    names = names.ConvertAll(s => s.Trim());
                    useDefinedName = true;
                }, ()=>{})},
                {"label", new Parameter(ParameterType.String , value=>
                {
                    packetLabel = new PacketLabel();
                    packetLabel.Label = int.Parse(value.ToString(), System.Globalization.NumberStyles.HexNumber);
                }, ()=>
                {
                    packetLabel = new PacketLabel();
                    packetLabel.Label = 0xffb0;
                })},
                {"address", new Parameter(ParameterType.String , value=>
                {
                    address = value.ToString();
                }, ()=>
                {
                    address = Settings.GetInstance().IPAddress;
                })},
                {"port", new Parameter(ParameterType.Signal , value=>
                {
                    port = value.ToInt();
                }, ()=>
                {
                    port = Settings.GetInstance().ListeningPort;
                })}
            };
        }
        public override void Check()
        {
            handles = new List<ControlInputHandle>();
            if (names is null)
            {
                names = OutPortsName.ConvertAll(n => Name+"."+n);
            }
            else if (names.Count != OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"cannot match name info {names.Count} to out port count {OutPortCount()}"));
            }
            if (size is null)
            {
                for (int i=0; i<OutPortCount(); i++)
                {
                    var handle = ControlInput.CreateInput(names[i], 1, 1);
                    handles.Add(handle);
                    OutPortList[i].SetShape(1, 1);
                }
            }
            else if (size.Count != OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"cannot match size info {size.Count} to out port count {OutPortCount()}"));
            }
            else
            {
                for (int i=0; i<size.Count; i++)
                {
                    var handle = ControlInput.CreateInput(names[i], size[i].Item1, size[i].Item2);
                    handles.Add(handle);
                    OutPortList[i].SetShape(size[i].Item1, size[i].Item2);
                }
            }
            inputs = handles.ConvertAll(h => h.GetInput());
        }
        public override void Prepare()
        {
            subscriber = new Subscriber(address, port);
            subscriber.AddAbility(packetLabel, packetString => 
            {
                JsonDict dict;
                try
                {
                    dict = JsonConvert.Load(packetString);
                }
                catch (System.ArgumentException e)
                {
                    throw logger.Error(new ModelException(this, $"Json text syntax error: {e.Message}"));
                }
                if (!dict.Value.ContainsKey("data"))
                {
                    throw logger.Error(new ModelException(this, "Invalid message: No data"));
                }
                var data = dict.Value["data"] as JsonDict;
                if (data is null)
                {
                    throw logger.Error(new ModelException(this, "Invalid message: Data is not dict"));
                }
                var dataName = useDefinedName ? names : OutPortsName;
                if (dataName.Any(n => !data.Value.ContainsKey(n)))
                {
                    throw logger.Error(new ModelException(this, "Invalid message: Key lost in data"));
                }
                for (int i=0; i<OutPortCount(); i++)
                {
                    try
                    {
                        inputs[i] = data.Value[dataName[i]].ToMatrix();
                    }
                    catch (System.ArgumentException e)
                    {
                        throw logger.Error(new ModelException(this, $"Invalid message: invalid data {dataName[i]}, {e.Message}"));
                    }
                }
            });
            Solver.Exited += subscriber.Unsubscribe;
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            for (int i=0; i<OutPortCount(); i++)
            {
                try
                {
                    handles[i].SetClosedLoopInput(inputs[i]);
                }
                catch (LigralException)
                {
                    throw logger.Error(new ModelException(this, $"Error occurred in output {i}"));
                }
                Results[i] = handles[i].GetInput();
            }
            return Results;
        }
    }
}