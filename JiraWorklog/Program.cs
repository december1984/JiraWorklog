using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jira.Api;
using Jira.Api.Models;
using Microsoft.Extensions.Configuration;

namespace JiraWorklog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var start = configuration.GetValue<DateTime>("start").Date;
            var end = configuration.GetValue<DateTime>("end").Date.AddDays(1);
            var output = configuration["output"];

            var client = new JiraClient(configuration["email"], configuration["token"], configuration["server"]);

            var calendar = new Dictionary<DateTime, Dictionary<string, TimeSpan>>();
            for (var date = start; date < end; date = date.AddDays(1))
            {
                calendar[date] = new Dictionary<string, TimeSpan>();
            }

            var issues = await client.GetIssues();
            var loggedIssues = new List<Issue>();

            foreach (var issue in issues)
            {
                var worklogs = await client.GetWorklogs(issue.Key, start, end);
                if (worklogs.Length == 0) continue;
                
                loggedIssues.Add(issue);
                foreach (var log in worklogs)
                {
                    if (!calendar[log.Started.Date].TryGetValue(issue.Key, out var time))
                    {
                        time = TimeSpan.Zero;
                    }

                    calendar[log.Started.Date][issue.Key] = time.Add(TimeSpan.FromSeconds(log.TimeSpentSeconds));
                }
            }

            using (var writer = new StreamWriter(output))
            {
                writer.Write("parent key;parent description;issue key;issue description;");
                writer.WriteLine(string.Join(';', calendar.Keys.Select(d => d.ToShortDateString())));
                writer.Write(";;;;");
                var total = TimeSpan.Zero;
                foreach (var day in calendar)
                {
                    var sum = TimeSpan.FromSeconds(day.Value.Sum(l => l.Value.TotalSeconds));
                    total = total.Add(sum);
                    writer.Write(sum);
                    writer.Write(';');
                }
                writer.WriteLine(total);
                foreach (var issue in loggedIssues.OrderBy(i => i.Key))
                {
                    total = TimeSpan.Zero;
                    writer.Write(issue.Fields.Parent != null
                        ? $"{issue.Fields.Parent.Key};{issue.Fields.Parent.Fields.Summary};"
                        : ";;");
                    writer.Write($"{issue.Key};{issue.Fields.Summary};");
                    foreach (var day in calendar)
                    {
                        if (day.Value.TryGetValue(issue.Key, out var time))
                        {
                            total = total.Add(time);
                            writer.Write(time);
                        }
                        writer.Write(';');
                    }
                    writer.WriteLine(total);
                }
            }
        }
    }
}
