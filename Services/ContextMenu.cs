using Microsoft.Win32;
using System;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace PlusStudioConverterTool.Services
{
    internal static class ContextMenu
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
        private const string MAIN_MENU_NAME = "Plus Studio Converter";
        private const string EXE_PATH = "C:\\Program Files\\PlusStudioConverterTool\\Program.exe";
        public const string FROM_CONTEXT_MENU_ARG = "FromContextMenu";

        private static void AddConvert(ConvertationRule rule)
        {
            try
            {
                using (RegistryKey extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{rule.From}"))
                {
                    extKey.SetValue("", rule.ProgId);
                }

                // Creating "multi button"
                string mainMenuPath = $@"Software\Classes\{rule.ProgId}\shell\PlusStudioMenu";
                using (RegistryKey mainMenuKey = Registry.CurrentUser.CreateSubKey(mainMenuPath))
                {
                    mainMenuKey.SetValue("MUIVerb", MAIN_MENU_NAME);
                    mainMenuKey.SetValue("SubCommands", "");
                    // mainMenuKey.SetValue("Icon", EXE_PATH);
                }


                string subCommandPath = $@"{mainMenuPath}\shell\{rule.ButtonText}";
                using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey(subCommandPath))
                {
                    subKey.SetValue("MUIVerb", rule.ButtonText);

                    using (RegistryKey cmdKey = subKey.CreateSubKey("command"))
                    {
                        StringBuilder b = new StringBuilder();
                        b.Append('"').Append(EXE_PATH).Append('"');
                        b.Append(" \"").Append(FROM_CONTEXT_MENU_ARG).Append("\"");
                        b.Append(" \"%1\"");
                        b.Append(" \"").Append(rule.To).Append('"');

                        foreach (string arg in rule.Args)
                        {
                            b.Append(" \"").Append(arg).Append('"');
                        }
                        cmdKey.SetValue("", b.ToString());
                    }
                }

                Console.WriteLine($"Added '{rule.ButtonText}' to sub-menu for {rule.From}");
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

        public static void Initialize()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    string folder = Path.GetDirectoryName(EXE_PATH);
                    Directory.CreateDirectory(folder);
                    File.Copy(Environment.ProcessPath, EXE_PATH, true); // Override to avoid exceptions

                    ContextMenu._Initialize();
                }
                else
                {
                    Console.WriteLine("Not running as Administrator, cannot add buttons to context menu!");
                    Console.WriteLine();
                }
            }
        }
        private static void _Initialize()
        {
            ConvertationRule[] rules = [
                // Legacy
                new ConvertationRule("Convert to .bld", ".cbld", ".bld"),

                // New
                new ConvertationRule("Convert to .ebpl", ".rbpl", ".ebpl")
            ];

            foreach (ConvertationRule rule in rules)
            {
                AddConvert(rule);
            }
        }
    }
}