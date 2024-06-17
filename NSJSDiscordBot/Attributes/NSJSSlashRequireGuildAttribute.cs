using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSJSDiscordBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NSJSSlashRequireGuildAttribute : SlashCheckBaseAttribute
    {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.FromResult(ctx.Guild != null);
        }
    }

    //AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    //public sealed class SlashRequireGuildAttribute : SlashCheckBaseAttribute
    //{
    //    //
    //    // Summary:
    //    //     Defines that this command is only usable within a guild.
    //    public SlashRequireGuildAttribute()
    //    {
    //    }

    //    //
    //    // Summary:
    //    //     Runs checks.
    //    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    //    {
    //        return Task.FromResult(ctx.Guild != null);
    //    }
    //}
}
