namespace Web2.Models
{
    public class OrganisationUsers
    {
        public int Id { get; set; }
        public Guid OrgId { get; set; }
        public int UserId { get; set; }
    }
}
