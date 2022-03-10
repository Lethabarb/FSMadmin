using System.Net.Http.Headers;
using Web2.Models;

namespace Web2.Helpers
{
    public class Api
    {
        private readonly HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://fsmorgapi.azurewebsites.net"),
        };

        public async Task<User> Login(string email, string password)
        {
            UserLogin login = new()
            {
                Email = email,
                Password = password
            };
            HttpResponseMessage res = await client.PostAsJsonAsync("/login", login);
            return await res.Content.ReadFromJsonAsync<User>();
        }
        public async Task<string> Regrister(User user)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/regrister", user);
            return res.Content.ToString();
        }
        public async Task<string> ForgotPassword(string email)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/forgotpassword", new Donkey()
            {
                Value = email
            });
            return res.Content.ToString();
        }
        public async Task<string> ChangePassword(string token, string newPassword)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync($"/forgotpassword/{token}", new Donkey()
            {
                Value=newPassword
            });
            return res.Content.ToString();

        }        
        public async Task<List<Login>> GetLogin()
        {
            return await client.GetFromJsonAsync<List<Login>>("/logins");
        }
        public async Task<List<Organisation>> GetOrganisations()
        {
            var orgs = await client.GetFromJsonAsync<List<Organisation>>("/Organisation/getall");
            var allteams = await GetTeams();
            foreach (Organisation org in orgs)
            {
                org.teams = allteams.Where(t => t.organisationId == org.id).ToList();
            }
            return orgs;
        }
        public async Task<Organisation> GetOrganisation(int UserId)
        {
            List<OrganisationUsers> organisationUsers = await client.GetFromJsonAsync<List<OrganisationUsers>>("/OrganisationUsers");
            Organisation Org = await client.GetFromJsonAsync<Organisation>($"/Organisation/get/{organisationUsers.Where(ou => ou.UserId == UserId).FirstOrDefault().OrgId}");
            IEnumerable<Team> teams = await client.GetFromJsonAsync<IEnumerable<Team>>("/Team/getall");
            Org.teams = teams;
            Org.teams = Org.teams.Where(t => t.organisationId == Org.id).ToList();
            return Org;
        }
        public async Task<int> getUserId(string Email)
        {
            User res = await client.GetFromJsonAsync<User>($"/User/{Email}");
            return res.Id;

        }
        public async Task<bool> AddUsertoOrg(int UserId, Guid orgId, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PostAsJsonAsync<OrganisationUsers>("/Organisation/AddUser", new()
            {
                OrgId = orgId,
                UserId = UserId
            });
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> IsUserInOrg(int UserId)
        {
            List<OrganisationUsers> organisationUsers = await client.GetFromJsonAsync<List<OrganisationUsers>>("/OrganisationUsers");
            return organisationUsers.Exists(u => u.UserId == UserId);

        }
        public async Task<bool> CreateOrganisation(Organisation org, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            org.id = Guid.NewGuid();
            HttpResponseMessage res = await client.PostAsJsonAsync<Organisation>("/Organisation/Create", org);
            HttpResponseMessage res2 = await client.PostAsJsonAsync<OrganisationUsers>("/Organisation/AddUser", new OrganisationUsers()
            {
                OrgId = org.id,
                UserId = user.Id
            });
            if (res.IsSuccessStatusCode && res2.IsSuccessStatusCode)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrganisation(Organisation org, Guid Id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PutAsJsonAsync<Organisation>($"Organisation/update/{Id}", org);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> DeleteOrganisation(int id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.DeleteAsync($"Organisation/Delete/{id}");
            return res.IsSuccessStatusCode;
        }
        public async Task<List<Player>> GetPlayers()
        {
            return await client.GetFromJsonAsync<List<Player>>("/Player/GetAll");
        }
        public async Task<bool> CreatePlayer(Player player, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PostAsJsonAsync<Player>("/Player/Create", player);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> UpdatePlayer(Player player, int Id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PutAsJsonAsync($"Player/update/{Id}", player);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> DeletePlayer(int id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.DeleteAsync($"player/delete/{id}");
            return res.IsSuccessStatusCode;
        }
        public async Task<List<Scrim>> GetScrims()
        {
            var scrims = await client.GetFromJsonAsync<List<Scrim>>("/Scrim/GetAll");
            var teams = await GetTeams();
            foreach (Scrim s in scrims)
            {
                s.teams = teams;
            }
            
            return scrims;
        }
        public async Task<bool> CreateScrim(Scrim Scrim, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PostAsJsonAsync<Scrim>("/Scrim/Create", Scrim);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateScrim(Scrim Scrim, int Id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PutAsJsonAsync($"Scrim/update/{Id}", Scrim);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> DeleteScrim(int id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.DeleteAsync($"Scrim/delete/{id}");
            return res.IsSuccessStatusCode;
        }
        public async Task<Team> GetTeam(Guid id)
        {
            Team team = await client.GetFromJsonAsync<Team>($"/Team/Get/{id}");
            return team;
        }
        public async Task<List<Team>> GetTeams()
        {
            List<Team> teams = await client.GetFromJsonAsync<List<Team>>("/Team/GetAll");
            List<Player> allplayers = await GetPlayers();
            foreach (Team t in teams)
            {
                t.players = allplayers.Where(p => p.TeamId == t.id).ToList();
                List<Player> sorted = new();
                int dps = 0;
                int supp = 0;
                foreach (Player p in t.players)
                {
                    if (p.prole == "Tank")
                    {
                        sorted.Insert(0, p);
                        dps++;
                        supp++;
                    } else if (p.prole == "DPS")
                    {
                        sorted.Insert(dps, p);
                        supp++;
                    } else if (p.prole == "Support")
                    {
                        sorted.Insert(supp, p);
                    } else
                    {
                        sorted.Add(p);
                    }
                }
                t.players = sorted;

            }
            return teams;
        }
        public async Task<bool> CreateTeam(Team Team, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PostAsJsonAsync<Team>("/Team/Create", Team);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateTeam(Team Team, Guid Id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.PutAsJsonAsync($"Team/update/{Id}", Team);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> DeleteTeam(Guid id, User user)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.token);
            var res = await client.DeleteAsync($"Team/delete/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}
