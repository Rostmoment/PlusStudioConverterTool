using Microsoft.Win32;
using System;
using System.Security;

namespace PlusStudioConverterTool
{
    internal static partial class Program
    {
        private const string PROG_ID = "Plus.Studio.Converter.Tool";

        private static bool CheckIfRegistered(string fileExtension, out string progId)
        {
            try
            {
                using (RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(fileExtension))
                {
                    if (extKey != null)
                    {
                        progId = extKey.GetValue("") as string;
                        return !string.IsNullOrEmpty(progId);
                    }
                }
            }
            catch (SecurityException)
            {
            }

            progId = null;
            return false;
        }

        private static void RegisterProgId(string description = "Plus Studio Converter")
        {
            try
            {
                using (RegistryKey progIdKey = Registry.ClassesRoot.CreateSubKey(PROG_ID))
                {
                    progIdKey?.SetValue("", description);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Needed to run as administrator");
            }
        }

        private static void RegisterExtensions(string[] extensions)
        {
            foreach (string ext in extensions)
            {
                if (CheckIfRegistered(ext, out string existingProgId) && existingProgId == PROG_ID)
                    continue;

                try
                {
                    using (RegistryKey extKey = Registry.ClassesRoot.CreateSubKey(ext))
                    {
                        extKey?.SetValue("", PROG_ID);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Failed to register {ext}, {ex}");
                }
            }
        }

        private static void AddConvert(string menuName, string exePath, string toFormat)
        {
            try
            {
                using (RegistryKey menuKey = Registry.ClassesRoot.CreateSubKey($"{PROG_ID}\\shell\\{menuName}\\command"))
                {
                    menuKey?.SetValue("", $"\"{exePath}\" \"%1\" \"{toFormat}\"");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Couldn't add {menuName} {ex}");
            }
        }
    }
}