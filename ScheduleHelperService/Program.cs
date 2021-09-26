using Quartz;
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
        static Properties.Settings settings = ScheduleHelperService.Properties.Settings.Default;

        static void l(string msg)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine(string.Format("[{0} {1} ({2}s)] {3}",
                   now.ToShortDateString(), now.ToShortTimeString(), now.Second, msg));
        }

        static void printCronInfo()
        {
            l(@"");
        }

        public DateTime ConvertUtcDateTime(DateTime utcDateTime, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);

            return convertedDateTime;
        }

        static void Main(string[] args)
        {
            ConfigUtils.TryLoad("config.yaml");
            return;


            /*
            if (String.IsNullOrEmpty(settings.exePath))
            {
                l("Empty exe path! Exisiting...");
            }
            else
            {
                bool allCronExpressionsValid = true;
                List<CronExpression> timesToCheck = new List<CronExpression>();
                settings.cronJobs
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    .ForEach((cronEx) =>
                    {
                        if (!cronEx.StartsWith("#")) // not a note
                        {
                            if (CronExpression.IsValidExpression(cronEx))
                            {
                                timesToCheck.Add(new CronExpression(cronEx));
                            }
                            else
                            {
                                l("[ERROR] Can't validate cron expression: '" + cronEx + "'");
                                allCronExpressionsValid = false;
                            }
                        }
                    });

                if (!allCronExpressionsValid)
                {
                    printCronInfo();
                }
                else
                {
                    l("Interval: " + settings.secInterval);
                    l("Cron Jobs:\n----\n" + settings.cronJobs + "\n----\n\n");

                    while (true)
                    {
                        l("Sleeping... " + settings.secInterval + "s");

                        // !! Thread.Sleep is the most simple way to not be garbage collected
                        Thread.Sleep(settings.secInterval * 1000);

                        bool timeMatchedCron = false;

                        DateTime now = DateTime.Now;
                        
                        timesToCheck.ForEach((cron) =>
                        {
                            if (cron.IsSatisfiedBy(now))
                            {
                                l("Matched Time! from '" + cron.ToString() + "'");
                                timeMatchedCron = true;
                            }
                        });

                        if (timeMatchedCron)
                        {
                            l("Running program....");

                            ProcessStartInfo si = new ProcessStartInfo(settings.exePath);
                            if (!string.IsNullOrEmpty(settings.exeArgs))
                                si.Arguments = settings.exeArgs;
                            if (!string.IsNullOrEmpty(settings.exeWorkDir))
                                si.WorkingDirectory = settings.exeWorkDir;
                            Process p = Process.Start(si);

                            l("Started process with PID: " + p.Id);
                            Thread.Sleep(5 * 1000);
                            l("Is process exited after 5 sec? " + p.HasExited);
                        }
                    }
                }

            }
            */

            l("[DONE]");
        }
    }
}
