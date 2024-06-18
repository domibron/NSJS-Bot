// NSJS Bot for Discord


using System.Text;
using Newtonsoft.Json;

namespace NSJSDiscordBot
{
	public class StoreData
	{
		public static StoreJson storejson;

		public static string? storeJsonString = "";

		public static List<string> Messages = new List<string>();
		public static List<DateTime> Times = new List<DateTime>();
		public static List<ulong> ChannelIDs = new List<ulong>();

		public static void StoreMessageAndTime(string msg, DateTime dateTime, ulong channel)
		{
			StoreData.Times.Add(dateTime);
			StoreData.Messages.Add(msg);
			StoreData.ChannelIDs.Add(channel);
			UpdateStoreFile();
		}

		public static void RemoveMessageAndTime(string msg, DateTime dateTime, ulong channel)
		{
			StoreData.Times.Remove(dateTime);
			StoreData.Messages.Remove(msg);
			StoreData.ChannelIDs.Remove(channel);
			UpdateStoreFile();
		}

		public static void UpdateStoreFile()
		{
			File.Delete("store.json");
			using (var fc = File.Create("store.json"))
				fc.Close();
			//         putting a json string here \/ will preseve the data
			StringBuilder strb = new StringBuilder(); //<-- important for writing to file
			StringWriter strw = new StringWriter(strb);

			using (JsonWriter writer = new JsonTextWriter(strw))
			{
				writer.Formatting = Formatting.Indented;

				writer.WriteStartObject();
				writer.WritePropertyName("Message");
				writer.WriteStartArray();
				foreach (string msg in StoreData.Messages)
				{
					writer.WriteValue(msg);
				}
				writer.WriteEnd();
				writer.WritePropertyName("Time");
				writer.WriteStartArray();
				foreach (DateTime dt in StoreData.Times)
				{
					writer.WriteValue(dt);
				}
				writer.WriteEnd();
				writer.WritePropertyName("ChannelID");
				writer.WriteStartArray();
				foreach (ulong dcid in StoreData.ChannelIDs)
				{
					writer.WriteValue(dcid);
				}
				writer.WriteEnd();
				writer.WriteEndObject();
			}

			using (var fs = File.OpenWrite("store.json"))
			using (var aasds = new StreamWriter(fs, new UTF8Encoding(false)))
				aasds.Write(strb);

			using (var fs = File.OpenRead("store.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				storeJsonString = sr.ReadToEnd();
		}

		public static void DropStore()
		{
			File.Delete("store.json");
			using (var fc = File.Create("store.json"))
				fc.Close();

			StringBuilder strb = new StringBuilder(); //<-- important for writing to file
			StringWriter strw = new StringWriter(strb);

			using (JsonWriter writer = new JsonTextWriter(strw))
			{
				writer.Formatting = Formatting.Indented;

				writer.WriteStartObject();
				writer.WritePropertyName("Message");
				writer.WriteStartArray();
				writer.WriteEnd();
				writer.WritePropertyName("Time");
				writer.WriteStartArray();
				writer.WriteEnd();
				writer.WritePropertyName("ChannelID");
				writer.WriteStartArray();
				writer.WriteEnd();
				writer.WriteEndObject();
			}

			using (var fs = File.OpenWrite("store.json"))
			using (var aasds = new StreamWriter(fs, new UTF8Encoding(false)))
				aasds.Write(strb);

			using (var fs = File.OpenRead("store.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				storeJsonString = sr.ReadToEnd();

			Messages.Clear();
			Times.Clear();
			ChannelIDs.Clear();
		}
	}
}