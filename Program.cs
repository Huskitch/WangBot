using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace WangBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();

        public static DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private BotConfig config;

        public async Task RunBot()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            if (!File.Exists("config.json"))
            {
                config = new BotConfig()
                {
                    prefix = "!",
                    token = "",
                    game = ""
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            }

            string botToken = config.token;

            _client.Log += Log;

            await RegisterCommandsAsync();

            _client.UserJoined += OnUserJoin;
            _client.MessageReceived += OnMessageRecieved;
            _client.ReactionAdded += InteractiblePageManager.OnReactionAdded;

            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            await _client.SetGameAsync(config.game);

            await Task.Delay(-1);
        }

        private async Task OnMessageRecieved(SocketMessage arg)
        {
            if ((arg.Content.Contains("wang") || arg.Content.Contains("yiren")) && arg.Content.Contains("cute") && !arg.Author.IsBot)
            {
                SocketGuild guild = ((SocketGuildChannel)arg.Channel).Guild;
                IEmote emote = guild.Emotes.First(e => e.Name == "wangry2");
                await arg.Channel.SendMessageAsync($"{emote}");
            }

            if ((arg.Content.Contains("wang")) && !arg.Content.Contains("cute") && !arg.Author.IsBot)
            {
                SocketGuild guild = ((SocketGuildChannel)arg.Channel).Guild;
                IEmote emote = guild.Emotes.First(e => e.Name == "aFatWang");
                await arg.Channel.SendMessageAsync($"{emote}");
            } 
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task OnUserJoin(SocketGuildUser user)
        {
            Console.WriteLine("Checking if server config exists");
            if (File.Exists(user.Guild.Id.ToString() + ".json"))
            {
                try
                {
                    Console.WriteLine("Server config exists");
                    var joinRole = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(user.Guild.Id.ToString() + ".json")).joinRole;
                    var role = user.Guild.Roles.FirstOrDefault(x => x.Name.ToString() == joinRole);
                    await (user as IGuildUser).AddRoleAsync(role);
                } catch (Exception e)
                {

                }

                try
                {
                    var memberCountChannel = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(user.Guild.Id.ToString() + ".json")).memberCountChannel;
                    var channel = user.Guild.VoiceChannels.FirstOrDefault(x => x.Id == memberCountChannel);
                    await channel.ModifyAsync(m => { m.Name = "🗿 Member Count: " + user.Guild.MemberCount; });
                }
                catch (Exception e)
                {

                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            string messageLower = arg.Content.ToLower();
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;
            int argumentPos = 0; 
            if (message.HasStringPrefix(config.prefix, ref argumentPos) || message.HasMentionPrefix(_client.CurrentUser, ref argumentPos))
            {
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argumentPos, _services);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                    await message.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }

    class BotConfig
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string game { get; set; }
    }
}
