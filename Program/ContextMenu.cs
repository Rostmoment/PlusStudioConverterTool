using Microsoft.Win32;
using System;
using System.Security;
using System.Text;

namespace PlusStudioConverterTool
{
    internal static partial class Program
    {
        private class ConvertationRule(string buttonText, string from, string to, params string[] args)
        {

            public string ProgId => $"{PROG_ID}{from}";
            public string ButtonText { get; } = buttonText;
            public string From { get; } = from;
            public string To { get; } = to;
            public string[] Args { get; } = args;
        }

        private const string PROG_ID = "Plus.Studio.Converter.Tool";


        private static void AddConvert(ConvertationRule rule)
        {
            try
            {
                using (RegistryKey extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{rule.From}"))
                {
                    extKey.SetValue("", rule.ProgId);
                }

                string shellPath = $@"Software\Classes\{rule.ProgId}\shell\{rule.ButtonText}\command";

                using (RegistryKey menuKey = Registry.CurrentUser.CreateSubKey(shellPath))
                {
                    StringBuilder b = new StringBuilder();
                    b.Append('"').Append(EXE_PATH).Append('"');

                    foreach (string arg in rule.Args)
                    {
                        b.Append(" \"").Append(arg).Append('"');
                    }

                    b.Append(" \"%1\"");

                    menuKey.SetValue("", b.ToString());
                }

                Console.WriteLine($"Added rule from {rule.From} to {rule.To} with name {rule.ButtonText}");
            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding {rule.ButtonText}: {ex.Message}");
            }
        }

        private static void InitializeContextMenu()
        {
            ConvertationRule[] rules = [
                // Legacy files
                new ConvertationRule("Convert to .bld", ".cbld", ".bld"),

                // Newer files
                new ConvertationRule("Convert to .ebpl", ".rbpl", ".ebpl")
            ];

            foreach (ConvertationRule rule in rules)
            {
                AddConvert(rule);
            }
        }
    }
}