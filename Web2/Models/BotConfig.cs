namespace Web2.Models
{
    public class BotConfig
    {
        public ulong GuildId { get; set; }
        public ulong BotChannel { get; set; }
        public List<TeamConfig> Teams { get; set; }
    }
}
