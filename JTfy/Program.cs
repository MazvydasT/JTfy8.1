using JTfy;
using System.Security.Cryptography;

Console.OutputEncoding = System.Text.Encoding.UTF8;

args = [@"C:\Users\mtadara1\Downloads\PD&D_MLA_NITRA_ZONE6.4_XC minus fence.3dxml"];

if (args.Length == 0)
    throw new ArgumentException("Input file argument no found");

var sourcePath = args[0];

var monolithic = true;

var destinationPath = Path.Combine(Path.GetDirectoryName(sourcePath) ?? "", Path.GetFileNameWithoutExtension(sourcePath) + ".jt");

var messages = new Dictionary<string, HashSet<string>>();
var progressConsoleRow = 0;

var printProgress = (float progress, string message, string messageExt) =>
{
    if (!messages.TryGetValue(message, out var messageExts))
    {
        messages.Add(message, messageExts = []);
    }

    messageExts.Add(messageExt);

    Console.SetCursorPosition(0, 0);

    Console.WriteLine($" In: {sourcePath}");
    Console.WriteLine($"Out: {destinationPath}");
    Console.WriteLine();

    foreach (var (storedMessage, storedMessageExts) in messages)
    {
        Console.WriteLine($"{storedMessage} {storedMessageExts.LastOrDefault("")}".Trim().PadRight(Console.WindowWidth));
    }

    Console.WriteLine($"{(progress * 100):#.00}%".PadLeft("100.00%".Length));
    Console.WriteLine();
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
        var nodeSaveProgress = MathF.Min(nodesSaved++ / (float)nodeCount, 1f);

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