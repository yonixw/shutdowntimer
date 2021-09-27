using Quartz;
using ScheduleHelperService.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;


/*
 * TODO
 * 
 * Get time from UTC?? also verify from some server?
 *      Time server? 
 *   
 *  YAML that will be in same Direcotry, or from first ARGS of this program (args[0])
*/

namespace ScheduleHelperService
{
    class Program
    {
        static void l(string msg)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine(string.Format("[{0} {1} ({2}s)] {3}",
                   now.ToShortDateString(), now.ToShortTimeString(), now.Second, msg));
        }

        public static DateTime ConvertUtcDateTime(DateTime utcDateTime, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);

            return convertedDateTime;
        }

        static void Main(string[] args)
        {
            ConfigLoadResult configLoadTask = ConfigUtils.TryLoad("config.yaml");

            if (configLoadTask.failed)
            {
                l("Config load failed");
                l(" - here is the error:");
                l(configLoadTask.error);
                l(" - here is the example from README:");
                l(Resources.ReadMe);
                return;
            }

            config currentConfig = configLoadTask.result;

            l("Creating cron objects...");
            Dictionary<string, CronExpression> cronExp = new Dictionary<string, CronExpression>();
            foreach (scheduleItem si in currentConfig.schedule.Values)
            {
                string key = si.pattern + ";" + si.timezone;
                CronExpression expr = new CronExpression(si.pattern);
                expr.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(si.timezone);
                if (!cronExp.ContainsKey(key))
                {
                    cronExp.Add(key, expr);
                }
            }


            while (true)
            {
                l("Sleeping... " + currentConfig.secinterval + "s");

                // !! Thread.Sleep is the most simple way to not be handaling timer being garbage collected
                Thread.Sleep(currentConfig.secinterval * 1000);

                DateTime nowUTC = DateTime.UtcNow;

                l("Trying to find a matched schedule...");
                foreach (string schKey in currentConfig.schedule.Keys)
                {
                    scheduleItem rule = currentConfig.schedule[schKey];
                    if (rule.targets.Count == 0) continue; // no reason to check..

                    // cron always use timezone from it's object 
                    if (!cronExp[rule.pattern + ";" + rule.timezone].IsSatisfiedBy(nowUTC)) 
                    {
                        continue;
                    }

                    l("Schedule matched! - " + schKey);
                    int j = 1;
                    foreach (program target in rule.targets)
                    {
                        string tag = "** " + schKey + " **, target #: " + j + "/" + rule.targets.Count ;
                        try
                        {
                            ProcessStartInfo si = new ProcessStartInfo(target.path);
                            if (!string.IsNullOrEmpty(target.args))
                                si.Arguments = target.args;
                            if (!string.IsNullOrEmpty(target.workdir))
                                si.WorkingDirectory = target.workdir;
                            Process p = Process.Start(si);
                            if (p != null)
                            {
                                int id_ref = p.Id;
                                p.Exited += (sender, obj) => l(tag + ", Process Id:" + id_ref + " - existed.");
                                l("Started process with PID: " + id_ref);
                            }
                            else
                            {
                                l("Error starting " + tag + ", found null as process!");
                            }
                        }
                        catch (Exception ex)
                        {
                            l("[ERROR] " + tag + ", Error Info:\n" + ex.ToString());
                        }
                        j++;
                    }
                }
            }

            l("[DONE]");

            Console.ReadKey();
        }

    }
}
