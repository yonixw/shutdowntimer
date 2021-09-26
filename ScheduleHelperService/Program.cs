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
 * program conf file + id that a cron format can call upon (with custom PARAM=PARAM???)
 * 
 * Get time from UTC?? also verify from some server?
 * 
 * Expression
 *  Cront Text
 *  ExecId
 *  Custom Additional Args
 *  
 * Execs:
 *   Id 
 *   Args
 *   WorkPath
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
            l(@"** Use the following documentation to write cron expressions: **
* (“all values”)
? (“no specific value”) - either day of month or day of week (can't specify both)
/ (used to specify increments. For example, “0/15” = 0,15,30,45)
L - Last day of month or week
W - weekdays (Mon-Fri)
# - 6#3 in the day of week field means the third Friday (day 6 is Friday; #3 is the 3rd Friday in the month). 
Field Name      Allowed Values       Allowed Special Characters
----------------------------------------------------
Seconds         0-59                 , - * /
Minutes         0-59                 , - * /
Hours           0-23                 , - * /
Day-of-month    1-31                 , - * ? / L W
Month           1-12 or JAN-DEC      , - * /
Day-of-Week     1-7 or SUN-SAT       , - * ? / L #
Year (Optional) empty, 1970-2199     , - * /
---------------------------------------------------");
        }

        static void Main(string[] args)
        {
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

            l("[DONE]");
        }
    }
}
