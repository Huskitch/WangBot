using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Wolfram.Alpha;
using Wolfram.Alpha.Models;

namespace WangBot
{
    public class ServerConfig
    {
        public ulong serverID { get; set; }
        public string joinRole { get; set; }
        public ulong emoteChannel { get; set; }
        public ulong memberCountChannel { get; set; }
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {
        ServerConfig config;

        [Command("emotes")]
        [Alias("listemotes", "emotelist")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Prints out the server emotes with a given spacing")]
        public async Task ListEmotes([Summary("Number of emotes before inserting a new line")]int spacing, int width = 0, bool showLatest = false)
        {
            ulong channel = 0;
            if (File.Exists(Context.Guild.Id.ToString() + ".json"))
            {
                channel = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(Context.Guild.Id.ToString() + ".json")).emoteChannel;

                var messages = await Context.Guild.GetTextChannel(channel).GetMessagesAsync(20).FlattenAsync();
                await ((ITextChannel)Context.Guild.GetTextChannel(channel)).DeleteMessagesAsync(messages);
            }
            else
            {
                await ReplyAsync("Set an emote channel first dumbo");
            }

            var emotes = Context.Guild.Emotes;
            var sentEmotes = emotes.OrderByDescending(x => x.Name).OrderBy(x => x.Animated).ToList();

            int count = 0;
            string emoteString = "";
            foreach (Emote emote in sentEmotes)
            {
                emoteString += $"{emote}";
                for (int i = 0; i < width; i++)
                    emoteString += " ";

                count++;
                if (count % spacing == 0)
                {
                    await Context.Guild.GetTextChannel(channel).SendMessageAsync(emoteString);
                    emoteString = "";
                    count = 0;
                }
            }

            await Context.Guild.GetTextChannel(channel).SendMessageAsync(emoteString);

            if (showLatest)
            {
                await Context.Guild.GetTextChannel(channel).SendMessageAsync("Latest added emotes:");

                var newEmotes = sentEmotes.OrderByDescending(x => x.CreatedAt);
                string newEmoteString = "";
                for (int i = 0; i < 3; i++)
                {
                    newEmoteString += $"{newEmotes.ElementAt(i)}";
                    for (int j = 0; j < width; j++)
                        newEmoteString += " ";
                }

                await Context.Guild.GetTextChannel(channel).SendMessageAsync(newEmoteString);
            }
            await ReplyAsync("Emote list updated");
        }

        [Command("ask")]
        public async Task AnswerQuery([Remainder]string query)
        {
            WolframAlphaService service = new WolframAlphaService("");
            WolframAlphaRequest request = new WolframAlphaRequest(query);
            WolframAlphaResult result = await service.Compute(request);

            string final = "";
            if (result.QueryResult.Pods != null)
                final = result.QueryResult.Pods[1].SubPods[0].Plaintext.Replace("Wolfram|Alpha", "wang");
            else
                final = "idk";

            if (final != null || final != "")
                await ReplyAsync(final);
            else
                await ReplyAsync("idk");
            //foreach (var pod in result.QueryResult.Pods)
            //{
            //    if (pod.SubPods != null)
            //    {
            //        await ReplyAsync(pod.Title);
            //        foreach (var subpod in pod.SubPods)
            //        {
            //            await ReplyAsync("    " + subpod.Plaintext);
            //        }
            //    }
            //}
        }

        [Command("setemotechannel")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task SetEmoteChannel(string channel)
        {
            if (!File.Exists(Context.Guild.Id.ToString() + ".json"))
            {
                config = new ServerConfig()
                {
                    serverID = Context.Guild.Id,
                    emoteChannel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channel).Id
                };
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(Context.Guild.Id.ToString() + ".json"));
                config.emoteChannel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channel).Id;
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            await ReplyAsync("Set emote update channel to **" + config.emoteChannel + "**");
        }

        [Command("av")]
        public async Task GetAvatar(string user = "")
        {
            if (user == "")
            {
                await ReplyAsync(Context.User.GetAvatarUrl());
            }
            else
            {
                var userObject = Context.Guild.Users.FirstOrDefault(x => user.Contains(x.Id.ToString()));
                await ReplyAsync(userObject.GetAvatarUrl());
            }

        }

        [Command("setmembercountchannel")]
        public async Task SetMemberCountChannel(ulong channel)
        {
            if (!File.Exists(Context.Guild.Id.ToString() + ".json"))
            {
                config = new ServerConfig()
                {
                    serverID = Context.Guild.Id,
                    memberCountChannel = Context.Guild.Channels.FirstOrDefault(x => x.Id == channel).Id
                };
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(Context.Guild.Id.ToString() + ".json"));
                config.memberCountChannel = Context.Guild.Channels.FirstOrDefault(x => x.Id == channel).Id;
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            await ReplyAsync("Set member count channel to **" + config.emoteChannel + "**");
        }

            //[Command("help")]
            //[Alias("helpage", "showhelp", "info", "information", "commands", "commandlist")]
            //[Summary("Shows the command information page")]
            //public async Task HelpPage()
            //{
            //    var builder = InteractiblePageManager.BuildHelpPage();
            //    var response = await Context.Channel.SendMessageAsync(embed: builder.Embed.Build());

            //    builder.MessageID = response.Id;
            //    InteractiblePageManager.AddPage(builder);
            //}

        [Command("setjoinrole")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Summary("Set the default role for new users")]
        public async Task SetJoinRole(string role)
        {
            if (!File.Exists(Context.Guild.Id.ToString() + ".json"))
            {
                config = new ServerConfig()
                {
                    serverID = Context.Guild.Id,
                    joinRole = role
                };
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(Context.Guild.Id.ToString() + ".json"));
                config.joinRole = role;
                File.WriteAllText(Context.Guild.Id.ToString() + ".json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            await ReplyAsync("Set new user role to **" + role + "**");
        }
    }
}
