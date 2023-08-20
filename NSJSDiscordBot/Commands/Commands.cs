using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

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
using System.Security;
using System.Net.Http.Headers;
using DSharpPlus.SlashCommands.Attributes;

namespace NSJSDiscordBot.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("allbots", "Test the bot")]
        public async Task AllBots(InteractionContext ctx)
        {
            DiscordGuild DG = ctx.Guild;
            int members = 0;
            int staffCount = 0;

            string bots = "";

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (member.IsBot) {
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Pong!"));
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
            if (time  == null)
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

    public class UngrouppedCommands : BaseCommandModule
    {
        [Command("random")]
        public async Task RandomCommand(CommandContext ctx, int min, int max)
        {
            var random = new Random();
            await ctx.RespondAsync($"Your number is: {random.Next(min, max)}");
        }

        [Command("memberCount")]
        [RequireGuild]
        public async Task MemberCount(CommandContext ctx)
        {
            DiscordGuild DG = ctx.Guild;
            int members = 0;
            int staffCount = 0;
            int botcount = 0;

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (!member.IsBot) members++;
                else botcount++;
            }

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (member.Roles.Contains(DG.GetRole(983400691777294396))) staffCount++;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Number of members",
                Description = $"Current Members: {members}\nCurrent Bots: {botcount}\nCurrent Staff: {staffCount}"
            };

            await ctx.RespondAsync(embed);
        }

        [Command("ban"), RequireGuild, Description("Bans a user with optional reason"), RequirePermissions(Permissions.BanMembers)]
        public async Task Test(CommandContext ctx, DiscordMember member, [RemainingText] string reason)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                bool isStaff = false;
                foreach (DiscordRole role in member.Roles)
                {
                    switch (role.Id)
                    {
                        default:
                            break;
                        case 983400691777294396:
                            isStaff = true;
                            break;
                        case 981206652927742003:
                            isStaff = true;
                            break;
                    }
                }
                if (ctx.Member == member || member.Id == 1009152549389082696) // safe guard to pervent self banning and bot banning. bot id 1009152549389082696.
                {
                    await ctx.Channel.SendMessageAsync("You cannot ban me or yourself");
                    await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync($"**Attention!**\n{ctx.Member.Mention} ({ctx.Member.Id})" +
                        $"\nTried to ban:\n{member.Mention} ({member.Id})\n\n**Faluire:**\nattempted to ban themselfs or the bot");
                    return;
                }
                else if (isStaff && (ctx.Member.Hierarchy <= member.Hierarchy) && member.Permissions == Permissions.All) // if the mentioned user is staff and the message
                                                                                                                         // sender is a lower hierarchywhile they have all permissions
                                                                                                                         // then the ban will be void.
                {
                    await ctx.Channel.SendMessageAsync("This is a staff member! (Aministrator overide: failed! You lack the hiarchy posision)");
                    await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync($"**Attention!**\n{ctx.Member.Mention} ({ctx.Member.Id})" +
                        $"\nTried to ban:\n{member.Mention} ({member.Id})\n\n**Faluire:**\nHiarchy permission is lower or equal to Member");
                    return;
                }
                else if (isStaff && member.Permissions == Permissions.All) // if the mentioned user is staff and the message sender has all perms then the user will be banned.
                {
                    await ctx.Channel.SendMessageAsync("This is a staff member! (Aministrator overide: user will be banned)");
                }
                else if (isStaff && member.Permissions != Permissions.All) // if the mentioned user is staff and they do not have all permissions the user cannot be banned.
                {
                    await ctx.Channel.SendMessageAsync("This is a staff member! You lack the permissions to ban the user");
                    await ctx.Guild.GetChannel(1008824886678016001).SendMessageAsync($"**Attention!**\n{ctx.Member.Mention} ({ctx.Member.Id})" +
                        $"\nTried to ban:\n{member.Mention} ({member.Id})\n\n**Faluire:**\nNot an Administrator");
                    return;
                }
                else if (!isStaff) // if the @user is not staff let them know they will be banned.
                {
                    await ctx.Channel.SendMessageAsync("User will be banned!");
                }


                // audit logs who made the ban.
                await member.ModifyAsync(x =>
                {
                    x.AuditLogReason = $"Member banned {member.Username} by {ctx.User.Username} ({ctx.User.Id}) for the reason {reason}.";
                });

                if (reason == null) reason = "**No Reason Specified**";

                //await ctx.Guild.BanMemberAsync(member.Id, 0, reason);
                await ctx.RespondAsync("Banning is disabled");

                //creates a embed for the ban to be logged.
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = member.AvatarUrl
                    },
                    Title = $"User was banned: {member.Username} ({member.Id})",
                    Description = ( $"Banned User: {member.Username}  id: {member.Id}" +
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
                var emoji = DiscordEmoji.FromName(ctx.Client, ":banhammer:");

                // and respond with it.
                await ctx.RespondAsync(emoji);
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji);
            }
        }

        [Command("info"), RequireGuild, Description("Get the info for a user, such as permissions")]
        public async Task Info(CommandContext ctx, [Description("member to look up.")] DiscordMember member)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                string mention = "";
                foreach (DiscordRole role in member.Roles)
                    mention += role.Mention + ", ";

                bool isStaff = false;
                foreach (DiscordRole role in member.Roles)
                {
                    switch (role.Id)
                    {
                        default:
                            break;
                        case 983400691777294396:
                            isStaff = true;
                            break;
                        case 981206652927742003:
                            isStaff = true;
                            break;
                    }
                }

                bool isMember = false;
                foreach (DiscordRole role in member.Roles)
                    if (role.Id == 998906377755959346) isMember = true;

                bool isTicketSupport = false;
                foreach (DiscordRole role in member.Roles)
                    if (role.Id == 1009117505488433234) isTicketSupport = true;

                string punishments = "";
                foreach (DiscordRole role in member.Roles)
                {
                    switch (role.Id)
                    {
                        default:
                            punishments += "";
                            break;
                        case 1008014112900530307:
                            punishments += "User is banned from making tickets, ";
                            break;
                        case 1009085467716767814:
                            punishments += $"User is muted with {ctx.Guild.GetRole(1009085467716767814).Mention}, ";
                            break;
                    }
                }
                if (member.IsMuted) punishments += "User is server muted, ";
                if (member.IsDeafened) punishments += "User is server deafened, ";
                if (punishments == "") punishments += "No current punishment(s); ";

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName} ({member.Id})",
                    Color = member.Color,
                    Description = $"**permissions:** \n{member.Permissions} \n\n**Roles:** \n{mention}\n\n**bools:** \nStaff: {isStaff}, Member: {isMember}, Robot: {member.IsBot}, " +
                    $"Ticket Support: {isTicketSupport}" +
                    $"\n\n**Punishments:** {punishments}",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = member.AvatarUrl
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Joined at: {member.JoinedAt}"
                    }
                };

                await ctx.RespondAsync(builder);
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji);
            }
        }
    }

    [Group("admin")] // let's mark this class as a command group
    [Description("Administrative commands.")] // give it a description for help purposes
    //[Hidden] // let's hide this from the eyes of curious users
    [RequirePermissions(Permissions.ManageGuild)] // and restrict this to users who have appropriate permissions
    public class AdminCommands : BaseCommandModule
    {
        // all the commands will need to be executed as <prefix>admin <command> <arguments>

        // this command will be only executable by the bot's owner
        [Command("sudo"), Description("Executes a command as another user."), RequireOwner]
        public async Task Sudo(CommandContext ctx, [Description("Member to execute as.")] DiscordMember member, [RemainingText, Description("Command text to execute.")] string command)
        {
            // note the [RemainingText] attribute on the argument.
            // it will capture all the text passed to the command

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // get the command service, we need this for
            // sudo purposes
            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);

            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // and perform the sudo
            await cmds.ExecuteCommandAsync(fakeContext);
        }

        [Command("nick"), Description("Gives someone a new nickname."), RequirePermissions(Permissions.ManageNicknames), Aliases("nickname")]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The nickname to give to that user.")] string new_nickname)
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            try
            {
                // let's change the nickname, and tell the 
                // audit logs who did it.
                await member.ModifyAsync(x =>
                {
                    x.Nickname = new_nickname;
                    x.AuditLogReason = $"Changed by {ctx.User.Username} ({ctx.User.Id}).";
                });

                // let's make a simple response.
                var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");

                // and respond with it.
                await ctx.RespondAsync(emoji);
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji);
            }
        }
    }
}
