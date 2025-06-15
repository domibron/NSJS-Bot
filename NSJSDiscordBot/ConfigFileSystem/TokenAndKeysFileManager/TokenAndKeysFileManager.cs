using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.DataFiles.BotTokensAndKeys
{
    public struct TokenAndKeysJson
    {
        [JsonProperty("DiscordToken")]
        public string DiscordToken { get; private set; }

        //[JsonProperty("GithubPersonalAccessToken")]
        //public string GithubPersonalAccessToken { get; private set; }
    }

    public class TokenAndKeysFileManager
    {
        private const string FileNameAndExtension = "BotTokenAndKeys.json";

        public static TokenAndKeysJson TokenAndKeys { get; private set; }

        private static readonly JObject fileContents = new JObject(
            new JProperty("DiscordToken", "YOUR DISCORD TOKEN HERE")//,
            //new JProperty("GithubPersonalAccessToken", "YOUR GITHUB FINE GRAINED TOKEN HERE")
        );


        public static async Task<bool> GetReadTokenAndKeys()
        {
            object? data = await JsonFileReader.GetJsonDataFromFile<TokenAndKeysJson>(FileNameAndExtension, fileContents);

            if (data == null) return false;

            TokenAndKeys = (TokenAndKeysJson)data;

            return true;
        }
    }
}
