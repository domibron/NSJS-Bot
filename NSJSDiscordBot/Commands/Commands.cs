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
                var emoji = DiscordEmoji.FromName(ctx.Client, ":banhammer:");

                // and respond with it.
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(emoji));
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

        [Command("ban"), Description("Bans a user with optional reason"), RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task Test(CommandContext ctx, DiscordMember member, [RemainingText] string reason)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                // audit logs who made the ban.
                await member.ModifyAsync(x =>
                {
                    x.AuditLogReason = $"Member banned {member.Username} by {ctx.User.Username} ({ctx.User.Id}) for the reason {reason}.";
                });

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

        [Command("info")]
        [Description("Get the info for a user, such as permissions")]
        public async Task Info(CommandContext ctx, [Description("member to look up.")] DiscordMember member)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = member.DisplayName,
                Description = "permissions: \n" + member.Permissions + "\n" + member.JoinedAt,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = member.AvatarUrl
                }
            };

            await ctx.RespondAsync(builder);
        }
        [Command("info"), RequireGuild]
        [Description("Get the info for a user, such as permissions")]
        public async Task Info(CommandContext ctx)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = ctx.Member.DisplayName,
                Description = "permissions: \n" + ctx.Member.Permissions + "\n" + ctx.Member.JoinedAt,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = ctx.Member.AvatarUrl
                }
            };

            await ctx.RespondAsync(builder);
        }
    }

    //[Group("admin")] // let's mark this class as a command group
    //[Description("Administrative commands.")] // give it a description for help purposes
    [Hidden] // let's hide this from the eyes of curious users
    [RequirePermissions(Permissions.ManageGuild)] // and restrict this to users who have appropriate permissions
    public class AdminCommands : BaseCommandModule
    {
        // all the commands will need to be executed as <prefix>admin <command> <arguments>

        // this command will be only executable by the bot's owner
        [Command("sudo"), Description("Executes a command as another user."), Hidden, RequireOwner]
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
