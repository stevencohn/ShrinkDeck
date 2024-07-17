//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

using ShrinkDeck;


var path = ValidateArguments(args);
if (path is null)
{
	return;
}

$"analyzing {path}".Report(ConsoleColor.Green);

using var scratch = new ScratchpadAdapter();
scratch.UnpackArtifacts(path);

var shrinker = new Shrinkifier(scratch.Pad);
shrinker.Shrink();

if (shrinker.BittySize < shrinker.BigSize)
{
	var savings = 100 - (((double)shrinker.BittySize / shrinker.BigSize) * 100);

	Console.WriteLine(
		$"\nShrunk {shrinker.BigSize} byes to {shrinker.BittySize} bytes, " +
		$"a {savings:F2}% savings of {shrinker.BigSize - shrinker.BittySize} bytes");

	var pack = Path.Combine(
		Path.GetDirectoryName(path) ?? ".\\",
		$"{Path.GetFileNameWithoutExtension(path)}-small") + Path.GetExtension(path);

	$"\nsaving to {pack}, retaining original".Report(ConsoleColor.Green);
	scratch.RepackageArtifacts(pack);
}

Console.WriteLine("Done");


// -----------------------------------------------------------------------------------------------

static string? ValidateArguments(string[] args)
{
	if (args.Length < 1)
	{
		Console.WriteLine("include path to ppt/pptx file");
		return null;
	}

	var path = Path.GetFullPath(args[0]);
	if (!File.Exists(path))
	{
		Console.WriteLine("file not found");
		return null;
	}

	var ext = Path.GetExtension(path);

	if (ext != ".pptx")
	{
		Console.WriteLine("file must have a .pptx extension");
		return null;
	}

	return path;
}