// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Text.RegularExpressions;

namespace Keypear.CliClient.CliModel.GetCommands;

[Command("password", "pass",
    Description = "generates a random Password based on input parameters.")]
public class GetPasswordCommand
{
    public static readonly Regex CharsRange = new Regex("(.)\\.\\.(.)");

    private readonly MainCommand _main;
    private readonly IConsole _console;

    public GetPasswordCommand(GetCommand parent, IConsole console)
    {
        _main = parent.Main;
        _console = console;
    }

    [Option]
    [Required]
    public string? File { get; set; }

    [Option]
    public uint? Count { get; set; } = 1;

    public int OnExecute()
    {
        if (!IOFile.Exists(File))
        {
            _console.WriteError("input file not found");
            return -1;
        }

        var inputBody = IOFile.ReadAllText(File);
        var input = JsonSerializer.Deserialize<PasswordGeneratorInput>(inputBody,
            MainCommand.JsonInputOpts);

        // Validate the inputs
        if (input?.Classes?.Count == 0)
        {
            _console.WriteError("no Character Classes defined");
            return 1;
        }
        if (input!.Length <= 1)
        {
            _console.WriteError("missing or invalid length");
            return 1;
        }

        var totalMin = 0;
        var totalMax = 0;

        foreach (var cc in input.Classes!)
        {
            var ccChars = cc.Value.Chars;
            if (string.IsNullOrEmpty(ccChars))
            {
                _console.WriteError($"missing characters for Character Class [{cc.Key}]");
                return 1;
            }

            if (cc.Value.Min is int ccMin)
            {
                if (cc.Value.Max is int ccMinMax && ccMin > ccMinMax)
                {
                    _console.WriteError($"invalid min/max specified for Character Class [{cc.Key}]");
                    return 1;
                }

                totalMin += ccMin;
            }
            if (cc.Value.Max is int ccMax)
            {
                totalMax += ccMax;
            }
            else
            {
                totalMax = short.MaxValue;
            }
        }

        if (totalMin > input.Length)
        {
            _console.WriteError("specified minimums for Character Classes exceed total length");
            return 1;
        }
        if (totalMax < input.Length)
        {
            _console.WriteError("specified maximums for Character Classes are insufficient for total length");
            return 1;
        }

        foreach (var cc in input.Classes)
        {
            var ccChars = cc.Value.Chars!;
            while (CharsRange.Match(ccChars) is { Success: true, Groups: var g })
            {
                var vals = g.Values.ToArray();
                var from = vals[1].Value[0];
                var till = vals[2].Value[0];

                // DEBUG:
                //_console.WriteLine($"Range {from} .. {till}");

                var range = new string((from < till
                    ? Enumerable.Range(from, till - from + 1)
                    : Enumerable.Range(till, from - till + 1)).Select(x => (char)x).ToArray());

                ccChars = ccChars.Substring(0, vals[1].Index) + range + ccChars.Substring(vals[2].Index + 1);
            }

            // Normalized form, removes duplicates and orders in increasing ordinal order
            ccChars = new string(ccChars.OrderBy(x => x).Distinct().ToArray());

            // DEBUG:
            //_console.WriteLine($"Class {cc.Key}: {chars}");

            if (!input.RepeatsAllowed && cc.Value.Min is int ccMin && ccMin > ccChars.Length)
            {
                _console.WriteError($"repeats are not allowed, but [{cc.Key}] does not have enough unique characters to satisfy the minimum");
                return 1;
            }
            cc.Value.Chars = ccChars;
        }

        for (var total = 0; total < Count; total++)
        {
            var genChars = new List<GenCharsClass>();

            foreach (var cc in input.Classes)
            {
                var ccChars = cc.Value.Chars!;
                genChars.Add(new(cc.Value, ccChars.ToList()));
            }

            var charPool = new List<char>();
            var minGenChars = new List<GenCharsClass>(genChars);

            // First make sure we produce the min
            // number of chars required for each CC
            foreach (var gcc in minGenChars)
            {
                if (gcc._class.Min is int ccMin)
                {
                    for (var i = 0; i < ccMin; i++)
                    {
                        var ndx = Sodium.SodiumCore.GetRandomNumber(gcc._chars.Count);
                        charPool.Add(gcc._chars[ndx]);
                        if (!input.RepeatsAllowed)
                        {
                            gcc._chars.RemoveAt(ndx);
                        }
                        gcc._count++;
                    }

                    if (gcc._class.Max is int ccMax && gcc._count == ccMax)
                    {
                        // We've maxed out this CC so remove it
                        // from any further char gen iterations
                        genChars.Remove(gcc);
                    }
                }
            }

            //_console.WriteLine("Min Char Pool: " + new string(charPool.ToArray()));

            while (charPool.Count < input.Length)
            {
                var classNdx = Sodium.SodiumCore.GetRandomNumber(genChars.Count);
                var gcc = genChars[classNdx];
                var charNdx = Sodium.SodiumCore.GetRandomNumber(gcc._chars.Count);

                charPool.Add(gcc._chars[charNdx]);

                if (!input.RepeatsAllowed)
                {
                    gcc._chars.RemoveAt(charNdx);
                    if (gcc._chars.Count == 0)
                    {
                        // We've exhausted this CC for unique chars so
                        // we can't pull from it anymore in the future
                        genChars.Remove(gcc);
                    }
                }
                gcc._count++;

                if (gcc._class.Max is int ccMax && gcc._count == ccMax)
                {
                    // We've maxed out this CC so remove it
                    // from any further char gen iterations
                    genChars.Remove(gcc);
                }
            }

            var chars = charPool.ToArray();
            //_console.WriteLine("Full Char Pool: " + new string(chars));

            // Finally, randomize the assembled char pool
            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = Sodium.SodiumCore.GetRandomNumber(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            _console.WriteLine(new string(chars));
        }

        return 0;
    }

    public class PasswordGeneratorInput
    {
        public Dictionary<string, CharacterClass>? Classes { get; set; }

        public int Length { get; set; } = 20;

        public bool RepeatsAllowed { get; set; } = true;

        public class CharacterClass
        {
            public string? Chars { get; set; }
            public int? Min { get; set; }
            public int? Max { get; set; }
        }
    }

    class GenCharsClass
    {
        public GenCharsClass(PasswordGeneratorInput.CharacterClass @class, List<char> chars)
        {
            _class = @class;
            _chars = chars;
        }

        public PasswordGeneratorInput.CharacterClass _class;

        public List<char> _chars;

        public int _count;
    }
}
