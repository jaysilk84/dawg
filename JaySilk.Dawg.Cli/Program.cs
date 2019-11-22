using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using JaySilk.Dawg.Lib;
using JaySilk.Dawg.Scrabble;

namespace JaySilk.Dawg.Cli
{
    class Program
    {
        public struct Vertex
        {
            public Vertex(int id, bool endOfWord) {
                Id = id;
                EndOfWord = endOfWord;
            }

            public Vertex(Node node) {
                EndOfWord = node.EndOfWord;
                Id = node.Id;
            }

            [JsonProperty("endOfWord")]
            public bool EndOfWord { get; }
            [JsonProperty("id")]
            public int Id { get; }
        }

        private struct Edge
        {
            public Edge(Vertex source, Vertex? target, char? key) {
                Source = source;
                Target = target;
                Key = key;
            }

            [JsonProperty("source")]
            public Vertex Source { get; }
            [JsonProperty("target")]
            public Vertex? Target { get; }
            [JsonProperty("key")]
            public char? Key { get; }
        }

        static void Main(string[] args) {
     
             var s = new Scrabble.Scrabble(new List<Square>());
             s.PrintBoard(s.Board);
             Console.WriteLine("");
             s.PrintBoard(s.Transpose(s.Board));
     
     
     
            return;


            var dawg = new Lib.Dawg();

            // var root = new Node();
            // var uncheckedNodes = new List<(Node, char, Node)>();
            // var minimizedNodes = new HashSet<Node>();
            // var previousWord = "";
            const string TargetFile = "enable.txt";
            const string DestinationFile = "output.json";
            const int WordCount = 100;
            const int BatchSize = 10;
            var rand = new Random();

            //var words = new string[] { "cities", "city", "pities", "pity" }; //, pities, pity" };

            //var words = new string[] { "blip", "cat", "catnip", "cats" };
            //var words = new string[] { "cat", "catnip", "zcatnip" };

            // foreach (var w in words) {
            //    dawg.Insert(w);
            // }

            // dawg.Finish();

            // printMap(dawg.Root);
            // //Console.WriteLine($"Nodes {dawg.Nodes.Count()}");
            // foreach (var n in dawg.Nodes)
            //     Console.WriteLine($"{n.Id}");
            // Console.WriteLine($"Node count {dawg.NodeCount}");
            // Console.WriteLine($"Edge count {dawg.EdgeCount}");

            // return;

            using (var r = new StreamReader(TargetFile)) {
                var count = 0;
                var batch = BatchSize;

                while (r.Peek() >= 0 && count < WordCount) {
                    if (rand.Next(1, 2001) > 1 && batch == 0) {
                        r.ReadLine();
                        continue;
                    }

                    if (batch == 0) batch = BatchSize;

                    var word = r.ReadLine();
                    dawg.Insert(word);
                    count++;
                    batch--;

                    Console.WriteLine($"Adding {word} ({count}/{WordCount})");
                }

                Console.WriteLine($"Wrote {count} of {WordCount} words. Node count: {dawg.NodeCount}");
            }

            dawg.Finish();
            Console.WriteLine($"Final Node count {dawg.NodeCount}");
           Console.WriteLine($"Edge count {dawg.EdgeCount}");

//            minimize(0);

            // foreach (var w in words) {

            //     var position = root;

            //     foreach (var c in w) {

            //         if (position.Children.ContainsKey(c)) {
            //             position = position.Children[c];
            //             continue;
            //         }

            //         //position = findStart(c, position) ?? position;

            //         position = position.AddChild(c);
            //     }
            //     position.EndOfWord = true;
            // }

            //printJson(root);
            //printMap(root);
            //printMinimized();
            // Console.WriteLine(serializeMap(root));


            // string serialize(Node root) {
            //     var settings = new JsonSerializerSettings();
            //     settings.Formatting = Formatting.Indented;
            //     settings.ContractResolver = new DictionaryAsArrayResolver();

            //     return JsonConvert.SerializeObject(root, settings);
            // }

            // void insert(string word) {
            //     var commonPrefix = 0;
            //     foreach (var i in Enumerable.Range(0, Math.Min(word.Length, previousWord.Length))) {
            //         if (word[i] != previousWord[i]) break;
            //         commonPrefix++;
            //     }

            //     minimize(commonPrefix);

            //     var node = (uncheckedNodes.Count() == 0) ? root : uncheckedNodes[^1].Item3;
            //     foreach (var c in word.Substring(commonPrefix)) {
            //         var next = new Node();
            //         node.Children[c] = next;
            //         uncheckedNodes.Add((node, c, next));
            //         node = next;
            //     }

            //     node.EndOfWord = true;
            //     previousWord = word;
            // }

            void printMap(Node root) {
                var stack = new Stack<Node>();
                var done = new HashSet<int>();


                stack.Push(root);

                while (stack.Count() > 0) {
                    var content = new StringBuilder();
                    var node = stack.Pop();

                    if (done.Contains(node.Id)) continue;

                    content.AppendLine($"{node.Id,-5} {node.ToString()}");

                    foreach (var (key, child) in node.Children.OrderByDescending(x => x.Key)) {
                        content.AppendLine($"{key,5} goto {child.Id}");
                        stack.Push(child);
                        done.Add(node.Id);
                    }

                    Console.WriteLine(content.ToString());

                }
            }

            // string serializeMap(Node root) {
            //     var stack = new Stack<Node>();
            //     var done = new HashSet<int>();
            //     var links = new List<Edge>();

            //     stack.Push(root);

            //     while (stack.Count() > 0) {
            //         var node = stack.Pop();

            //         if (done.Contains(node.Id)) continue;

            //         if (node.Children.Count == 0)
            //             links.Add(new Edge(new Vertex(node), null, null));

            //         foreach (var (key, child) in node.Children.OrderByDescending(x => x.Key)) {
            //             links.Add(new Edge(new Vertex(node), new Vertex(child), key));
            //             stack.Push(child);
            //             done.Add(node.Id);
            //         }
            //     }

            //     using (StreamWriter writer = new StreamWriter(DestinationFile))
            //     using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            //     {
            //         JsonSerializer ser = new JsonSerializer();
            //         ser.Serialize(jsonWriter, links);
            //         jsonWriter.Flush();
            //     }

            //     return "";
            //     //return JsonConvert.SerializeObject(links, Formatting.Indented);
            // }

            // void minimize(int downTo) {
            //     for (var i = uncheckedNodes.Count() - 1; i > downTo - 1; i--) {
            //         var (parent, letter, child) = uncheckedNodes[i];
            //         if (minimizedNodes.Contains(child)) {
            //             minimizedNodes.TryGetValue(child, out Node val);
            //             parent.Children[letter] = val;
            //         }
            //         else
            //             minimizedNodes.Add(child);

            //         uncheckedNodes.Remove(uncheckedNodes[^1]);
            //     }
            // }

            // void printJson(Node root) {
            //     Console.WriteLine(serialize(root));
            // }

            // void printList(Node root, int level = 1) {
            //     Console.WriteLine($"{level}:{root.Key}");

            //     foreach (var c in root.Children)
            //         printList(c.Value, level + 1);
            // }

            // Node findStart(char c, Node root) {
            //     if (root == null) return null;

            //     if (root.Key == c) return root;

            //     foreach (var p in root.Children) {
            //         if (p.Key == c) return p.Value;
            //     }

            //     return null;
            // }
        }

    }
}
