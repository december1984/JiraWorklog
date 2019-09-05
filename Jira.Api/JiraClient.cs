using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Jira.Api.Models;
using Newtonsoft.Json;

namespace Jira.Api
{
    public class JiraClient
    {
        private readonly string _user;
        private readonly string _email;
        private readonly string _server;
        private readonly HttpClient _httpClient;

        public JiraClient(string email, string token, string server)
        {
            _email = email;
            _user = _email.Substring(0, _email.IndexOf('@'));
            _server = server;

            var authheader = $"{email}:{token}";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authheader));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);

        }

        public async Task<Issue[]> GetIssues()
        {
            var response = await _httpClient.GetAsync($"https://{_server}/rest/api/latest/search?jql=worklogAuthor={_user}&fields=summary,parent");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SearchResult>(json);
            return result.Issues;
        }

        public async Task<Worklog[]> GetWorklogs(string issue, DateTime start, DateTime end)
        {
            var response = await _httpClient.GetAsync($"https://{_server}/rest/api/latest/issue/{issue}/worklog");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WorklogResult>(json);
            return result.Worklogs.Where(w => w.Author.Name == _user && w.Started > start && w.Started < end).ToArray();
        }
    }
}
