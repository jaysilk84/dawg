using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace JaySilk.Dawg.Cli
{
    class Program
    {
        static void Main(string[] args) {
            var root = new Node();
            var uncheckedNodes = new List<(Node, char, Node)>();
            var minimizedNodes = new HashSet<Node>();
            var previousWord = "";

            //var words = new string[] { "cities", "city", "pities", "pity" }; //, pities, pity" };

            //var words = new string[] { "blip", "cat", "catnip", "cats" };
            var words = new string[] { "cat", "catnip", "zcatnip" };

            foreach (var w in words) {
                insert(w);
            }

            minimize(0);

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
            printMap(root);

            string serialize(Node root) {
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = new DictionaryAsArrayResolver();

                return JsonConvert.SerializeObject(root, settings);
            }

            void insert(string word) {
                var commonPrefix = 0;
                foreach (var i in Enumerable.Range(0, Math.Min(word.Length, previousWord.Length))) {
                    if (word[i] != previousWord[i]) break;
                    commonPrefix++;
                }

                minimize(commonPrefix);

                var node = (uncheckedNodes.Count() == 0) ? root : uncheckedNodes[^1].Item3;
                foreach (var c in word.Substring(commonPrefix)){
                    var next = new Node();
                    node.Children[c] = next;
                    uncheckedNodes.Add((node, c, next));
                    node = next;
                }

                node.EndOfWord = true;
                previousWord = word;
            }

            void printMap(Node root) {
                var stack = new Stack<Node>();
                var done = new HashSet<int>();

                
                stack.Push(root);

                while (stack.Count() > 0) {
                    var content = new StringBuilder();
                    var node = stack.Pop();

                    if (done.Contains(node.Id)) continue;

                    content.AppendLine($"{node.Id,-5} {node.ToString()}");
                    
                    foreach(var (key, child) in node.Children.OrderByDescending(x => x.Key)) {
                        content.AppendLine($"{key, 5} goto {child.Id}");
                        stack.Push(child);
                        done.Add(node.Id);
                    }

                    Console.WriteLine(content.ToString());

                }
            }

            void minimize(int downTo) {
                for (var i = uncheckedNodes.Count() - 1; i > downTo - 1; i--) {
                    var (parent, letter, child) = uncheckedNodes[i];
                    if (minimizedNodes.Contains(child)) {
                        minimizedNodes.TryGetValue(child, out Node val);
                        parent.Children[letter] = val;
                    } else 
                        minimizedNodes.Add(child);

                    uncheckedNodes.Remove(uncheckedNodes[^1]);
                }
            }

            void printJson(Node root) {
                Console.WriteLine(serialize(root));
            }

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
