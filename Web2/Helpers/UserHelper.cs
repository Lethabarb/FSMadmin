using Web2.Models;

namespace Web2.Helpers
{
    public class UserHelper
    {
        private bool SignedIn = false;
        private User user;

        public User GetUser()
        {
            if (SignedIn) return user;
            return new User();
        }
        public int GetUserId()
        {
            if (SignedIn) return user.Id;
            return 0;
        }

        public bool isSignedIn()
        {
            return SignedIn;
        }
        public async Task<User> Login(string email, string password)
        {
            Api api = new Api();
            user = await api.Login(email, password);
            if (user.EmailAddress != "Invalid")
            {
                SignedIn = true;
            }
            
            return user;
        }

        public async Task Regrister(User user)
        {
            Api api =new Api();
            var res = api.Regrister(user);
        }
        public void Logout()
        {
            user = new User();
            SignedIn = false;
        }
        public async Task<string> ForgotPassword(string email)
        {
            Api api = new Api();
            string res = await api.ForgotPassword(email);
            return res;
        }
        public async Task<string> ResetPassword(string token, string newpassword)
        {
            Api api = new Api();
            string res = await api.ChangePassword(token, newpassword);
            return res;
        }
        public async Task<int> GetLoginCount()
        {
            Api api = new Api();
            List<Login> Logins = await api.GetLogin();
            return Logins.Where(l => l.UserId == user.Id).Count();
        }
        public async Task<Organisation> GetOrganisation()
        {
            Api api = new Api();
            Organisation organisation = new();
            if (SignedIn)
            {
                Organisation org = await api.GetOrganisation(user.Id);
                List<Team> teams = await api.GetTeams();
                teams = teams.Where(t => t.organisationId == org.id).ToList();
                org.teams = teams;
                return org;
            }
            return organisation;
        }
    }
}
