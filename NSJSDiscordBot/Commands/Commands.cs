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

namespace NSJSDiscordBot.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {   
        [SlashCommand("help", "get some help")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("use !help"));
        }

        [SlashCommand("ban", "This bans a user, as long you have the permissions")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
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

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (!member.IsBot) members++;
            }

            foreach (DiscordMember member in DG.Members.Values)
            {
                if (member.Roles.Contains(DG.GetRole(983400691777294396))) staffCount++;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Number of members",
                Description = $"Current Members: {members}\nCurrent Bots: {DG.MemberCount - members}\nCurrent Staff: {staffCount}"
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
                if (member.IsDeafened) punishments += "User is meaden, ";
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
