using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangBot
{
    public class InteractiblePage
    {
        public ulong MessageID { get; set; }
        public EmbedBuilder Embed { get; set; }
        public int CurrentPage { get; set; }

        private Dictionary<Emoji, Action> emoteActions;

        public InteractiblePage()
        {

        }

        public void AddEmoteAction(Emoji emote, Action action)
        {
            emoteActions.Add(emote, action);
        }
    }
}
