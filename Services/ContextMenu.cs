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
                    MoveExe();
                    ContextMenu.InternalInitialize();
                }
                else
                {
                    Console.WriteLine("Not running as administrator, cannot add buttons to context menu!");
                    Console.WriteLine();
                }
            }
        }
        private static void MoveExe()
        {
            string folder = Path.GetDirectoryName(EXE_PATH);
            Directory.CreateDirectory(folder);
            File.Copy(Environment.ProcessPath, EXE_PATH, true); // Override to avoid exceptions

            // Need to copy default filters to be sure that they are active
            foreach (string json in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json"))
            {
                using FileStream source = new FileStream(json, FileMode.Open, FileAccess.Read);
                using FileStream dest = new FileStream(Path.Combine(folder, Path.GetFileName(json)), FileMode.Create, FileAccess.Write);
                source.CopyTo(dest); // to avoid IOException
            }
        }
        private static void InternalInitialize()
        {
            ConvertationRule[] rules = [
                // Legacy
                new ConvertationRule("Convert to .bld", ".cbld", ".bld"),
                new ConvertationRule("Convert to .rbpl (With auto door spots)", ".cbld", ".rbpl", bool.TrueString),
                new ConvertationRule("Convert to .rbpl (No auto door spots)", ".cbld", ".rbpl", bool.FalseString),

                new ConvertationRule("Convert to full .ebpl (With procedural light)", ".bld", ".ebpl",  bool.TrueString, "Full"),
                new ConvertationRule("Convert to full .ebpl (No procedural light)", ".bld", ".ebpl",  bool.FalseString, "Full"),
                new ConvertationRule("Convert to compliant .ebpl (With procedural light)", ".bld", ".ebpl",  bool.TrueString, "Compliant"),
                new ConvertationRule("Convert to compliant .ebpl (No procedural light)", ".bld", ".ebpl",  bool.FalseString, "Compliant"),

                // New
                new ConvertationRule("Convert to .ebpl", ".rbpl", ".ebpl"),
                new ConvertationRule("Export Lua script", ".pbpl", ".lua"),

                new ConvertationRule("Convert to full .ebpl", ".pbpl", ".ebpl", "Full"),
                new ConvertationRule("Convert to compliant .ebpl", ".pbpl", ".ebpl", "Compliant"),

                new ConvertationRule("Convert to compliant .ebpl", ".bpl", ".ebpl", "Compliant"),
                new ConvertationRule("Convert to full .ebpl", ".bpl", ".ebpl", "Full")
            ];

            foreach (ConvertationRule rule in rules)
            {
                AddConvert(rule);
            }
        }
    }
}