using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task DelAll(CommandContext context, [Description("number of lines to delete")] int numberOfLines)
        {
            await context.TriggerTypingAsync();
            string message = context.Message.Content.ToString();
            await context.Message.DeleteAsync();

            var msgTask = context.Channel.GetMessagesAsync(numberOfLines);
            await msgTask;

            var msgs = msgTask.Result;
            await context.Channel.DeleteMessagesAsync(msgs);

            var doneMsg = context.RespondAsync("Done.");
            await doneMsg;

            System.Threading.Thread.Sleep(2000);
            await doneMsg.Result.DeleteAsync();
        }
    }
}
