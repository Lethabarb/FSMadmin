namespace Web2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ulong DiscordUserId { get; set; }
        public string Role { get; set; }
        public string token { get; set; }
        
    }
}
