using CommandLine;
using J2JBreaker.Breakers;
using J2JBreaker.Breakers.Abstractions;
using J2JBreaker.Breakers.Enums;
using J2JBreaker.Extensions;
using J2JBreaker.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;

namespace J2JBreaker
{
    public class Options
    {
        // Variables
        [Option('i', "input", Required = true, HelpText = "Specifies a J2J file to estimate the password.")]
        public string? InputPath { get; set; }

        [Option('f', "format", Required = true, HelpText = "Specifies the file format(Available formats: ZIP, 7Z, RAR).")]
        public string? Format { get; set; }

        [Option('l', "length", Required = false, HelpText = "Specifies the max length of password.", Default = (uint)32)]
        public uint Length { get; set; }

        [Option('t', "table", Required = false, HelpText = "Specifies the path of a rainbow table file.")]
        public string? RainbowTablePath { get; set; }

        [Option('u', "use-specials", Required = false, HelpText = "Specifies whether to use special characters.")]
        public bool UseSpecials { get; set; }

        [Option('n', "no-except", Required = false, HelpText = "Specifies whether to except abnormal passwords.")]
        public bool NoExcept { get; set; }

        // Modes
        [Option('b', "bruteforce", Required = false, HelpText = "Sets the mode to brute-force mode.")]
        public bool BruteForce { get; set; }

        [Option('s', "signature", Required = false, HelpText = "Sets the mode to signature-based mode.")]
        public bool Signature { get; set; }

        [Option('e', "use-enhancer", Required = false, HelpText = "Specifies whether to use the enhancer")]
        public bool UseEnhancer { get; set; }
    }

    public class Program
    {
        private static Stopwatch _stopwatch = new Stopwatch();

