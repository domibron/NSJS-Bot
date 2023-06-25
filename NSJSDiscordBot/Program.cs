// NSJS Bot for Discord


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

namespace NSJSDiscordBot
{

    public static class CoreData
    {
        public static ConfigJson configJson { get; set; }
        public static string DiscordToken = "YOUR TOKEN HERE";
        public static string Prefix = "PREFIX";
    }

    public class StoreData
    {
        public static List<string> messages = new List<string>();
        public static List<TimeSpan?> times = new List<TimeSpan?>();
        public static InteractionContext lctx = new InteractionContext();

        public static void StoreValue(string msg, TimeSpan? timespan)
        {
            Program.storejson.Time.Add(timespan);
            Program.storejson.Message.Add(msg);
            Console.WriteLine(Program.storejson.Message[1] + " " + Program.storejson.Time[1]);
        }
    }

    public class Program
    {
        public readonly EventId BotEventId = new EventId(42, "Bot-Ex01");

        public DiscordClient Client { get; set; }

        public CommandsNextExtension Commands { get; set; }

        public SlashCommandsExtension Slash { get; set; }

        public DiscordConfiguration cfg;

        public ConfigJson cfgjson;

        public static StoreJson storejson;

        //public static CoreData coreData = new CoreData();

        public static void Main(string[] args)
        {
            // since we cannot make the entry method asynchronous,
            // let's pass the execution to asynchronous code
            var prog = new Program();
            prog.Update();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        public async void Update()
        {
            var json = "";
            try
            {
                Console.WriteLine("ATTEMPTING TO OPEN STORE...");
                using (var fs = File.OpenRead("store.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();
            }
            catch
            {
                Console.WriteLine("FALIURE IN UPDATE!");
                using (var fc = File.Create("store.json"))
                    fc.Close();
                using (var fs = File.OpenRead("store.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                StringBuilder strb = new StringBuilder(); //<-- important for writing to file
                StringWriter strw = new StringWriter(strb);

                using (JsonWriter writer = new JsonTextWriter(strw))
                {
                    writer.Formatting = Formatting.Indented;

                    await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("Message");
                    await writer.WriteStartArrayAsync();
                    await writer.WriteValueAsync("A MESSAGE");
                    await writer.WriteEndAsync();
                    await writer.WritePropertyNameAsync("Time");
                    await writer.WriteStartArrayAsync();
                    await writer.WriteValueAsync("11:11:00");
                    await writer.WriteEndAsync();
                    await writer.WriteEndObjectAsync();
                }

                using (var fs = File.OpenWrite("store.json"))
                using (var aasds = new StreamWriter(fs, new UTF8Encoding(false)))
                    await aasds.WriteAsync(strb);

                using (var fs = File.OpenRead("store.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();
            }

            try
            {
                Console.WriteLine("ATTEMPTING TO STORE STORE...");
                storejson = JsonConvert.DeserializeObject<StoreJson>(json);
            }
            catch
            {
                Console.WriteLine("FALIURE ATTEMPTING TO STORE STORE...");
                using (var fs = File.OpenRead("store.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();

                //storejson = JsonConvert.DeserializeObject<StoreJson>(json);

            }

            try
            {
                StoreData.times = storejson.Time;
                StoreData.messages = storejson.Message;
            }
            catch 
            {
                Console.WriteLine("UH OH");
                System.Environment.FailFast("FAILURE AT STORE.JSON");
            }

            while (true)
            {
                for (int i = 0; i < storejson.Time.Count; i++)
                {
                    if (storejson.Time[i].Value.Hours == DateTime.Now.TimeOfDay.Hours && storejson.Time[i].Value.Minutes == DateTime.Now.TimeOfDay.Minutes)
                    {
                        DiscordChannel dc = await Client.GetChannelAsync(994362031488643204);
                        await Client.SendMessageAsync(dc, storejson.Message[i]);

                        // remove from data
                        TimeSpan? timeSpanToRemove = storejson.Time[i];
                        string messageToRemove = storejson.Message[i];
                        storejson.Time.Remove(timeSpanToRemove);
                        storejson.Message.Remove(messageToRemove);

                    }
                    //try // I am calling data that is not there, should immidetly back out after deletion.
                    //{
                    //    Console.WriteLine(storejson.Time[i].Value + " vs " + DateTime.Now.TimeOfDay);
                    //}
                    //catch(ArgumentOutOfRangeException)
                    //{
                    //    Console.WriteLine("Ill go fuck my self then");
                    //}
                }



            }
        }

        public async Task RunBotAsync()
        {
            // first, let's load our configuration file
            var json = "";
            try
            {
                Console.WriteLine("Attempting to open settings...");
                using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();
            }
            catch
            {
                Console.WriteLine("Failure to load, creating...");
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
                Console.WriteLine("Failure to read...");
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
                    Console.WriteLine("FALIURE");
                    System.Environment.FailFast("FAILURE, NRE OCCURED! saved your comuper :)");
                }
            }

            Console.WriteLine("NO FALIURE");


            cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,

                Intents = DiscordIntents.All
            };

            Console.WriteLine("SUCCESS");

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
                EnableDms = true,

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

            // let's hook some command events, so we know what's 
            // going on
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

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

            Console.WriteLine("BOT READY?");

            // and this is to prevent premature quitting
            await Task.Delay(-1);

            Console.WriteLine("YES");
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

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
    }

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
        public List<string> Message { get; private set; }

        [JsonProperty("time")]
        public List<TimeSpan?> Time { get; private set; }
    }
}