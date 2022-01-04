using Microsoft.AspNetCore.Mvc;

namespace API.Services
{
    public interface IEmailService
    {
        public AccountRecovery SendEmail( string email);
    }
}
