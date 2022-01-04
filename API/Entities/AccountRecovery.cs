namespace API.Entities
{
    public class AccountRecovery
    {
        public Guid Id { get; set; }
        public string email { get; set; }
        public bool used { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