        public static void Main(string[] args)
        {
            Console.WriteLine(@"       _ ___      _ ____  _____  ______          _  ________ _____  
      | |__ \    | |  _ \|  __ \|  ____|   /\   | |/ /  ____|  __ \ 
      | |  ) |   | | |_) | |__) | |__     /  \  | ' /| |__  | |__) |
  _   | | / /_   | |  _ <|  _  /|  __|   / /\ \ |  < |  __| |  _  / 
 | |__| |/ /| |__| | |_) | | \ \| |____ / ____ \| . \| |____| | \ \ 
  \____/|____\____/|____/|_|  \_\______/_/    \_\_|\_\______|_|  \_\");

            // Initializes a Serilog Logger.
            string fileName = @"data\logs\log-.log";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(fileName, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 100000)
                .CreateLogger();

            // Parses Commandline Arguments.
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    // Checks the existence of the file.
                    if (!File.Exists(o.InputPath))
                    {
                        Log.Error($"The input file does not found.");
                        return;
                    }

                    // Checks the header of the file.
                    using (J2JHelper helper = new J2JHelper(new FileStream(o.InputPath ?? string.Empty, FileMode.Open, FileAccess.Read)))
                    {
                        if (!helper.IsValid())
                        {
                            Log.Error($"The input file is not a valid J2J file.");
                            return;
                        }
                    }

                    // Parses the file format of a file and sets the mode.
                    Breaker? breaker = null;

                    switch (o.Format?.ToLower())
                    {
                        case "zip":
                            breaker = new ZipBreaker(new FileStream(o.InputPath ?? string.Empty, FileMode.Open, FileAccess.Read));
                            break;
                        case "7z":
                            breaker = new SevenZipBreaker(new FileStream(o.InputPath ?? string.Empty, FileMode.Open, FileAccess.Read));
                            break;
                        case "rar":
                            breaker = new RarBreaker(new FileStream(o.InputPath ?? string.Empty, FileMode.Open, FileAccess.Read));
                            break;
                        default:
                            Log.Warning($"An invalid file format has been entered.");
                            return;
                    }

                    // Sets the max length.
                    if (o.Length > 0 && o.Length <= 32)
                    {
                        breaker.Length = o.Length;
                    }
                    else if (o.Length > 32)
                    {
                        Log.Information($"The max length must be 32 or less.");
                    }
                    else
                    {
                        Log.Warning($"The max length cannot be zero.");
                        return;
                    }

                    // Sets the rainbow table.
                    if (o.BruteForce || o.UseEnhancer)
                    {
                        if (!File.Exists(o.RainbowTablePath))
                        {
                            Log.Error($"The rainbow table file does not found.");
                            return;
                        }

                        breaker.RainbowTablePath = o.RainbowTablePath;
                    }

                    // Sets the character set.
                    if (o.UseSpecials)
                    {
                        breaker.CharacterSet = VariableBuilder.GetCharacterSet(true);
                    }

                    // Sets the enhancer.
                    if (o.UseEnhancer)
                    {
                        breaker.UseEnhancer = o.UseEnhancer;
                    }

                    // Sets the no-except mode.
                    if (o.NoExcept)
                    {
                        breaker.NoExcept = o.NoExcept;
                    }

                    // Checks the operation mode.
                    bool isModeSelected = false;

                    if (o.BruteForce)
                    {
                        isModeSelected = true;
                        breaker.Mode = BreakingMode.BruteForce;
                    }

                    if (o.Signature)
                    {
                        isModeSelected = true;
                        breaker.Mode = BreakingMode.Signature;
                    }

                    if (!isModeSelected)
                    {
                        Log.Warning("The operation mode is not selected.");
                        return;
                    }

                    if (o.BruteForce && o.Signature)
                    {
                        Log.Warning("You can select only one operation mode.");
                        return;
                    }

                    // Prints the information.
                    string mode = string.Empty;

                    if (o.BruteForce)
                    {
                        mode = "Brute-Force";
                    }

                    if (o.Signature)
                    {
                        mode = "Signature-Based";
                    }

                    Log.Information($"--------------------------------------------------");
                    Log.Information($"Variables");
                    Log.Information($"--------------------------------------------------");
                    Log.Information($" * INPUT PATH : {o.InputPath}");
                    Log.Information($" * BREAKER TYPE : {o.Format.ToUpper()}");
                    Log.Information($" * MAX LENGTH : {o.Length}");
                    Log.Information($" * RAINBOW TABLE PATH : {o.RainbowTablePath}");
                    Log.Information($" * USE-SPECIALS : {o.UseSpecials}");
                    Log.Information($" * NO-EXCEPT : {o.NoExcept}");
                    Log.Information($"--------------------------------------------------");
                    Log.Information($"Mode");
                    Log.Information($"--------------------------------------------------");
                    Log.Information($" * MODE : {mode}");
                    Log.Information($" * USE-ENHANCER : {o.UseEnhancer}");

                    // Starts the task.
                    try
                    {
                        Log.Information($"--------------------------------------------------");
                        Log.Information($"Log");
                        Log.Information($"--------------------------------------------------");
                        _stopwatch.Restart();

                        bool result = false;

                        List<string> passwords = breaker.Break(out result);
                        passwords?.Sort();

                        _stopwatch.Stop();

                        Log.Information($"COMPLETE. The operation is finished.(RESULT : {result}, ELAPSED TIME : {_stopwatch.ElapsedMilliseconds}ms).");

                        // Prints the result.
                        if (result == true)
                        {
                            Log.Information($"--------------------------------------------------");
                            Log.Information($"Result");
                            Log.Information($"--------------------------------------------------");

                            int digits = passwords.Count().DigitsLog10();

                            for (int i=0; i<passwords.Count(); i++)
                            {
                                Log.Information($"{(i + 1).ToString("D" + digits)}: {passwords[i]}");
                            }

                            Log.Information($"※ Accuracy of the result may vary depending on the mode selected.");
                        }
                        else
                        {
                            Log.Information($"※ The estimation result does not exist.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"An unknown error has occurred.");
                    }
                });
        }
    }
}