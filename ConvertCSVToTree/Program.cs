using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConvertCSVToTree
{
    class Program
    {
        private class Node
        {
            public Node()
            {
                Tree = new List<Node>();
            }

            public string BindTo { get; set; }
            public string Name { get; set; }
            public IList<Node> Tree { get; set; }
        }

        private static Dictionary<string, Node> nodes = new Dictionary<string, Node>();

        static void Main(string[] args)
        {
            var input = string.Empty;
            var output = string.Empty;
            if(args.Length > 1)
            {
                input = args[0];
                output = args[1];
            }
            
            if(string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                System.Console.WriteLine("Expected 2 parameters : first input csv path, second output json path");
                return;
            }
            var rootNode = new Node
            {
                BindTo = ""
            };
            nodes.Add("root", rootNode);

            Console.WriteLine("Reading csv File");
            var lines = File.ReadAllLines(input);
            var levels = new string[] { };

            Console.WriteLine("Going through lines");
            var lineIndex = 0;
            foreach (var line in lines)
            {
                var fields = line.Split(";");
                if(lineIndex == 0) 
                {
                    levels = fields;
                    ++lineIndex;
                    continue;
                }

                var key = "";
                for(var i = 0; i < fields.Length; ++i)
                {
                    var field = fields[i].Trim();
                    var parentKey = string.IsNullOrEmpty(key) ? "root":key;
                    key += field + "_";

                    if (!nodes.ContainsKey(key) && !string.IsNullOrEmpty(field))
                    {
                        nodes.Add(key, new Node { BindTo = levels[i], Name = field });
                        var parent = nodes[parentKey];
                        var node = nodes[key];
                        parent.Tree.Add(node);
                    }
                }
            }

            Console.WriteLine("Exporting data");
            File.WriteAllText(output, JsonConvert.SerializeObject(levels) + Environment.NewLine + JsonConvert.SerializeObject(rootNode.Tree));
        }
    }
}
