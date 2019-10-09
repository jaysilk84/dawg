using System;
using Newtonsoft.Json;

namespace JaySilk.Dawg.Cli
{
    class Program
    {
        static void Main(string[] args) {
            Node root = null;
            var words = new string[] { "cities", "city" }; //, pities, pity" };

            foreach (var w in words) {

                Node position = root;

                foreach (var c in w) {

                    if (root == null) {
                        root = new Node(c);
                        position = root;
                        continue;
                    }

                    position = findStart(c, position) ?? position;

                    if (position.Key != c)
                        position = position.AddChild(c);
                }
            }

            printJson(root);

            string serialize(Node root) {
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = new DictionaryAsArrayResolver();

                return JsonConvert.SerializeObject(root, settings);
            }

            void printJson(Node root) {
                Console.WriteLine(serialize(root));
            }

            void printList(Node root, int level = 1) {
                Console.WriteLine($"{level}:{root.Key}");

                foreach (var c in root.Children)
                    printList(c.Value, level + 1);
            }

            Node findStart(char c, Node root) {
                if (root == null) return null;

                if (root.Key == c) return root;

                foreach (var p in root.Children) {
                    if (p.Key == c) return p.Value;
                }

                return null;
            }
        }

    }
}
