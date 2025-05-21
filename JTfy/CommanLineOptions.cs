using CommandLine;
using CommandLine.Text;

namespace JTfy
{   public class PositionalForExampleOnly
    {
        [Value(0, Required = true, HelpText = "Path to input, 3DXML, file")]
        public required string Input { get; set; }
    }

    public class CommanLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to input, 3DXML, file")]
        public required string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to output, JT, file")]
        public string? Output { get; set; }

        [Option('m', "monolithic", Default = true, Required = false, HelpText = "Produces single JT file")]
        public bool? Monolithic { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples => [
            new Example("Create JT file in same location and with same name as source file", new PositionalForExampleOnly { Input = "path/to/file.3dxml" }),
            new Example("Change name and/or location of JT file", new CommanLineOptions { Input = "path/to/file_name.3dxml", Output = "different/path/to/new_file_name.jt" }),
            new Example("Produce Standard (as opposed to Monolithic) JT file structure", new CommanLineOptions { Input = "path/to/file.3dxml", Monolithic = false })
        ];
    }
}