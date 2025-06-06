using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.Attributes
{
	/*
	 * I have no clue on what is going on, well some clue but its sooooo complicated.
	 */

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class NSJSSlashRequireOwnerAttribute : SlashCheckBaseAttribute
	{
		// so its a task that is a bool, that checks wha....
		public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
		{
			// huh...
			InteractionContext ctx2 = ctx;
			DiscordApplication currentApplication = ctx2.Client.CurrentApplication;
			DiscordUser currentUser = ctx2.Client.CurrentUser;
			if (!(currentApplication != null))
			{
				return Task.FromResult(ctx2.User.Id == currentUser.Id);
			}
			// if the id matches and code owners.
			return Task.FromResult(currentApplication.Owners.Any((DiscordUser x) => x.Id == ctx2.User.Id));
		}
	}

	//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	//public sealed class RequireOwnerAttribute : CheckBaseAttribute
	//{
	//    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	//    {
	//        CommandContext ctx2 = ctx;
	//        DiscordApplication currentApplication = ctx2.Client.CurrentApplication;
	//        DiscordUser currentUser = ctx2.Client.CurrentUser;
	//        if (!(currentApplication != null))
	//        {
	//            return Task.FromResult(ctx2.User.Id == currentUser.Id);
	//        }

	//        return Task.FromResult(currentApplication.Owners.Any((DiscordUser x) => x.Id == ctx2.User.Id));
	//    }
	//}
}
