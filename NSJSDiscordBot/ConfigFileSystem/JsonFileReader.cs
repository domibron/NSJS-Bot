using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace NSJSDiscordBot.DataFiles
{
    public class JsonFileReader
    {



        public static async Task<object?> GetJsonDataFromFile<T>(string pathToFile, JObject configFileStructure)
        {
            try
            {
                return await TryToDecryptAndReadJsonFile<T>(pathToFile, configFileStructure);
            }
            catch
            {
                Util.PrintToConsole.Print(ConsoleColor.Red, "Failed to get the data, trying again just in case!");

                try // not sure about this.
                {
                    return await TryToDecryptAndReadJsonFile<T>(pathToFile, configFileStructure);
                }
                catch
                {
                    throw;
                }
            }
        }

        private static async Task<object?> TryToDecryptAndReadJsonFile<T>(string pathToFile, JObject configFileStructure)
        {
            object? returnObject = DecryptJsonFile<T>(await ReadJsonFile(pathToFile, configFileStructure));
            Util.PrintToConsole.Print(ConsoleColor.Green, "File contents read and decrypted successfully");
            return returnObject;
        }

        private static async Task<string> ReadJsonFile(string pathToFile, JObject fileContentsToGenerate)
        {
            string? json = "";
            try
            {
                Util.PrintToConsole.Print(ConsoleColor.Yellow, $"Attempting to open {pathToFile}...");

                using (var fs = File.OpenRead(pathToFile))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                Util.PrintToConsole.Print(ConsoleColor.Green, "Read file successfully");
            }
            catch
            {
                Util.PrintToConsole.Print(ConsoleColor.Red, "Failure to load, creating...");

                await JsonFileWriter.CreateJsonFile(pathToFile, fileContentsToGenerate);


                // we re read it so we can continue in the application.
                Util.PrintToConsole.Print(ConsoleColor.Yellow, $"Attempting to open {pathToFile}...");

                using (var fs = File.OpenRead(pathToFile))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                Util.PrintToConsole.Print(ConsoleColor.Green, "Read file successfully");

                //if (crashOut) Environment.FailFast($"File error {pathToFile}");

                //throw;

            }

            return json;
        }

        private static object? DecryptJsonFile<T>(string json)
        {
            // next, let's load the values from that file
            // to our client's configuration
            try
            {
                T? returnValue = JsonConvert.DeserializeObject<T>(json);
                Util.PrintToConsole.Print(ConsoleColor.Green, "Decrypted file successfully");
                return returnValue;
            }
            catch
            {
                Environment.FailFast($"ERROR OCCURED! When trying to read the file");
            }

            return null;
        }
    }
}
