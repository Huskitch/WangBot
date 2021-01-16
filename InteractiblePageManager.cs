using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WangBot
{
    public static class InteractiblePageManager
    {
        private static List<InteractiblePage> pages;

        public static void AddPage(InteractiblePage page)
        {
            pages.Add(page);
        }

        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var page = pages.Single(p => p.MessageID == message.Id);
        }

        public static InteractiblePage BuildHelpPage()
        {
            var commandType = new Commands();
            var methods = commandType.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();
            var methodAttributes = methods[0].GetCustomAttributes();

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Default Command List");
            builder.WithThumbnailUrl("https://cdn.discordapp.com/emojis/779798986659594271.gif");
            builder.WithColor(Color.Purple);
            builder.WithFooter(footer => footer.Text = "WangBot v0.1").WithCurrentTimestamp();

            var page = new InteractiblePage();
            page.Embed = builder;

            var leftEmote = new Emoji("");
            Action leftAction = () =>
            {

            };

            page.AddEmoteAction(leftEmote, leftAction);

            return page;
        }
    }
}
