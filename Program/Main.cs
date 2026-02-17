using PlusStudioConverterTool.Extensions;
using PlusStudioConverterTool.Services;
using System.Reflection;
using System.Security.Principal;

namespace PlusStudioConverterTool
{
	internal static partial class Program
	{
        private const string EXE_PATH = "C:\\Program Files\\PlusStudioConverterTool\\Program.exe";

        private static void Main(string[] args)
		{

            // Debug operation to get the json file all ready
            // File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "FilterObjectSample.json"),
            // 	System.Text.Json.JsonSerializer.Serialize(new FilterObject(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
            // 	{
            // 		AreaType = LevelFieldType.Object,
            // 		replacements = new()
            // 		{
            // 		{ "examination", "examinationtable" },
            // 		{ "cabinettall", "cabinet" },
            // 		}
            // 	}, Newtonsoft.Json.Formatting.Indented)
            // );
            // File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "FilterDoorSample.json"),
            // 	System.Text.Json.JsonSerializer.Serialize(new FilterObject(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
            // 	{
            // 		AreaType = LevelFieldType.Door,
            // 		replacements = new()
            // 		{
            // 		{ "swing", "swinging" },
            // 		{ "swingsilent", "swinging_silent" },
            // 		{ "coin", "coinswinging" },
            // 		}
            // 	}, Newtonsoft.Json.Formatting.Indented)
            // );
            // File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "FilterTextureSample.json"),
            // 	System.Text.Json.JsonSerializer.Serialize(new FilterObject(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
            // 	{
            // 		AreaType = LevelFieldType.RoomTexture,
            // 		replacements = new()
            // 		{
            // 		{ "FacultyWall", "WallWithMolding" },
            // 		{ "Actual", "TileFloor" }
            // 		}
            // 	}, Newtonsoft.Json.Formatting.Indented)
            // );
            // return;

        // ********* Only-once setup ***********
            AltLevelLoaderExtensions.InitializeSettings();
			Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? Version;

		start:
			Console.Clear();
			Console.WriteLine(@"
██████╗ ██╗     ██╗   ██╗███████╗    ███████╗████████╗██╗   ██╗██████╗ ██╗ ██████╗      ██████╗████████╗
██╔══██╗██║     ██║   ██║██╔════╝    ██╔════╝╚══██╔══╝██║   ██║██╔══██╗██║██╔═══██╗    ██╔════╝╚══██╔══╝
██████╔╝██║     ██║   ██║███████╗    ███████╗   ██║   ██║   ██║██║  ██║██║██║   ██║    ██║        ██║   
██╔═══╝ ██║     ██║   ██║╚════██║    ╚════██║   ██║   ██║   ██║██║  ██║██║██║   ██║    ██║        ██║   
██║     ███████╗╚██████╔╝███████║    ███████║   ██║   ╚██████╔╝██████╔╝██║╚██████╔╝    ╚██████╗   ██║   
╚═╝     ╚══════╝ ╚═════╝ ╚══════╝    ╚══════╝   ╚═╝    ╚═════╝ ╚═════╝ ╚═╝ ╚═════╝      ╚═════╝   ╚═╝                                                                                                                                                                                                                                                                                                            
			");

			Console.WriteLine($"Plus Studio Converter Tool. Made by PixelGuy. v{Version}");
			Console.WriteLine("Plus Level Editor and Plus Level Studio were made by MissingTextureMan101.");
			ConfigurationHandler.InitializeConfigFile();

			Console.WriteLine();

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    string folder = Path.GetDirectoryName(EXE_PATH);
                    Directory.CreateDirectory(folder);
                    File.Copy(Environment.ProcessPath, EXE_PATH, true); // Override to avoid exceptions
                    InitializeContextMenu();
                }
                else
                {
                    Console.WriteLine("Not running as Administrator, cannot add buttons to context menu!");
                    Console.WriteLine();
                }
            }

            if (args.Length != 0)
            {
                if (args[0] == FROM_CONTEXT_MENU_ARG)
                {
                    string file = args[1];
                    string to = args[2];

					ConverterService.ConvertSingleFile(file, to, args.Skip(3).ToArray());
                    return;
                }
            }

            bool emptyOutArgs = false, promptRestartTool = true;

			// **) Between options
			var optionTuple = ConsoleHelper.RetrieveUserSelection("Here\'s a list of the available menus to explore inside this tool.",
					"Converter Tool", // 1
					"Content Package Extractor", // 2
					"JSON-Filter Settings", // 3
					"EBPL Filter" // 4
					);
			Console.Clear();
			switch (optionTuple.Item1)
			{
				case 1:
					(emptyOutArgs, promptRestartTool) = ConverterField(ref args);
					if (!emptyOutArgs) // If false, it wants to exit
						goto exit;
					break;
				case 2:
					(emptyOutArgs, promptRestartTool) = ContentPackageExtractorField(ref args);
					if (!emptyOutArgs) // If false, it wants to exit
						goto exit;
					break;
				case 3:
					JSONConfigField();
					promptRestartTool = false;
					break;
				case 4:
					(emptyOutArgs, promptRestartTool) = EBPLFilterField(ref args);
					break;
			}



		exit:
			if (emptyOutArgs && args.Length != 0)
				args = []; // Empties out args to now repeat the same files
			if (!promptRestartTool || ConsoleHelper.CheckIfUserInputsYOrN("Restart the tool?"))
				goto start;

			Console.WriteLine("====\nPress any key to quit...");
			Console.ReadKey(true);
		}


		public static Version Version = new();
	}
}