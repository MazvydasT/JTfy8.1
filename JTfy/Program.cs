using CommandLine;
using JTfy;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var startTime = DateTime.Now;

[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CommanLineOptions))]
CommanLineOptions? getOptions(string[] args)
{
    if (args.Length == 1 && File.Exists(args[0]))
        args = ["-i", args[0]];

    var result = Parser.Default.ParseArguments<CommanLineOptions>(args);

    return result.Value;
}

var options = getOptions(args);

if (options == null) return;

var sourcePath = options.Input;

var monolithic = options.Monolithic;

var destinationPath = options.Output ?? Path.Combine(Path.GetDirectoryName(sourcePath) ?? "", Path.GetFileNameWithoutExtension(sourcePath) + ".jt");

var messages = new Dictionary<string, HashSet<string>>();
var progressConsoleRow = 0;

var lastWidth = 1;
var lastHeight = -1;

var printProgress = (float progress, string message, string messageExt) =>
{
    var width = Console.WindowWidth;
    var height = Console.WindowHeight;

    if(lastWidth != width || lastHeight != height)
    {
        Console.Clear();

        lastWidth = width;
        lastHeight = height;
    }

    if (!messages.TryGetValue(message, out var messageExts))
        messages.Add(message, messageExts = []);

    messageExts.Add(messageExt);

    Console.SetCursorPosition(0, 0);

    string[] rows =
    [
        $" In: {sourcePath}",
        $"Out: {destinationPath}",
        "",
        .. messages.Select(messageAndExts => $"{messageAndExts.Key} {messageAndExts.Value.Last()}".Trim()),
        "",
        $"{(progress * 100):#.00}%",
        "",
        $"{(DateTime.Now - startTime):c}",
        ""
    ];

    var textToPrint = String.Join(
        "\n",
        rows.Select(row => row.PadRight(width - 1, ' '))
    );

    Console.Write(textToPrint);
};

progressConsoleRow = Console.CursorTop;

var rootJTNode = ThreeDXMLReader.Read(sourcePath, out var nodeCount, (progress) =>
{
    var messageExt = "";

    if (progress > .5 && progress < .75)
        messageExt = "(wait for it)";

    else if (progress > .75 && progress < 1f)
        messageExt = "(nearly there)";

    else if (progress == 1f)
        messageExt = "- done!";

    printProgress(progress * .5f, "Reading input file", messageExt);
});

var nodesSaved = 0;
rootJTNode.Save(destinationPath, monolithic, false, (progress, message, messageExt) =>
{
    if (progress == null)
    {
        var nodeSaveProgress = MathF.Min((nodesSaved++ * .5f) / (float)nodeCount, 1f);

        messageExt ??= "";

        if (nodeSaveProgress > .5 && nodeSaveProgress < .75)
            messageExt = "(I'm working as fast as I can)";

        else if (nodeSaveProgress > .75 && nodeSaveProgress < 1f)
            messageExt = "(one more sec)";

        else if (nodeSaveProgress == 1f)
            messageExt = "- done!";

        printProgress(.5f + nodeSaveProgress * .25f, message, messageExt);

        return;
    }

    printProgress(.75f + progress.Value * .25f, message, messageExt);
});

string[] completionMessages =
[
    "Done!",
    "Nothing's gone wrong, this time.",
    "Somehow we've reached the end.",
    "Next time we might not be as lucky!",
    "I think all is complete.",
    monolithic ? "Check if output file is usable." : "Check if output files are usable.",
    "All done, innit?",
    "Transmutational processes pertaining to binary data structures have reached terminal completion.",
    "Binary transmogrification successfully actualized!",
    "Bitwise alchemy: concluded.",
    "All 0s and 1s are now in their final, glorified form.",
    "Binary optimization lifecycle successfully executed!",
    "End-to-end binary realignment finalized across the data stack."
];
var completionMessageIndex = RandomNumberGenerator.GetInt32(completionMessages.Length);

string[] faces =
[
    @"<(^_^)>",
    @"(ᵔᴥᵔ)"
];
var faceIndex = RandomNumberGenerator.GetInt32(faces.Length);

printProgress(1f, completionMessages[completionMessageIndex], faces[faceIndex]);