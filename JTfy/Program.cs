using JTfy;

if (args.Length == 0)
    throw new ArgumentException("Input file argument no found");

var sourcePath = args[0];

var destinationPath = Path.Combine(Path.GetDirectoryName(sourcePath) ?? "", Path.GetFileNameWithoutExtension(sourcePath) + ".jt");

ThreeDXMLReader.Read(sourcePath).Save(destinationPath, true);