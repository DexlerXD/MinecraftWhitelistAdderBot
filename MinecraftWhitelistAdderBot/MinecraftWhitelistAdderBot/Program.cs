using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using RconSharp;

namespace MinecraftWhitelistAdderBot
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient discordClient;
        private string discordToken;
        private RconClient rconClient;

        private async Task MainAsync()
        {
            discordClient = new DiscordSocketClient();
            discordToken = File.ReadAllText("token.txt");

            string[] rconFile = File.ReadAllLines("rcon.txt");
            string rconAddress = rconFile[0];
            int rconPort;
            Int32.TryParse(rconFile[1], out rconPort);
            string rconPassword = rconFile[2];
            rconClient = RconClient.Create(rconAddress, rconPort);

            await discordClient.LoginAsync(TokenType.Bot, discordToken);
            await discordClient.StartAsync();

            discordClient.Ready += ClientReady;

            await rconClient.ConnectAsync();
            bool isRconConnected = await rconClient.AuthenticateAsync(rconPassword);
            if (!isRconConnected)
                return;

            await Task.Delay(-1);
        }

        private async Task ClientReady()
        {
            Console.WriteLine("Bot is working!");
            await RegisterCommand();
            discordClient.SlashCommandExecuted += OnSlashCommandExecuted;
        }

        private async Task RegisterCommand()
        {
            var addToWhiteList = new SlashCommandBuilder()
                .WithName("add")
                .WithDescription("Adds username to whitelist.")
                .AddOption("username", ApplicationCommandOptionType.String, "Username to add", true);

            try
            {
                await discordClient.CreateGlobalApplicationCommandAsync(addToWhiteList.Build()); 
            }
            catch (HttpException exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            //this bot has only one purpose for now, so we got a lazy implementation here
            string username = (String)command.Data.Options.First();
            string response = "Username succesfully added!";

            try
            {
                await AddToWhitelist(username);
            }
            catch (HttpException)
            {
                response = "Something went wrong! Please contact admins.";
            }

            await command.RespondAsync(response);
        }

        private async Task AddToWhitelist(string username)
        {
            string command = $"whitelist add {username}";

            try
            {
                await rconClient.ExecuteCommandAsync(command, false);
            }
            catch (HttpException exception)
            {
                Console.WriteLine(exception);
                return;
            }

            Console.WriteLine("command is used!");
        }
    }
}
