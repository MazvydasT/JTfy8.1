using CommandLine;

namespace JTfy
{
    public class CommanLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to input, 3DXML, file")]
        public required string Input { get; set; }
        
        [Option('o', "output", Required = false, HelpText = "Path to output, JT, file")]
        public string? Output { get; set; }

        [Option('m', "monolithic", Default = true, Required = false, HelpText = "Produces single JT file")]
        public bool Monolithic { get; set; }
    }
}