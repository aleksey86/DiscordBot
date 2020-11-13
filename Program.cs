using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
namespace DiscordBot
{
    class Program
	{
		public static IUser lastDueledUser { get; set; }
		public static IUser lastDuelEnemy { get; set; }
		public static string lastDueledUserChoise { get; set; }
		public static string lastDuelEnemyChoise { get; set; }
		static string token = "NzUyNTc2NzMxMTM5MDE0NzQ3.X1Zpsg.I-ZF7RaEXA-PZCNQocf4xfNDmc0";
		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient _client;
		private CommandService _commands;
		private IServiceProvider _services;
		public async Task MainAsync()
		{
			_client = new DiscordSocketClient();
			_commands = new CommandService();
			_services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();
			
			// Remember to keep token private or to read it from an 
			// external source! In this case, we are reading the token 
			// from an environment variable. If you do not know how to set-up
			// environment variables, you may find more information on the 
			// Internet or by using other methods such as reading from 
			// a configuration.
			_client.Log += Log;
			_client.Ready += ReadyAsync;
			_client.UserJoined += AnnounceUserJoined;

			await RegisterCommandsAsync();

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
			await _client.SetGameAsync("Visual Studio 2019");
			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Task ReadyAsync()
		{
			Console.WriteLine($"{_client.CurrentUser} is connected! Total Servers: {_client.Guilds.Count}");

			return Task.CompletedTask;
		}
		private async Task AnnounceUserJoined(SocketGuildUser user)
		{
			var guild = user.Guild;
			var channel = guild.DefaultChannel;
			EmbedBuilder builder = new EmbedBuilder().WithTitle("Новенький!").WithDescription($"Привет, {user.Mention}!").WithAuthor(_client.CurrentUser).WithColor(Color.Purple);
			await channel.SendMessageAsync("", false, builder.Build());
			await user.SendMessageAsync(embed: builder.Build());

		}

		private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

		private async Task RegisterCommandsAsync()
        {
			_client.MessageReceived += HandleCommandAsync;
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

		private async Task HandleCommandAsync(SocketMessage arg)
        {
			var message = arg as SocketUserMessage;
			var ctx = new SocketCommandContext(_client, message);
			if (message.Author.IsBot) return;

			int argPos = 0;
			if(message.HasCharPrefix('+', ref argPos))
            {
				var result = await _commands.ExecuteAsync(ctx, argPos, _services);
				if (!(result.IsSuccess)) Console.WriteLine(result.Error);
			 
			}
        }
	}
}

