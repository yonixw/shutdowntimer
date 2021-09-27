using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

// Thanks to https://archive.is/5TLci for MergingParser

namespace ScheduleHelperService
{
    public static class ConfigUtils
    {
        public const string lineSeperator = "===================================";

        static IDeserializer d = new DeserializerBuilder() // TODO how to validate empty proprs?
                .Build();

        static ISerializer s = new SerializerBuilder()
            .WithMaximumRecursion(50)
            .Build();

        static bool isInvalidPath(string path)
        {
            //https://stackoverflow.com/a/26071956/1997873
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path))
            {
                return true;
            }
            return false;
        }

        public static ConfigLoadResult TryLoad(string path)
        {
            string lastStepDesc = "initial";
            string latestContextDesc = "none";
            config currentConfig = null;

            try
            {
                lastStepDesc = "Loading config file from '" + path + "'";
                string yamlText = File.ReadAllText(path);

                lastStepDesc = "Checking if valid yaml and supported version";
                try
                {
                    using (TextReader yamlsr = new StringReader(yamlText))
                    {
                        var yamlObj = d.Deserialize(new MergingParser(new Parser(yamlsr)));
                        string version = (string)((Dictionary<object, object>)yamlObj)["version"];
                        if (version != "1")
                        {
                            throw new Exception("Unknown ScheduleHelper config version '" + version + "'");
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                lastStepDesc = "Loading config as built-in object";
                using (TextReader yamlsr = new StringReader(yamlText))
                {
                    currentConfig = d.Deserialize<config>(new MergingParser(new Parser(yamlsr)));
                }

                lastStepDesc = "Checking schedules - cron validities";
                foreach(string schKey in currentConfig.schedule.Keys)
                {
                    latestContextDesc = schKey;
                    string pattern = currentConfig.schedule[schKey].pattern;
                    if (!CronExpression.IsValidExpression(pattern))
                    {
                        throw new Exception("Pattern key '" + schKey + "': '" + pattern + "' is invalid ");
                    }
                    // TODO: ignore schedules with 0 tasks in main loop.
                }

                lastStepDesc = "Checking schedules - timezone validities";
                foreach (string schKey in currentConfig.schedule.Keys)
                {
                    latestContextDesc = schKey;
                    string tzName = currentConfig.schedule[schKey].timezone;
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(tzName);
                }

                lastStepDesc = "Checking schedules - target process ";
                // only not empty and valid character path, because don't want to exclude
                //      scenarios of exes or workdirs that might be created in the future
                // character check also for working directory if not empty
                foreach (string schKey in currentConfig.schedule.Keys)
                {
                    int j = 1;
                    foreach (program targ in currentConfig.schedule[schKey].targets)
                    {
                        latestContextDesc = schKey + ", target #: " + j + "/" + currentConfig.schedule[schKey].targets.Count;

                        if (String.IsNullOrWhiteSpace(targ.path)  ||  isInvalidPath(targ.path))
                        {
                            throw new Exception("path, key '" + latestContextDesc + "': '" + targ.path + "' is invalid ");
                        }
                        if (!String.IsNullOrWhiteSpace(targ.workdir) && isInvalidPath(targ.workdir))
                        {
                            throw new Exception("workdir, key '" + latestContextDesc + "': '" + targ.workdir + "' is invalid ");
                        }
                        j++;
                    }
                }

                // Check interval >= 5
                lastStepDesc = "Checking timer interval";
                if (currentConfig.secinterval < 5)
                {
                    throw new Exception("Timer interval must be at least 5 sec");
                }

            }
            catch (Exception e)
            {
                return ConfigLoadResult.fail(
                    "Failed:  in context - " + latestContextDesc + "\n" + 
                    "Nice Error: " + lastStepDesc + "\n" 
                    + e.ToString());
            }

            if (currentConfig.echoconfig)
            {
                Console.WriteLine("Echo config is true,\n here is the validated config:\n"
                    + lineSeperator);
                string configEchoText = s.Serialize(currentConfig);
                Console.WriteLine(configEchoText + "\n" + lineSeperator);
            }

            return ConfigLoadResult.ok(currentConfig);

        }
    }

    public class ConfigLoadResult
    {
        public bool failed = true;
        public string error = "Initial error";
        public config result = null;

        public static ConfigLoadResult fail(string reason)
        {
            return new ConfigLoadResult()
            {
                error = reason
            };
        }

        public static ConfigLoadResult ok(config result)
        {
            return new ConfigLoadResult()
            {
                failed = false,
                result = result
            };
        }
    }

    public class program
    {
        public string args { get; set; } = "";
        public string path { get; set; } = "";
        public string workdir { get; set; } = "";
    }

    public class scheduleItem
    {
        public string pattern { get; set; } = "";
        public List<program> targets { get; set; } = new List<program>();
        public string timezone { get; set; } = "GMT Standard Time"; // EMPTY IS GMT=UTC
    }

    public class config
    {
        public bool echoconfig { get; set; } = true;
        public string version { get; set; } = "-1";
        public List<program> programs { get; set; } = new List<program>();
        public Dictionary<string, scheduleItem> schedule { get; set; } = new Dictionary<string, scheduleItem>();
        public int secinterval { get; set; } = 30;

    }

}
