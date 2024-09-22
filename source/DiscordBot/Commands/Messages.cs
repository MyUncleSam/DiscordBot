using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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

            var msgTask = await context.Channel.GetMessagesAsync(numberOfLines);
            await context.Channel.DeleteMessagesAsync(msgTask);

            var doneMsg = await context.RespondAsync("Done.");

            await Task.Delay(2000);
            await doneMsg.DeleteAsync();
        }
    }
}
