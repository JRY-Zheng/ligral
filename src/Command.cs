/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral
{
    abstract class Command
    {}
    class Linearization : Command
    {
        public string FileName;
        public string OutputFile;
        public bool? IsJsonFile;
        public override string ToString()
        {
            return $"[lin] {FileName}, to {OutputFile} -j {IsJsonFile}";
        }
    }
    class Trimming : Command
    {
        public string FileName;
        public string OutputFile;
        public bool? IsJsonFile;
        public override string ToString()
        {
            return $"[trim] {FileName}, to {OutputFile} -j {IsJsonFile}";
        }
    }
    class SimulationProject : Command
    {
        public string FileName;
        public string OutputFolder;
        public double? StepSize;
        public double? StopTime;
        public bool? RealTimeSimulation;
        public bool? ToCompile;
        public bool? IsJsonFile;
        public override string ToString()
        {
            return $"[sim] {FileName}, to {OutputFolder} ({StepSize}:{StopTime}), -r {RealTimeSimulation} -c {ToCompile} -j {IsJsonFile}";
        }
    }
    class Document : Command
    {
        public string ModelName;
        public bool? ToJson;
        public string OutputFolder;
        public override string ToString()
        {
            return $"[doc] model={ModelName} to_json={ToJson}";
        }
    }
    class Help : Command
    {
        public override string ToString()
        {
            return "[help]";
        }
    }
    class Version : Command
    {
        public override string ToString()
        {
            return "[version]";
        }
    }
    class Main : Command
    {}
}