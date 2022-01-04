namespace API.Services
{
    public class UserService : IUserService
    {
        public User Get(UserLogin Login, List<User> users)
        {
            User user = users.Where(u => u.Username.Equals(Login.Username, StringComparison.OrdinalIgnoreCase) && u.Password.Equals(Login.Password)).FirstOrDefault();

            return user;
        }
    }
}
