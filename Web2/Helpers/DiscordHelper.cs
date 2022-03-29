using Discord;
using Discord.WebSocket;

namespace Web2.Helpers
{
    public class DiscordHelper
    {
        private readonly IConfiguration configuration;
        private readonly DiscordSocketClient client = new();
        public DiscordHelper(DiscordSocketClient _client, IConfiguration _configuration)
        {
            client = _client;
            configuration = _configuration;
            StartClient().GetAwaiter().GetResult();
        }
        
        private Task log(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
        private async Task StartClient()
        {
            await client.LoginAsync(TokenType.Bot, configuration.GetValue<string>("DiscordBotToken"));
            await client.StartAsync();
            client.Ready += Ready;
            await Task.Delay(1000);
        }
        private async Task Ready()
        {
            Console.WriteLine("botready");
        }
        public async Task SendMessage(string message, ulong channel)
        {
            var Channel = client.GetChannel(channel) as SocketTextChannel;
            await Channel.SendMessageAsync(message);
        }
        public async Task SendMessage(Embed message, ulong channel)
        {
            var Channel = client.GetChannel(channel) as SocketTextChannel;
            await Channel.SendMessageAsync(null, false, message);
        }
        public async Task SendUserMessage(string message, ulong id)
        {
            var user = await client.GetUserAsync(id) as IUser;
            await user.SendMessageAsync(message);
        }
        public async Task SendUserMessage(Embed message, ulong id)
        {
            var user = await client.GetUserAsync(id) as IUser;
            await user.SendMessageAsync("", false, message);
        }
        public async Task<IUser> getUser(ulong id, ulong guildId)
        {
            var user = client.GetGuild(guildId).GetUser(id) as IUser;
            return user;
        }
        public async Task deleteMessages(ulong id)
        {
            var channel = client.GetChannel(id) as ITextChannel;
            var messages = await channel.GetMessagesAsync(100, CacheMode.AllowDownload).FlattenAsync();
            await channel.DeleteMessagesAsync(messages);
        }
    }
}
