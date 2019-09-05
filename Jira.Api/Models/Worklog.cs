using System;

namespace Jira.Api.Models
{
    public class Worklog
    {
        public WorklogAuthor Author { get; set; }
        public DateTime Started { get; set; }
        public int TimeSpentSeconds { get; set; }
    }
}