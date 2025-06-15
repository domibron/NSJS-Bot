using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using NSJSDiscordBot.DataFiles.BotTokensAndKeys;
using NSJSDiscordBot.Discord.SlashCommands;

namespace NSJSDiscordBot.Discord
{
    public class BotCore
    {
        public readonly EventId BotEventId = new EventId(42, "NSJS Bot:");

        public static DiscordClient? NSJSDiscordClient;

        //private CommandsNextExtension? _commands;
        private SlashCommandsExtension? _slash;

        public async Task InitAndStartBotAsync()
        {

            DiscordConfiguration _discordConfiguration = new DiscordConfiguration()
            {
                // BAD as this required reading the file. there should be a thing to read the file and if its a null we detect it here. Not in file reader.
                Token = TokenAndKeysFileManager.TokenAndKeys.DiscordToken,
                //Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,

                Intents = DiscordIntents.All,
                AlwaysCacheMembers = true,

                // TEMP SOlution to drop out connection.
                ReconnectIndefinitely = true
            };

            NSJSDiscordClient = new DiscordClient(_discordConfiguration);

            // https://dsharpplus.github.io/DSharpPlus/articles/interactivity.html

            NSJSDiscordClient.UseInteractivity(new DSharpPlus.Interactivity.InteractivityConfiguration()
            {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30),
            });

            NSJSDiscordClient.Ready += ClientReady;
            NSJSDiscordClient.GuildAvailable += ClientGuildAvailable;
            NSJSDiscordClient.ClientErrored += ClientClientError;


            // regular commands, we dont want this...?
            //// up next, let's set up our commands
            //var ccfg = new CommandsNextConfiguration
            //{
            //    // let's use the string prefix defined in config.json
            //    StringPrefixes = new[] { "!" },

            //    // enable responding in direct messages
            //    EnableDms = false,

            //    // enable mentioning the bot as a command prefix
            //    EnableMentionPrefix = true
            //};

            //// and hook them up
            //_commands = NSJSDiscordClient.UseCommandsNext(ccfg);

            



            var slashConfig = new SlashCommandsConfiguration
            {
                Services = new ServiceCollection().AddSingleton<Random>().BuildServiceProvider()
            };

            _slash = NSJSDiscordClient.UseSlashCommands(slashConfig);

            // commands go here.
            //_slash.RegisterCommands<FeedBackSlashCommands>();
            _slash.RegisterCommands<NoCatCommands>();

            _slash.SlashCommandInvoked += SlashInvoked;
            _slash.SlashCommandErrored += SlashCommandErrord;


            await NSJSDiscordClient.ConnectAsync();


            await Task.Delay(-1);
        }

    

        private Task ClientReady(DiscordClient sender, ReadyEventArgs e)
        {

            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
            //Connection = true;

            return Task.CompletedTask;
        }

        private Task ClientGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            return Task.CompletedTask;
        }

        private Task ClientClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            return Task.CompletedTask;
        }





        private async Task SlashCommandErrord(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
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
                        foreach (var owner in NSJSDiscordClient.CurrentApplication.Owners)
                        {
                            listOfOwners += $"<@{owner.Id}> ";
                        }

                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()
                            .WithContent($"Only {listOfOwners}can run this command!"));
                    }
                    else if (check is SlashRequireGuildAttribute)
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()
                            .WithContent($"Can't run this command here! Try in a server."));
                    }
                    else if (check is SlashRequirePermissionsAttribute)
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()
                            .WithContent($"Missing required permissions!"));
                    }
                    else if (check is SlashRequireDirectMessageAttribute)
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()
                            .WithContent($"You can only do this in my DMs!"));
                    }
                    else
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()
                            .WithContent($"Unkown Error! {check.GetType()}"));
                    }
                }
            }
            else if (args.Exception is CommandNotFoundException)
            {
                var emoji = DiscordEmoji.FromName(args.Context.Client, ":shrug:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Unkown command",
                    Description = $"{emoji} I do not recognise this command. perhaps you typed it wrong?", //{e.Exception.InnerException}
                    Color = new DiscordColor(0xF0FC03)
                };
                await args.Context.CreateResponseAsync(embed, true);
            }
            else if (args.Exception is InvalidOperationException)
            {
                var emoji = DiscordEmoji.FromName(args.Context.Client, ":shrug:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Unkown command",
                    Description = $"{emoji} I do not reconise this command. perhaps you typed it wrong?", //{e.Exception.InnerException}
                    Color = new DiscordColor(0xF0FC03)
                };
                await args.Context.CreateResponseAsync(embed, true);
            }
        }

        private Task SlashInvoked(SlashCommandsExtension sender, SlashCommandInvokedEventArgs args)
        {
            args.Context.Client.Logger.LogInformation(BotEventId, $"{args.Context.User.Username} successfully executed '{args.Context.QualifiedName}'");

            return Task.CompletedTask;
        }

    }
}
