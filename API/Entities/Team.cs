namespace API.Entities
{
    public class Team
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public Guid organisationId { get; set; }
        public ulong captain { get; set; }
        public int Rank { get; set; }
        public string ImageName { get; set; }
        public string ImagePath { get; set; }
    }
}
