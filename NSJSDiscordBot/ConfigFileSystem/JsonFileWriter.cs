using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.DataFiles
{
    public class JsonFileWriter
    {
        public static async Task CreateJsonFile(string pathToFile, JObject fileStructure)
        {
            using (var fc = File.Create(pathToFile))
                fc.Close();


            using (FileStream fileSteam = File.OpenWrite(pathToFile))
            using (StreamWriter streamWriter = new StreamWriter(fileSteam))
            using (JsonWriter writer = new JsonTextWriter(streamWriter))
            {
                writer.Formatting = Formatting.Indented;
                await fileStructure.WriteToAsync(writer);
            }
        }

        public static async Task<bool> UpdateFile(string pathToFile, JObject fileData)
        {
            try
            {
                using (FileStream fileSteam = File.Create(pathToFile))
                using (StreamWriter streamWriter = new StreamWriter(fileSteam))
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    await fileData.WriteToAsync(writer);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
