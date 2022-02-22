namespace API.Entities
{
    public class OrganisationUsers
    {
        public int Id { get; set; }
        public Guid OrgId { get; set; }
        public int UserId { get; set; }
    }
}
