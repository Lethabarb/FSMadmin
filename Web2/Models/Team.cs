namespace Web2.Models
{
    public class Team
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public Guid organisationId { get; set; }
        public ulong captain { get; set; }
        public int rank { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
        public IEnumerable<Player> players { get; set; }
        public string exception { get; set; }
        public bool myTeam { get; set; }
    }
}
