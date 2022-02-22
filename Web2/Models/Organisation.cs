using System.Web;
namespace Web2.Models
{
    public class Organisation
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string shortName { get; set; }
        public ulong guildId { get; set; }
        public string botConfig { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public IEnumerable<Team> teams { get; set; }
        public BotConfig Config => Newtonsoft.Json.JsonConvert.DeserializeObject<BotConfig>(botConfig);
    }
}
