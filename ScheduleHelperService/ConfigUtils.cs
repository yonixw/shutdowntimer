using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

// Thanks to https://archive.is/5TLci for MergingParser

namespace ScheduleHelperService
{
    public static class ConfigUtils
    {
        static IDeserializer d = new DeserializerBuilder() // TODO how to validate empty proprs?
                .Build();

        static ISerializer s = new SerializerBuilder()
            .WithMaximumRecursion(50)
            .Build();

        public static bool TryLoad(string path)
        {
            bool parsed = false;

            string yamlText = File.ReadAllText(path);

            // Step 1 - Check if a version is present:




            config config_loaded = null;
            using (TextReader sr = new StringReader(yamlText))
            {
                config_loaded = d.Deserialize<config>(new MergingParser(new Parser(sr)));
            }
            

            Console.WriteLine(s.Serialize(config_loaded));

            return;



            //var Obj = s.Deserialize(yamlText);
            //Console.WriteLine(s.Serialize(Obj));

            var config_v1 = new config();
            config_v1.version = "1";
            
            program p1 = new program();
            p1.path = @"C:\path\to\program\1.exe";
            p1.args = "-SpaceAfter ";
            program p2 = new program();
            p2.path = @"C:\path\to\program\2.exe";
            p2.args = " -SpaceBefore";
            program p3 = new program();
            p3.path = @"C:\path\to\program\3.exe";

            config_v1.programs = new List<program>() { p1, p2, p3 };

            scheduleItem si1 = new scheduleItem();
            si1.pattern = "* * * * 45";
            si1.timezone = "UTC";
            si1.targets = new List<program>() { p1, p3 };

            config_v1.schedule = new Dictionary<string, scheduleItem>() { { "weekday", si1 } };

            Console.WriteLine(s.Serialize(config_v1));

            Console.WriteLine("OK");
        }
    }

    public class ConfigLoadResult
    {
        public bool failed = true;
        public string error = "Initial error";
        public config result = null;

        public ConfigLoadResult fail(string reason)
        {
            return new ConfigLoadResult()
            {
                error = reason
            };
        }

        public ConfigLoadResult ok(config result)
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
        public string timezone { get; set; } = "UTC";
    }

    public class config
    {
        public string version { get; set; } = "-1";
        public List<program> programs { get; set; } = new List<program>();
        public Dictionary<string, scheduleItem> schedule { get; set; } = new Dictionary<string, scheduleItem>();
        public int secinterval { get; set; } = 30;

    }

}
