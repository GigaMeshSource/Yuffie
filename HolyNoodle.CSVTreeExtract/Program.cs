using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HolyNoodle.CSVTreeExtract
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
        private const string CSV_PATH = @"C:\Users\kbrif\Documents\Classeur1.csv";
        private const string TREE_PATH = @"C:\Users\kbrif\Documents\tree.json";

        static void Main(string[] args)
        {
            var rootNode = new Node
            {
                BindTo = ""
            };
            nodes.Add("root", rootNode);

            Console.WriteLine("Reading csv File");
            var lines = File.ReadAllLines(CSV_PATH);

            var levels = new string[] {  "Distributeur","Sous-distributeur","Niveau 1","Niveau 2 ","Niveau 3","Niveau 4","Codes Agences" };

            Console.WriteLine("Going through lines");
            foreach (var line in lines)
            {
                var fields = line.Split(";");
                var key = "";
                for(var i = 0; i < fields.Length; ++i)
                {
                    var field = fields[i].Trim();
                    var parentKey = string.IsNullOrEmpty(key) ? "root":key;
                    key += field + "_";

                    if (!nodes.ContainsKey(key))
                    {
                        nodes.Add(key, new Node { BindTo = levels[i], Name = field });
                        var parent = nodes[parentKey];
                        var node = nodes[key];
                        parent.Tree.Add(node);
                    }
                }
            }

            Console.WriteLine("Exporting data");
            File.WriteAllText(TREE_PATH, JsonConvert.SerializeObject(rootNode.Tree));
        }
    }
}
