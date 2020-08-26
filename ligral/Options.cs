using CommandLine;
using CommandLine.Text;

namespace Ligral
{
    class Options
    {
        [Value(0, MetaName="input", HelpText="Parse the .lig file.")] 
        public string InputFile { get; set; }
            
        [Option('o', "output", HelpText = "Redirect the outputs to a given folder.")]
        public string OutputFolder { get; set; }
        
        // [Option('t', "test", HelpText = "Run a test script.")]
        // public bool RequireTest { get; set; }
        
        [Option('d', "doc", HelpText = "Show the document of specific model")]
        public string RequireDoc { get; set; }

        [Option('D', "docs", HelpText = "Show the documents of all models.")]
        public bool RequireDocs { get; set; }

        [Option('h', HelpText = "Show the examples.")]
        public bool RequireExamples { get; set; }

        [Option('s', "size", HelpText = "Set the step size.")]
        public double? StepSize { get; set; }

        [Option('t', "time", HelpText = "Set the stop time.")]
        public double? StopTime { get; set; }
    }
}