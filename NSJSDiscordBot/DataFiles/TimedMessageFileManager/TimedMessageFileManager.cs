using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NSJSDiscordBot.DataFiles.TimedMessageFileManager
{
    public struct TimedMessageJson
    {
        [JsonProperty("Messages")]
        public List<string> Messages;

        [JsonProperty("Time")]
        public List<DateTime> Times;

        [JsonProperty("ChannelID")]
        public List<ulong> ChannelIDs;
    }

    public struct TimedMessage
    {
        public string Message;

        public DateTime Time;

        public ulong ChannelID;

        public TimedMessage(string Message, DateTime Time, ulong ChannelID)
        {
            this.Message = Message;
            this.Time = Time;
            this.ChannelID = ChannelID; 
        }
    }

    public class TimedMessageFileManager
    {

        private const string FileNameAndExtension = "TimeMessages.json";

        public static List<TimedMessage> TimedMessages = new List<TimedMessage>();



        public static readonly JObject fileContents = new JObject(
            new JProperty("Messages", new JArray()),
            new JProperty("Time", new JArray()),
            new JProperty("ChannelID", new JArray())
            );

        public static async Task<bool> ReadFile()
        {
            object? data = await JsonFileReader.GetJsonDataFromFile<TimedMessageJson>(FileNameAndExtension, fileContents);

            if (data == null) return false;

            TimedMessageJson jsonFileData = (TimedMessageJson)data;

            TimedMessages.Clear();

            
            for (int i = 0; i < jsonFileData.Messages.Count; i++)
            {
                TimedMessages.Add(new TimedMessage(jsonFileData.Messages[i], jsonFileData.Times[i], jsonFileData.ChannelIDs[i]));
            }

            return true;
        }

        public static async Task<bool> UpdateFile()
        {
            JObject jsonData;


            if (TimedMessages.Count <= 0)
            {
                jsonData = new JObject(
                new JProperty("Messages", new JArray()),
                new JProperty("Time", new JArray()),
                new JProperty("ChannelID", new JArray())
                );

            }
            else
            {

                List<string> messages  = new List<string>();
                TimedMessages.ForEach(x => messages.Add(x.Message));

                List<DateTime> times = new List<DateTime>();
                TimedMessages.ForEach(x => times.Add(x.Time));

                List<ulong> channelIDs = new List<ulong>();
                TimedMessages.ForEach(x => channelIDs.Add(x.ChannelID));

                // cant lists be turned into a array automatically?
                jsonData = new JObject(
                    new JProperty("Messages", messages.ToArray()),
                    new JProperty("Time", times.ToArray()),
                    new JProperty("ChannelID", channelIDs.ToArray())
                    );
            }

            try
            {
                await JsonFileWriter.UpdateFile(FileNameAndExtension, jsonData);
                return true;
            }
            catch
            {
                return false;
            }
        }



        public static async Task<bool> AddTimedMessage(TimedMessage message)
        {
            TimedMessages.Add(message);
            await UpdateFile();

            return true;
        }
        public static async Task<bool> AddTimedMessage(string message, DateTime time, ulong channel)
        {
            TimedMessages.Add(new TimedMessage(message, time, channel));
            await UpdateFile();

            return true;
        }



        public static async Task<bool> RemoveTimedMessage(string message, DateTime time, ulong channel)
        {
            TimedMessages.Remove(new TimedMessage(message, time, channel));
            await UpdateFile();

            return true;
        }

        public static async Task<bool> RemoveTimedMessage(TimedMessage message)
        {
            TimedMessages.Remove(message);
            await UpdateFile();

            return true;
        }
    }
}
