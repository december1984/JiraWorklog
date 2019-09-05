using System.Collections.Generic;
using System.Text;

namespace Jira.Api.Models
{
    public class SearchResult
    {
        public int Total { get; set; }
        public Issue[] Issues { get; set; }
    }
}
