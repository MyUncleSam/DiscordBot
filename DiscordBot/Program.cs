using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DiscordBot.Commands;
using DiscordBot.Enums;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Program
    {
        public readonly EventId BotEventId = new EventId(42, "WolfBot");

        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public ConfigJson Config { get; set; }
        private Timer VoiceTimer { get; set; }

        public static void Main(string[] args)
        {
            // since we cannot make the entry method asynchronous,
            // let's pass the execution to asynchronous code
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            System.IO.Directory.CreateDirectory("config");
            if (!System.IO.File.Exists("config/config.json"))
                System.IO.File.Copy("config.json", "config/config.json");

            // first, let's load our configuration file
            var json = "";
            using (var fs = File.OpenRead("config/config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            // next, let's load the values from that file
            // to our client's configuration
            Config = JsonConvert.DeserializeObject<ConfigJson>(json);
            var cfg = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };

            // then we want to instantiate our client
            this.Client = new DiscordClient(cfg);

            // next, let's hook some events, so we know
            // what's going on
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;

            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] { Config.CommandPrefix },

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            this.Commands = this.Client.UseCommandsNext(ccfg);

            // let's hook some command events, so we know what's going on
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;


            // let's add a converter for a custom type and a name
            //var mathopcvt = new MathOperationConverter();
            //Commands.RegisterConverter(mathopcvt);
            //Commands.RegisterUserFriendlyTypeName<MathOperation>("operation");

            //// up next, let's register our commands
            this.Commands.RegisterCommands<Admin>();
            this.Commands.RegisterCommands<Public>();
            this.Commands.RegisterCommands<Messages>();
            this.Commands.RegisterCommands<Roles>();

            this.Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            //var cmds = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(s => s.GetTypes())
            //    .Where(w => typeof(ICommands).IsAssignableFrom(w) && !w.IsInterface);

            //var test = Activator.CreateInstance(cmds.First().GetType());

            //foreach (var cmd in cmds.Cast<Type>())
            //    this.Commands.RegisterCommands(cmd.GetType());

            // set up our custom help formatter
            this.Commands.SetHelpFormatter<SimpleHelpFormatter>();

            // finally, let's connect and log in
            await this.Client.ConnectAsync();

            // when the bot is running, try doing <prefix>help
            // to see the list of registered commands, and 
            // <prefix>help <command> to see help about specific
            // command.

            // start the timer for auto removal of old notification elements
            VoiceTimer = new Timer(30 * 60 * 1000); // check for not deleted messages (should be deleted directly - so just for security reasons)
            VoiceTimer.Elapsed += VoiceTimer_Elapsed;
            VoiceTimer.Enabled = true;
            VoiceTimer.AutoReset = true;

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

        private void VoiceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Config.DeleteVoiceMessageAfterMinutes <= 0)
                return;

            Client.Logger.LogInformation(BotEventId, "Checking for old voice notification entries...");

            foreach (DiscordGuild guild in Client.Guilds.Values)
            {
                Client.Logger.LogInformation(BotEventId, $"\tGuild: {guild.Name} ({guild.Id})");
                var chans = guild.Channels.Select(s => s.Value).Where(w => w.Type == ChannelType.Text && w.Name.Equals(Config.NotifyChannelName, StringComparison.OrdinalIgnoreCase));
                if (chans.Count() <= 0)
                    continue;

                // get last 10 messages from this channel, check if sent date is older than provided date and if so - delete them
                DateTime deleteOlderThan = DateTime.Now.AddMinutes(Config.DeleteVoiceMessageAfterMinutes * -1);
                foreach (DiscordChannel chan in chans)
                {
                    var msgs = chan.GetMessagesAsync(10).Result;
                    var msgsOlder = msgs.Where(w => w.Timestamp.DateTime <= deleteOlderThan);

                    if (msgsOlder.Count() > 0)
                        chan.DeleteMessagesAsync(msgsOlder);

                    Client.Logger.LogInformation(BotEventId, $"\t\t- #{chan.Name} ({chan.Id}): Deleting {msgsOlder.Count()} of {msgs.Count} messages");
                }
            }
        }

        private async Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            // search for the given channel name
            var chans = e.Guild.Channels.Select(s => s.Value).Where(w => w.Type == ChannelType.Text && w.Name.Equals(Config.NotifyChannelName, StringComparison.OrdinalIgnoreCase));
            if (chans.Count() <= 0)
                return;

            if (e.After?.Channel?.Id == e.Before?.Channel?.Id)
                return;

            VoiceJoinEnum action = VoiceJoinEnum.none;

            if (e.After?.Channel == null && e.Before?.Channel != null)
                action = VoiceJoinEnum.left;
            else if (e.After?.Channel != null && e.Before?.Channel != null)
                action = VoiceJoinEnum.moved;
            else if (e.After?.Channel != null && e.Before?.Channel == null)
                action = VoiceJoinEnum.joined;

            if(action == VoiceJoinEnum.none)
                return;

            string text;
            DiscordUser curUser = e.User;

            if (string.IsNullOrWhiteSpace(curUser.Username))
            {
                // in some cases the username is missing (not sent by api) - need to retrieve it using the api
                // but first check the cache
                curUser = e.Guild.GetMemberAsync(e.User.Id).Result; // DSharpPlus already caches, so no custom cache needed
                sender.Logger.LogInformation(BotEventId, $"Looked up user with id {e.User.Id}: {curUser.Username}");
            }

            switch (action)
            {
                case VoiceJoinEnum.joined:
                    text = $"'{curUser.Username}' joined '{e.After.Channel.Name}'";
                    break;
                case VoiceJoinEnum.left:
                    text = $"'{curUser.Username}' left '{e.Before.Channel.Name}'";
                    break;
                case VoiceJoinEnum.moved:
                    text = $"'{curUser.Username}' moved to '{e.After.Channel.Name}' (from '{e.Before.Channel.Name}')";
                    break;
                default:
                    sender.Logger.LogError(BotEventId, $"unknown voice join enum: '{action.ToString()}'");
                    return;
            }

            sender.Logger.LogInformation(BotEventId, text);

            foreach (DiscordChannel chan in chans)
            {
                var done = await chan.SendMessageAsync($"{chan.Mention}: {text}");
                var delLater = DeleteVoiceNotification(done);
            }
        }

        public async Task DeleteVoiceNotification(DiscordMessage message)
        {
            await Task.Delay(Config.DeleteVoiceMessageAfterMinutes * 60 * 1000);
            //System.Threading.Thread.Sleep(99999999);
            try
            {
                Client.Logger.LogInformation(BotEventId, $"Deleting message {message.Id}: '{message.Content}'");
            }
            catch
            {
                Client.Logger.LogInformation(BotEventId, $"Deleting message <unknown>");
            }
            await message.DeleteAsync();
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed);
                await e.Context.Message.DeleteAsync();
            }
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("notify_channelname")]
        public string NotifyChannelName { get; set; }

        [JsonProperty("delete_voice_message_after_minutes")]
        public int DeleteVoiceMessageAfterMinutes { get; set; }
    }
}
