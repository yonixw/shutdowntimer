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
    public class ConfigUtils
    {
        public static void TryLoad(string path)
        {
            IDeserializer d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            ISerializer s = new SerializerBuilder()
                .WithMaximumRecursion(50)
                .Build(); 

            string yamlText = File.ReadAllText(path);
            config_v1 config_loaded = null;
            using (TextReader sr = new StringReader(yamlText))
            {
                config_loaded = d.Deserialize<config_v1>(new MergingParser(new Parser(sr)));
            }
            

            Console.WriteLine(s.Serialize(config_loaded));

            return;



            //var Obj = s.Deserialize(yamlText);
            //Console.WriteLine(s.Serialize(Obj));

            var config_v1 = new config_v1();
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

    public class program
    {
        public string args { get; set; }
        public string path { get; set; }
        public string workdir { get; set; }
    }

    public class scheduleItem
    {
        public string pattern { get; set; }
        public List<program> targets { get; set; }
        public string timezone { get; set; }
    }

    public class config_v1
    {
        public string version { get; set; }
        public List<program> programs { get; set; }
        public Dictionary<string, scheduleItem> schedule { get; set; }

    }

    public class versionOnly
    {
        public string version { get; set; }
    }
}
