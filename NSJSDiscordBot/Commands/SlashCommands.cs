using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace NSJSDiscordBot.Commands
{
    #region Slash Commands
    public class SlashCommands : ApplicationCommandModule
    {
        // HUH???
        [SlashCommand("testAttribute", "tests the attribute"), SlashRequireGuild]
        public async Task TestAttribute(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("hi"));
        }

        [SlashCommand("allbots", "Test the bot")]
        public async Task AllBots(InteractionContext ctx)
        {
            DiscordGuild DG = ctx.Guild;
            int members = 0;
            int staffCount = 0;

            string bots = "";

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (member.IsBot)
                {
                    bots += " " + member.Nickname;
                }
            }

            //foreach (DiscordMember member in DG.Members.Values)
            //{
            //    if (member.Roles.Contains(DG.GetRole(983400691777294396))) staffCount++;
            //}

            //DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            //{
            //    Title = "Number of members",
            //    Description = $"Current Members: {members}\nCurrent Bots: {DG.MemberCount - members}\nCurrent Staff: {staffCount}"
            //};

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "All bots",
                Description = $"Current bots: {bots}"
            };

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("ping", "Test the bot")]
        public async Task Ping(InteractionContext ctx)
        {
            int ping = ctx.Client.Ping;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Pong! \n  took {ping}ms"));
        }

        [SlashCommand("help", "get some help")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"use {CoreData.Prefix}help"));

        }

        [SlashCommand("ban", "This bans a user, as long you have the permissions"), SlashRequireGuild, SlashRequirePermissions(Permissions.BanMembers)]
        public async Task BanCommand(InteractionContext ctx, [Option("user", "User to ban")] DiscordUser user,
            [Choice("None", 0)]
            [Choice("1 Day", 1)]
            [Choice("1 Week", 7)]
            [Option("deletedays", "Number of days of message history to delete")] long deleteDays = 0,
            [Option("reason", "reason of ban")] string reason = "**None specified**")
        {
            try
            {
                //await ctx.Guild.BanMemberAsync(member.Id, deleteDays, reason);

                //creates a embed for the ban to be logged.
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = user.AvatarUrl
                    },
                    Title = $"User was banned: {user.Username} ({user.Id})",
                    Description = ($"Banned User: {user.Username}  id: {user.Id}" +
                    $"\nReason for ban: {reason}" +
                    $"\nBanner User: {ctx.User.Username} id: {ctx.User.Id}"),
                    Timestamp = DateTime.UtcNow,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"time when banned: {DateTime.Now}"
                    }
                };

                // sends the embed to the logging channel of NSJS.
                await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync(embed);

                // let's make a simple response.
                //var emoji = DiscordEmoji.FromName(ctx.Client, ":banhammer:");

                // and respond with it.
                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(emoji));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("banning is disabled"));
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(emoji));
            }
        }

        // [Option("Hour", "The hour to be sent on")] int hour, [Option("Minute", "The minute to be sent on")] int minute

        [SlashCommand("TimedMessage", "Delay a message that is to be sent"), SlashRequireGuild, SlashRequireOwner]
        public async Task TimedMessage(InteractionContext ctx, [Option("Mesage", "The string message")] string message, [Option("Channel", "The channel to senf the message")] DiscordChannel discordChannel = null, [Option("Time", "Time for when the message is sent")] TimeSpan? time = null, [Option("Year", "The year to send the message")] long year = 0, [Option("Month", "The month to send the message")] long month = 0, [Option("Day", "The day to send the message")] long day = 0)
        {
            if (time == null)
            {
                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"error with the time, defualt value is 12:00:00"));
                //return;
            }

            bool pass = false;

            foreach (DiscordRole role in ctx.Member.Roles)
            {
                if (role.Id == 981206652927742003 || role.Id == 1002688931613114418 || role.Id == 983398522982383626)
                {
                    pass = true;
                }
            }

            if (!pass)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You are not a coordinator!"));
                return;
            }

            if (discordChannel == null)
            {
                discordChannel = ctx.Channel;
                Console.WriteLine(discordChannel.ToString());
            }
            else
            {
                Console.WriteLine(discordChannel.ToString());
            }

            if (year == 0 || year == null)
            {
                year = DateTime.Now.Year;
                Console.WriteLine(year.ToString());
            }

            if (month == 0 || month == null)
            {
                month = DateTime.Now.Month;
                Console.WriteLine(month.ToString());
            }

            if (day == 0 || day == null)
            {
                //day = DateTime.Now.Day;

                // fix to set the day to the one in advance

                DateTime ldt = DateTime.Now;
                ldt = ldt.AddDays(1);

                day = ldt.Day;

                Console.WriteLine(day.ToString());
            }

            DateTime dt;

            if (time == null)
            {
                dt = new((int)year, (int)month, (int)day, 12, 0, 0);
                Console.WriteLine(dt.ToString());
            }
            else
            {
                dt = new((int)year, (int)month, (int)day, time.Value.Hours, time.Value.Minutes, time.Value.Seconds);
                Console.WriteLine(dt.ToString());
            }

            if (DateTime.Compare(dt, DateTime.Now) > 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"message will be sent at {dt} in {discordChannel.Name}"));

                Console.WriteLine(dt.ToString());

                StoreData.StoreMessageAndTime(message, dt, discordChannel.Id);
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"the time you want to send the message is in the past! your time {dt}, current time {DateTime.Now}"));
            }
        }

        [SlashRequireGuild, SlashCommand("MemberCount", "Gets the current count of members")]
        public async Task MemberCount(InteractionContext ctx)
        {
            DiscordGuild DG = ctx.Guild;
            int members = 0;
            int staffCount = 0;
            int botcount = 0;

            // stops it running in DMs
            if (ctx.Guild == null || ctx.Guild.Id != 981206447004213258) return;

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (!member.IsBot) members++;
                else botcount++;
            }

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (member.Roles.Contains(DG.GetRole(1002689383473889371)) || member.Roles.Contains(DG.GetRole(1002689425404338228)))
                    staffCount++;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Number of members",
                Description = $"Current Members: {members}\nCurrent Bots: {botcount}\nCurrent Staff: {staffCount}"
            };

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("UpdateStore", "fore save of any data stored in cache"), SlashRequireGuild, SlashRequireOwner]
        public async Task UpdateStore(InteractionContext ctx)
        {
            StoreData.UpdateStoreFile();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"done"));
        }

        [SlashCommand("DropData", "drops all store data"), SlashRequireGuild, SlashRequireOwner]
        public async Task DropData(InteractionContext ctx)
        {
            if (!ctx.Member.IsOwner)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Unauthorized user!"));

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = ctx.Member.AvatarUrl
                    },
                    Title = $"User attempted to drop data: {ctx.Member.Username} ({ctx.Member.Id})",
                    Description = ($"user: {ctx.Member.Username}  id: {ctx.Member.Id}" +
                    $"\nKeep an eye out"),
                    Timestamp = DateTime.UtcNow,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"time when command was ran: {DateTime.Now}"
                    }
                };

                // sends the embed to the logging channel of NSJS.
                await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync(embed);

                return;
            }

            StoreData.DropStore();

            DiscordEmbedBuilder embedO = new DiscordEmbedBuilder
            {
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = ctx.Member.AvatarUrl
                },
                Title = $"**User successfully dropped the data**: {ctx.Member.Username} ({ctx.Member.Id})",
                Description = ($"user: {ctx.Member.Username}  id: {ctx.Member.Id}" +
                $"\nAll data is lost!"),
                Timestamp = DateTime.UtcNow,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"time when command was ran: {DateTime.Now}"
                }
            };

            // sends the embed to the logging channel of NSJS.
            await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync(embedO);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Done, hope you ment to do that!"));
        }
    }

    #endregion
}
