namespace Jira.Api.Models
{
    public class Issue
    {
        public string Key { get; set; }
        public IssueFields Fields { get; set; }
    }
}