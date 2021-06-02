using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Messages : BaseCommandModule
    {
        [Command("delall")]
        [Description("Deletes all messages in the current channel")]
        [Aliases("rm")]
        [RequireOwner()]
        [RequireGuild()]
        public async Task DelAll(CommandContext ctx, [Description("number of lines to delete")] int numberOfLines)
        {
            await ctx.TriggerTypingAsync();
            string message = ctx.Message.Content.ToString();
            await ctx.Message.DeleteAsync();

            var msgTask = ctx.Channel.GetMessagesAsync(numberOfLines);
            await msgTask;

            var msgs = msgTask.Result;
            await ctx.Channel.DeleteMessagesAsync(msgs);

            var doneMsg = ctx.RespondAsync("Done.");
            await doneMsg;

            System.Threading.Thread.Sleep(2000);
            await doneMsg.Result.DeleteAsync();
        }
    }
}
