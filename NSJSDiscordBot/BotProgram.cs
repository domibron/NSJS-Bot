using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

using NSJSDiscordBot.Commands;
using System.Security.Authentication;
using Newtonsoft.Json.Converters;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using DSharpPlus.SlashCommands.EventArgs;

using DSharpPlus.SlashCommands.Attributes;
using System.Runtime.InteropServices;

namespace NSJSDiscordBot
{
	public class BotProgram
	{

		#region Varibles
		//public static string Version { get; set; }

		public bool sentMessage = false;

		public readonly EventId BotEventId = new EventId(42, "Bot-Ex01");

		public DiscordClient Client { get; set; }

		public CommandsNextExtension Commands { get; set; }

		public SlashCommandsExtension Slash { get; set; }

		public DiscordConfiguration cfg;

		public ConfigJson cfgjson;



		public VersionJson versionjson;



		public bool Connection = false;

		//public static CoreData coreData = new CoreData();

		#endregion

		#region Update
		public async void Update()
		{
			try
			{
				NSJSUtil.Print("ATTEMPTING TO OPEN STORE...", ConsoleColor.Yellow);
				using (var fs = File.OpenRead("store.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					StoreData.storeJsonString = await sr.ReadToEndAsync();
			}
			catch
			{
				NSJSUtil.Print("FALIURE IN UPDATE!", ConsoleColor.Red);
				using (var fc = File.Create("store.json"))
					fc.Close();
				using (var fs = File.OpenRead("store.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					StoreData.storeJsonString = await sr.ReadToEndAsync();

				StringBuilder strb = new StringBuilder(); //<-- important for writing to file
				StringWriter strw = new StringWriter(strb);

				using (JsonWriter writer = new JsonTextWriter(strw))
				{
					writer.Formatting = Formatting.Indented;

					await writer.WriteStartObjectAsync();
					await writer.WritePropertyNameAsync("Message");
					await writer.WriteStartArrayAsync();
					await writer.WriteEndAsync();
					await writer.WritePropertyNameAsync("Time");
					await writer.WriteStartArrayAsync();
					await writer.WriteEndAsync();
					await writer.WritePropertyNameAsync("ChannelID");
					await writer.WriteStartArrayAsync();
					await writer.WriteEndAsync();
					await writer.WriteEndObjectAsync();
				}

				using (var fs = File.OpenWrite("store.json"))
				using (var aasds = new StreamWriter(fs, new UTF8Encoding(false)))
					await aasds.WriteAsync(strb);

				using (var fs = File.OpenRead("store.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					StoreData.storeJsonString = await sr.ReadToEndAsync();
			}

			try
			{
				NSJSUtil.Print("ATTEMPTING TO STORE STORE...", ConsoleColor.Yellow);
				var format = "yyyy-MM-ddTHH:mm:ss.FFFZ"; // your datetime format
				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };

				StoreData.storejson = JsonConvert.DeserializeObject<StoreJson>(StoreData.storeJsonString, dateTimeConverter);


			}
			catch
			{
				NSJSUtil.Print("FALIURE ATTEMPTING TO STORE STORE...", ConsoleColor.Red);
				using (var fs = File.OpenRead("store.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					StoreData.storeJsonString = await sr.ReadToEndAsync();

				NSJSUtil.Print(StoreData.storeJsonString);


				var format = "yyyy-MM-ddTHH:mm:ss.FFFZ"; // your datetime format
				var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
				try
				{
					StoreData.storejson = JsonConvert.DeserializeObject<StoreJson>(StoreData.storeJsonString, dateTimeConverter);
				}
				catch
				{
					NSJSUtil.Print("Error with store, is it empty?", ConsoleColor.Red);
					System.Environment.FailFast("Error with store, is it empty?");
				}

				//storejson = JsonConvert.DeserializeObject<StoreJson>(storeJsonString);
			}

			try
			{
				StoreData.Times = StoreData.storejson.Time;
				StoreData.Messages = StoreData.storejson.Message;
				StoreData.ChannelIDs = StoreData.storejson.ChannelID;
			}
			catch
			{
				NSJSUtil.Print("UH OH", ConsoleColor.Red);
				System.Environment.FailFast("FAILURE AT STORE.JSON");
			}

			if (StoreData.Times.Count > 0 && Connection)
			{
				for (int i = 0; i < StoreData.Times.Count; i++)
				{
					//d1 < 0 − If date1 is earlier than date2
					//d1 = 0 − If date1 is the same as date2
					//d1 > 0 − If date1 is later than date2
					if (DateTime.Compare(StoreData.Times[i], DateTime.Now) <= 0)
					{
						string rsm = StoreData.Messages[i];
						DateTime rdt = StoreData.Times[i];
						ulong rdc = StoreData.ChannelIDs[i];

						DiscordChannel dc = await Client.GetChannelAsync(rdc); // I want to send the message even if its late.
						await Client.SendMessageAsync(dc, rsm);

						StoreData.RemoveMessageAndTime(rsm, rdt, rdc);

						NSJSUtil.Print("I sent and removed some outdated messages", ConsoleColor.DarkYellow);
					}
				}
			}

			while (true)
			{
				//try
				//{
				//    Console.WriteLine(Client.ReconnectAsync().Status);
				//}
				//catch when (Client.ReconnectAsync() != null)
				//{
				//    Console.WriteLine("NR");
				//}


				// if we return then the application will stop the update method.
				if (!Connection) { continue; }

				for (int i = 0; i < StoreData.Times.Count; i++)
				{
					try
					{
						if (StoreData.Times[i].Year <= DateTime.Now.Year && StoreData.Times[i].Month <= DateTime.Now.Month && StoreData.Times[i].Day <= DateTime.Now.Day && StoreData.Times[i].Hour <= DateTime.Now.Hour && StoreData.Times[i].Minute <= DateTime.Now.TimeOfDay.Minutes)
						{

							try
							{
								DateTime timeSpanToRemove = StoreData.Times[i];
								string messageToRemove = StoreData.Messages[i];
								ulong discordChannelToRemove = StoreData.ChannelIDs[i];

								DiscordChannel dc = await Client.GetChannelAsync(discordChannelToRemove);
								await Client.SendMessageAsync(dc, messageToRemove);

								StoreData.RemoveMessageAndTime(messageToRemove, timeSpanToRemove, discordChannelToRemove);
							}
							catch (Exception ex) { Console.WriteLine("connot send the message"); } // message will not send

						}
					}
					catch (ArgumentOutOfRangeException)
					{
						NSJSUtil.Print("Data was removed before I could do anything", ConsoleColor.Yellow);
					}

					//Console.WriteLine(DateTime.Now.Day);

					//Console.WriteLine(storejson.Time[i].Value);

					//try // I am calling data that is not there, should immidetly back out after deletion.
					//{
					//    Console.WriteLine(storejson.Time[i].Value + " vs " + DateTime.Now.TimeOfDay);
					//}
					//catch(ArgumentOutOfRangeException)
					//{
					//    Console.WriteLine("Ill go fuck my self then");
					//}
				}

				if (DateTime.Now.Minute == 0 && DateTime.Now.Second == 0 && !sentMessage)
				{
					Client.Logger.LogInformation($"Im Alive! Time:{DateTime.Now.TimeOfDay}");
					DiscordChannel dc = await Client.GetChannelAsync(1127014221968846948);
					await Client.SendMessageAsync(dc, $"Im alive! \nTime:{DateTime.Now.TimeOfDay}");
					sentMessage = true;
				}

				if (DateTime.Now.Minute != 0 && DateTime.Now.Second != 0 && sentMessage)
				{
					sentMessage = false;
				}

			}
		}

		#endregion

		#region Bot Core Running

		public async Task RunBotAsync()
		{
			string json = await ReadConfigFile();

			json = await DecryptConfigJsonFile(json);

			NSJSUtil.Print("NO FALIURE", ConsoleColor.Yellow);

			#region Set Up Bot

			cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug,

				Intents = DiscordIntents.All,

				// TEMP SOlution to drop out connection.
				ReconnectIndefinitely = true
			};

			NSJSUtil.Print("SUCCESS", ConsoleColor.Green);

			// then we want to instantiate our client
			this.Client = new DiscordClient(cfg);

			// next, let's hook some events, so we know
			// what's going on
			this.Client.Ready += this.Client_Ready;
			this.Client.GuildAvailable += this.Client_GuildAvailable;
			this.Client.ClientErrored += this.Client_ClientError;

			// up next, let's set up our commands
			var ccfg = new CommandsNextConfiguration
			{
				// let's use the string prefix defined in config.json
				StringPrefixes = new[] { cfgjson.CommandPrefix },

				// enable responding in direct messages
				EnableDms = false,

				// enable mentioning the bot as a command prefix
				EnableMentionPrefix = true
			};

			// and hook them up
			this.Commands = this.Client.UseCommandsNext(ccfg);

			var sccfg = new SlashCommandsConfiguration
			{
				Services = new ServiceCollection().AddSingleton<Random>().BuildServiceProvider()
			};

			this.Slash = this.Client.UseSlashCommands(sccfg);

			#endregion

			#region Event Hooking

			// let's hook some command events, so we know what's 
			// going on
			this.Commands.CommandExecuted += this.Commands_CommandExecuted;
			this.Commands.CommandErrored += this.Commands_CommandErrored;

			// very important in keeping the bot running incase of a error with discord or internet.
			this.Client.Resumed += this.Client_Ready;
			this.Client.SocketErrored += this.SockError;
			this.Client.SocketClosed += this.SockClosed;
			this.Client.UnknownEvent += this.UnkownError;


			//// let's add a converter for a custom type and a name
			//var mathopcvt = new MathOperationConverter();
			//Commands.RegisterConverter(mathopcvt);
			//Commands.RegisterUserFriendlyTypeName<MathOperation>("operation");

			// up next, let's register our commands
			this.Commands.RegisterCommands<UngrouppedCommands>();
			this.Commands.RegisterCommands<AdminCommands>();
			//this.Commands.RegisterCommands<ExampleExecutableGroup>();

			this.Slash.RegisterCommands<SlashCommands>();

			// set up our custom help formatter
			this.Commands.SetHelpFormatter<SimpleHelpFormatter>();

			this.Slash.SlashCommandInvoked += this.Slash_Invoked;
			this.Slash.SlashCommandErrored += this.Slash_CommandErrord;

			#endregion

			NSJSUtil.Print("----=={ BOT READY? }==----", ConsoleColor.Red);

			// finally, let's connect and log in
			try
			{
				await this.Client.ConnectAsync();
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("You need a valid discord token!");
				System.Environment.Exit(1);
			}

			NSJSUtil.Print("----=={ YES! } ==----", ConsoleColor.Green);

			// and this is to prevent premature quitting
			await Task.Delay(-1);

		}

		#endregion

		#region Decrypt Config Json File

		private async Task<string> DecryptConfigJsonFile(string json)
		{
			// next, let's load the values from that file
			// to our client's configuration
			try
			{
				cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

				CoreData.configJson = cfgjson;
				CoreData.DiscordToken = "REDACTED";
				CoreData.Prefix = cfgjson.CommandPrefix;
			}
			catch
			{
				NSJSUtil.Print("Failure to read settings...", ConsoleColor.Red);
				using (var fs = File.OpenRead("config.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					json = await sr.ReadToEndAsync();

				try
				{
					cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

					CoreData.configJson = cfgjson;
					CoreData.DiscordToken = "REDACTED";
					CoreData.Prefix = cfgjson.CommandPrefix;
				}
				catch
				{
					NSJSUtil.Print("FALIURE", ConsoleColor.Red);
					System.Environment.FailFast("FAILURE, NRE OCCURED! config file");
				}
			}

			return json;
		}

		#endregion

		#region Read Config File

		private static async Task<string> ReadConfigFile()
		{
			// first, let's load our configuration file
			string? json = "";
			try
			{
				NSJSUtil.Print("Attempting to open settings...", ConsoleColor.White);
				using (var fs = File.OpenRead("config.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					json = await sr.ReadToEndAsync();
			}
			catch
			{
				NSJSUtil.Print("Failure to load, creating...", ConsoleColor.Yellow);
				using (var fc = File.Create("config.json"))
					fc.Close();
				using (var fs = File.OpenRead("config.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					json = await sr.ReadToEndAsync();

				StringBuilder sb = new StringBuilder(); // important
				StringWriter sw = new StringWriter(sb);

				using (JsonWriter writer = new JsonTextWriter(sw))
				{
					writer.Formatting = Formatting.Indented;

					await writer.WriteStartObjectAsync();
					await writer.WritePropertyNameAsync("Token");
					await writer.WriteValueAsync("null");
					await writer.WritePropertyNameAsync("Prefix");
					await writer.WriteValueAsync("null");
					await writer.WriteEndObjectAsync();
				}

				using (var fs = File.OpenWrite("config.json"))
				using (var aasds = new StreamWriter(fs, new UTF8Encoding(false)))
					await aasds.WriteAsync(sb);

				using (var fs = File.OpenRead("config.json"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
					json = await sr.ReadToEndAsync();

			}

			return json;
		}

		#endregion

		#region  Bot Events

		private async Task Slash_CommandErrord(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
		{
			// let's log the error details
			args.Context.Client.Logger.LogError(BotEventId, $"{args.Context.User.Username} tried executing '{args.Context.CommandName ?? "<unknown command>"}' but it errored: {args.Exception.GetType()}: {args.Exception.Message ?? "<no message>"}", DateTime.Now);



			// made error checking for slash commands.
			if (args.Exception is SlashExecutionChecksFailedException ex)
			{

				foreach (var check in ex.FailedChecks)
				{
					if (check is SlashRequireOwnerAttribute)
					{
						string listOfOwners = "";
						foreach (var owner in Client.CurrentApplication.Owners)
						{
							listOfOwners += $"<@{owner.Id}> ";
						}

						await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Only {listOfOwners}can run this command!"));
					}
					else if (check is SlashRequireGuildAttribute)
					{
						await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Can't run this command here!"));
					}
					else if (check is SlashRequirePermissionsAttribute)
					{
						await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Missing required permissions!"));
					}
					else
					{
						await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Generic Error!"));
					}
				}

				// yes, the user lacks required permissions, 
				// let them know

				//var emoji = DiscordEmoji.FromName(args.Context.Client, ":no_entry:");

				//// let's wrap the response into an embed
				//var embed = new DiscordEmbedBuilder
				//{
				//    Title = "Access denied",
				//    Description = $"{emoji} You do not have the permissions required to execute this command.", //{e.Exception.InnerException}
				//    Color = new DiscordColor(0xFF0000) // red
				//};
				//await args.Context.CreateResponseAsync(embed);
			}
			else if (args.Exception is CommandNotFoundException)
			{
				var emoji = DiscordEmoji.FromName(args.Context.Client, ":shrug:");

				var embed = new DiscordEmbedBuilder
				{
					Title = "Unkown command",
					Description = $"{emoji} I do not reconise this command. perhaps you typed it wrong?", //{e.Exception.InnerException}
					Color = new DiscordColor(0xF0FC03)
				};
				await args.Context.CreateResponseAsync(embed);
			}
		}

		private Task Slash_Invoked(SlashCommandsExtension sender, SlashCommandInvokedEventArgs args)
		{
			args.Context.Client.Logger.LogInformation(BotEventId, $"{args.Context.User.Username} successfully executed '{args.Context.QualifiedName}'");

			return Task.CompletedTask;
		}

		private Task SockClosed(DiscordClient sender, SocketCloseEventArgs args)
		{
			Connection = false;
			sender.Logger.LogError(BotEventId, "Socket Closed");

			return Task.CompletedTask;
		}

		private Task SockError(DiscordClient sender, SocketErrorEventArgs args)
		{
			Connection = false;
			sender.Logger.LogError(BotEventId, "Failed to connect to socket");

			return Task.CompletedTask;
		}

		private Task UnkownError(DiscordClient sender, UnknownEventArgs args)
		{
			sender.Logger.LogError(BotEventId, args.EventName);
			return Task.CompletedTask;
		}

		private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
		{
			// let's log the fact that this event occured
			sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
			Connection = true;

			// since this method is not async, let's return
			// a completed task, so that no additional work
			// is done
			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
		{
			// let's log the name of the guild that was just
			// sent to our client

			sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

			// since this method is not async, let's return
			// a completed task, so that no additional work
			// is done
			return Task.CompletedTask;
		}

		private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
		{
			// let's log the details of the error that just 
			// occured in our client
			sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

			// since this method is not async, let's return
			// a completed task, so that no additional work
			// is done
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
		{
			// let's log the name of the command and user
			e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

			// since this method is not async, let's return
			// a completed task, so that no additional work
			// is done
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
		{
			// let's log the error details
			e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

			// let's check if the error is a result of lack
			// of required permissions
			if (e.Exception is ChecksFailedException)
			{
				// yes, the user lacks required permissions, 
				// let them know

				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// let's wrap the response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.", //{e.Exception.InnerException}
					Color = new DiscordColor(0xFF0000) // red
				};
				await e.Context.RespondAsync(embed);
			}
			else if (e.Exception is CommandNotFoundException)
			{
				var emoji = DiscordEmoji.FromName(e.Context.Client, ":shrug:");

				var embed = new DiscordEmbedBuilder
				{
					Title = "Unkown command",
					Description = $"{emoji} I do not reconise this command. perhaps you typed it wrong?", //{e.Exception.InnerException}
					Color = new DiscordColor(0xF0FC03)
				};
				await e.Context.RespondAsync(embed);
			}
		}

		#endregion
	}

	#region Json stuff

	// this structure will hold data from config.json
	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
	}

	public struct StoreJson
	{
		[JsonProperty("message")]
		public List<string> Message { get; set; }

		[JsonProperty("time")]
		public List<DateTime> Time { get; set; }

		[JsonProperty("channelid")]
		public List<ulong> ChannelID { get; set; }
	}

	public struct VersionJson
	{
		[JsonProperty("version")]
		public string Version { get; set; }
	}
	#endregion
}