using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Collections.ObjectModel;
using Nekos.Net;
using Nekos.Net.Responses;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Dynamic;

namespace DiscordBot.Modules
{

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("roll")]
        [Alias("dice", "кубики", "кости")]
        private async Task RollDices(int faces=6, int count=1)
        {
            if(count <= 5)
            {
                EmbedBuilder DiceEmbed = new EmbedBuilder().WithColor(Color.Purple)
                    .WithImageUrl(@"https://gilkalai.files.wordpress.com/2017/09/dice.png?w=640").WithTitle("Результаты: ").WithTimestamp(DateTimeOffset.Now)
                    .WithAuthor(Context.User.Username).WithThumbnailUrl(Context.User.GetAvatarUrl(size: 64));
                for (int i = 0; i < count; i++)
                {
                    Random random = new Random();

                    DiceEmbed.AddField($":game_die: Бросок {i + 1}:", random.Next(1, faces));
                }
                await ReplyAsync(embed: DiceEmbed.Build());
            }
            else
            {
                await ReplyAsync("Увы, но вы максимальное количество бросков - 5");
            }
        }

        [Command("uinfo")]
        [Alias("user")]
        private async Task GetUserInfo(IUser user)
        {
            EmbedBuilder userEmbed = new EmbedBuilder().WithAuthor(user).WithImageUrl(user.GetAvatarUrl()).WithDescription(user.Status.ToString()).WithTimestamp(DateTimeOffset.Now);
            await ReplyAsync("", embed: userEmbed.Build());   
        }
    }
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Alias("Бан")]
        public async Task BanAsync(IUser user, [Remainder] string reason)
        {
            EmbedBuilder banembed = new EmbedBuilder();
            banembed.WithAuthor(Context.User.Username);
            banembed.WithColor(Color.Purple);
            banembed.WithDescription($"Администратор {Context.User.Mention} забанил **{user.Username}** по причине **{reason}**");
            await Context.Guild.AddBanAsync(user, 0, reason);
            await ReplyAsync("", false, banembed.Build());
        }
    }

    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        public void ClearDuel()
        {
            Program.lastDueledUser = null;
            Program.lastDuelEnemy = null;
            Program.lastDueledUserChoise = null;
            Program.lastDuelEnemyChoise = null;
        }
        public string GetEmoji(string result)
        {
            switch (result.ToLower())
            {
                case "камень":
                    return result += " :rock:";
                case "ножницы":
                    return result += " :skissors:";
                case "бумага":
                    return result += " :newspaper:";
            }
            return "";
        }
        [Command("RPC", RunMode=RunMode.Async)]
        [Alias("кнб")]
        public async Task RPC(string ChosenItem)
        {
            ChosenItem = ChosenItem.Substring(0, 1).ToUpper() + ChosenItem.Substring(1);

            EmbedBuilder resultEmbed = new EmbedBuilder().WithTitle("Результаты");
            Random rand = new Random();
            dynamic BotChosenItem = rand.Next(1, 4);
            string result = "";
            switch (BotChosenItem)
            {
                case 1:
                    result = "Камень :rock:";
                    break;
                case 2:
                    result = "Ножницы :scissors: ";
                    break;
                case 3:
                    result = "Бумага :newspaper: ";
                    break;

            }
            if(ChosenItem.ToLower() == result.Split(':')[0].ToLower().Trim())
            {
                resultEmbed.AddField(new EmbedFieldBuilder().WithName("Выбор бота: ").WithValue(result));
                resultEmbed.AddField(new EmbedFieldBuilder().WithName("Ваш выбор: ").WithValue(GetEmoji(ChosenItem)));
                await ReplyAsync(embed: resultEmbed.WithDescription("Ничья!").WithCurrentTimestamp().Build());
            }
            else
            {
                if (ChosenItem.ToLower() == "камень" && BotChosenItem == 2)
                {
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Выбор бота: ").WithValue(result));
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Ваш выбор: ").WithValue(GetEmoji(ChosenItem)));
                    await ReplyAsync(embed: resultEmbed.WithDescription("Вы победили!").WithCurrentTimestamp().Build());
                }
                else if (ChosenItem.ToLower() == "ножницы" && BotChosenItem == 3)
                {
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Выбор бота: ").WithValue(result));
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Ваш выбор: ").WithValue(GetEmoji(ChosenItem)));
                    await ReplyAsync(embed: resultEmbed.WithDescription("Вы победили!").WithCurrentTimestamp().Build());
                }
                else if (ChosenItem.ToLower() == "бумага" && BotChosenItem == 1)
                {
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Выбор бота: ").WithValue(result));
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Ваш выбор: ").WithValue(GetEmoji(ChosenItem)));
                    await ReplyAsync(embed: resultEmbed.WithDescription("Вы победили!").WithCurrentTimestamp().Build());
                }
                else
                {
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Выбор бота: ").WithValue(result));
                    resultEmbed.AddField(new EmbedFieldBuilder().WithName("Ваш выбор: ").WithValue(GetEmoji(ChosenItem)));
                    await ReplyAsync(embed: resultEmbed.WithDescription("Вы проиграли!").WithCurrentTimestamp().Build());
                }
            }
            

        }

        [Command("RPCDuel", RunMode = RunMode.Async)]
        [Alias("дуэль", "duel")]
        public async Task Duel(IUser user, string ChosenItem)
        {
            await Context.Message.DeleteAsync();
            var duelmessage = await Context.Channel.SendMessageAsync($"{Context.User.Mention} вызвал {user.Mention} на дуэль в камень ножницы бумага!");
            Program.lastDueledUser = Context.User;
            Program.lastDuelEnemy = user;
            Program.lastDueledUserChoise = ChosenItem;
        }

        [Command("duel accept", RunMode = RunMode.Async)]
        public async Task AcceptDuel(string ChosenItem)
        {
            if(Context.User != Program.lastDuelEnemy)
            {
                await ReplyAsync($"**{ Program.lastDueledUser.Username}** вызвал на дуэль **{ Program.lastDuelEnemy.Username}**, а не вас!");
            }
            else
            {
                await ReplyAsync($"Вы приняли вызов от { Program.lastDueledUser.Mention }");
                Program.lastDuelEnemyChoise = ChosenItem;

                ClearDuel();

            }
        }

        [Command("duel decline", RunMode = RunMode.Async)]
        public async Task DeclineDuel()
        {
            if (Context.User != Program.lastDuelEnemy)
            {
                await ReplyAsync($"**{ Program.lastDueledUser.Username}** вызвал на дуэль **{ Program.lastDuelEnemy.Username}**, а не вас!");
            }
            else
            {
                await ReplyAsync($"{ Program.lastDuelEnemy.Mention } отклонил вызов от { Program.lastDueledUser.Mention }");
                ClearDuel();

            }
        }
    }
    public class AnimalCommands : ModuleBase<SocketCommandContext>
    {
        #region Рандомные картинки
        [Command("fox")]
        [Alias("foxy", "лис", "лиса")]
        private async Task RandomFox()
        {
            using (WebClient wc = new WebClient())
            {

                var rawjson = wc.DownloadString(@"https://some-random-api.ml/img/fox").Split('"');
                var foxurl = rawjson[3];

                var FoxEmbed = new EmbedBuilder().WithTimestamp(DateTimeOffset.Now).WithTitle("Держите лису, :fox:").WithImageUrl(foxurl);

                await ReplyAsync("", embed: FoxEmbed.Build());

            }
        }

        [Command("cat")]
        [Alias("кот", "котя")]
        private async Task RandomCat()
        {
            using (WebClient wc = new WebClient())
            {

                var rawjson = wc.DownloadString(@"https://some-random-api.ml/img/cat").Split('"');
                var caturl = rawjson[3];

                var CatEmbed = new EmbedBuilder().WithTimestamp(DateTimeOffset.Now).WithTitle("Держите котейкина, :cat: ").WithImageUrl(caturl);

                await ReplyAsync("", embed: CatEmbed.Build());

            }
        }
        [Command("dog")]
        [Alias("пёс", "собакен")]
        private async Task RandomDog()
        {
            using (WebClient wc = new WebClient())
            {

                var rawjson = wc.DownloadString(@"https://some-random-api.ml/img/dog").Split('"');
                var dogurl = rawjson[3];

                var DogEmbed = new EmbedBuilder().WithTimestamp(DateTimeOffset.Now).WithTitle("Держите собакена, :dog: ").WithImageUrl(dogurl);

                await ReplyAsync("", embed: DogEmbed.Build());

            }
        }
        #endregion
    }

    public class NekoCommands : ModuleBase<SocketCommandContext>
    {
        #region neko
        [Command("rneko")]
        private async Task RandomNekoAsync()
        {
            NekosImage image = await NekosClient.GetRandomSfwAsync();

            EmbedBuilder NekoEmbed = new EmbedBuilder().WithImageUrl(image.FileUrl);
            await ReplyAsync("", false, NekoEmbed.Build());
        }
        [Command("fact")]
        private async Task FactAsync()
        {
            var fact = await NekosClient.GetFactAsync();

            EmbedBuilder NekoEmbed = new EmbedBuilder().WithDescription(fact.Fact);
            await ReplyAsync("", false, NekoEmbed.Build());
        }
        [Command("owo")]
        private async Task OwoAsync([Remainder] string text)
        {
            var owoifystring = await NekosClient.GetOwoifyStringAsync(text);

            await ReplyAsync(owoifystring.OwoString);
        }
        [Command("neko")]
        private async Task NekoAsync([Remainder] string Type)
        {
            NekosImage cute = null;
            switch (Type)
            {
                case "kiss":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Kiss);
                    break;
                case "hug":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Hug);
                    break;
                case "fox":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Fox_Girl);
                    break;
                case "pat":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Pat);
                    break;
                case "meow":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Meow);
                    break;
                case "poke":
                    cute = await NekosClient.GetSfwAsync(Nekos.Net.Endpoints.SfwEndpoint.Poke);
                    break;
            }
            EmbedBuilder NekoEmbed = new EmbedBuilder().WithImageUrl(cute.FileUrl);
            await ReplyAsync("", false, NekoEmbed.Build());
        }
        #endregion
        #region LewdNeko 
        [Command("rlewdneko")]
        [RequireNsfw]
        private async Task RandomLewdNekoAsync()
        {
            NekosImage image = await NekosClient.GetRandomNsfwAsync();
            EmbedBuilder NekoEmbed = new EmbedBuilder().WithAuthor(Context.Client.CurrentUser.Username).WithImageUrl(image.FileUrl);
            await ReplyAsync("", false, NekoEmbed.Build());
        }
        [Command("lewdneko")]
        [RequireNsfw]
        private async Task LewdNekoAsync([Remainder] string Type)
        {
            List<string> Tags = new List<string>
            {
                "anal",
                "pussy",
                "trap",
                "yuri",
                "tits",
                "cum",
                "blowjob",
                "futa",
                "kuni"
            };
            NekosImage image = null;
            EmbedBuilder HelpEmbed = null;
            switch (Type)
            {
                case "anal":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Anal);
                    break;
                case "pussy":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Pussy);
                    break;
                case "trap":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Trap);
                    break;
                case "yuri":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Yuri);
                    break;
                case "tits":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Tits);
                    break;
                case "cum":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Cum);
                    break;
                case "blowjob":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Blowjob);
                    break;
                case "futa":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Futanari);
                    break;
                case "kuni":
                    image = await NekosClient.GetNsfwAsync(Nekos.Net.Endpoints.NsfwEndpoint.Kuni);
                    break;
                case "help":
                    image = null;
                    HelpEmbed = new EmbedBuilder();
                    int i = 0;
                    foreach (var tag in Tags)
                    {
                        i++;
                        EmbedFieldBuilder f = new EmbedFieldBuilder().WithName(i.ToString()).WithValue(tag).WithIsInline(true);
                        HelpEmbed.AddField(f);
                    }
                    break;
            }
            if (image == null)
            {
                await ReplyAsync("", false, HelpEmbed.Build());
            }
            else
            {
                EmbedBuilder NekoEmbed = new EmbedBuilder().WithAuthor(Context.Client.CurrentUser.Username).WithImageUrl(image.FileUrl);
                await ReplyAsync("", false, NekoEmbed.Build());
            }

        }
        #endregion
    }
}
