namespace API.Entities
{
    public class Scrim
    {
        public int id { get; set; }
        public Guid Team1 { get; set; }
        public Guid Team2 { get; set; }
        public DateTime datetime { get; set; }

    }
}
