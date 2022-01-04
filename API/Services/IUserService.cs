namespace API.Services
{
    public interface IUserService
    {
        public User Get(UserLogin login, List<User> users);
    }
}
