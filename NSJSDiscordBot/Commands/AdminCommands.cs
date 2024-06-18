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
using DSharpPlus.VoiceNext;
using System.Security;
using System.Net.Http.Headers;
using NSJSDiscordBot.Attributes;

namespace NSJSDiscordBot.Commands
{

	#region Ungroupped Commands

	#endregion

	#region Admin Commands

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

	#endregion
}
