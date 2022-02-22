namespace API.Entities
{
    public class Player
    {
        public int id { get; set; }
        public string discord { get; set; }
        public string battlenet { get; set; }
        public string prole { get; set; }
        public Guid TeamId { get; set; }

    }
}
