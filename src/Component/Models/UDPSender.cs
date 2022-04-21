/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    class UDPSender : InPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model sends udp data from output";
            }
        }
        private List<string> names;
        private bool useDefinedName = false;
        private List<ObservationHandle> handles;
        private Dictionary<string, JsonObject> data;
        private Publisher publisher;
        private PacketLabel packetLabel;
        private bool simpleFormat = true;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
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
                {"format", new Parameter(ParameterType.String , value=>
                {
                    switch (value.ToString().ToLower())
                    {
                    case "simple":
                        simpleFormat = true;
                        break;
                    case "full":
                        simpleFormat = false;
                        break;
                    default:
                        throw new System.ArgumentException($"Unknown value {value} for format, simple or full expected.");
                    }
                }, ()=>{})}
            };
        }
        public override void Check()
        {
            handles = new List<ObservationHandle>();
            if (!useDefinedName)
            {
                names = InPortsName;
            }
            else if (names.Count != InPortCount())
            {
                throw logger.Error(new ModelException(this, $"cannot match name info {names.Count} to in port count {InPortCount()}"));
            }
            for (int i=0; i<InPortCount(); i++)
            {
                var handle = Observation.CreateObservation(names[i], InPortList[i].RowNo, InPortList[i].ColNo);
                handles.Add(handle);
            }
        }
        public override void Prepare()
        {
            Settings settings = Settings.GetInstance();
            if (!settings.RealTimeSimulation)
            {
                logger.Warn("UDP sender will not work in non-realtime simulation");
            }
            publisher = new Publisher();
            data = new Dictionary<string, JsonObject>();
        }
        public override void Refresh()
        {
            Settings settings = Settings.GetInstance();
            if (!settings.RealTimeSimulation) return;
            for (int i=0; i<InPortCount(); i++)
            {
                var mat = handles[i].GetObservation();
                var enumerableNumbers = mat.EnumerateRows().Select((vector, _) => vector.Enumerate().Select((item, _) => new JsonNumber() {Value = item}));
                var list = enumerableNumbers.Select((row, _) => JsonList.FromList(row));
                var json = JsonList.FromList(list);
                if (simpleFormat && json.Value.Count == 1)
                {
                    var vec = (JsonList) json.Value[0];
                    if (vec.Value.Count == 1)
                    {
                        data[names[i]] = vec.Value[0];
                    }
                    else
                    {
                        data[names[i]] = vec;
                    }
                }
                else
                {
                    data[names[i]] = json;
                }
            }
            publisher.Send(packetLabel.Label, data);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            for (int i=0; i<InPortCount(); i++)
            {
                try
                {
                    handles[i].Cache(values[i]);
                }
                catch (LigralException)
                {
                    throw logger.Error(new ModelException(this, $"Error occurred in output {i}"));
                }
            }
            return Results;
        }
    }
}