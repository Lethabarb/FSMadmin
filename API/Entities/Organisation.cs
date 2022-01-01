namespace API.Entities
{
    public class Organisation
    {
        public int id { get; set; }
        public string name { get; set; }
        public ulong guildId { get; set; }
        public string botConfig { get; set; }
    }
}
