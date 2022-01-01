namespace API.Entities
{
    public class Team
    {
        public int id { get; set; }
        public string name { get; set; }
        public int organisationId { get; set; }
        public int captain { get; set; }
    }
}
