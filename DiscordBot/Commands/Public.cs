using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Public : BaseCommandModule
    {
        [Command("ping")]
        [Description("checks the bot ping to the discord server")]
        [Aliases("pong")]
        public async Task Ping(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(context.Client, ":ping_pong:");
            await context.RespondAsync($"{emoji} Pong! Ping: {context.Client.Ping}ms");
        }
    }
}
