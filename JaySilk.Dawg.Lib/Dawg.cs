using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace JaySilk.Dawg.Lib
{
    public class Dawg
    {
        public Node Root { get; } = new Node(true);
        private readonly List<(Node, char, Node)> uncheckedNodes = new List<(Node, char, Node)>();
        private readonly HashSet<Node> minimizedNodes = new HashSet<Node>();
        private string previousWord = "";

        public int NodeCount => minimizedNodes.Count;

        public void Insert(string word) {
            var commonPrefix = 0;
            foreach (var i in Enumerable.Range(0, Math.Min(word.Length, previousWord.Length))) {
                if (word[i] != previousWord[i]) break;
                commonPrefix++;
            }

            Minimize(commonPrefix);

            var node = (uncheckedNodes.Count() == 0) ? Root : uncheckedNodes[^1].Item3;
            foreach (var c in word.Substring(commonPrefix)) {
                var next = new Node();
                node.Children[c] = next;
                uncheckedNodes.Add((node, c, next));
                node = next;
            }

            node.EndOfWord = true;
            previousWord = word;
        }

        public void Finish() {
            Minimize(0);
        }

        private void Minimize(int downTo) {
            for (var i = uncheckedNodes.Count() - 1; i > downTo - 1; i--) {
                var (parent, letter, child) = uncheckedNodes[i];
                if (minimizedNodes.Contains(child)) {
                    minimizedNodes.TryGetValue(child, out Node val);
                    parent.Children[letter] = val;
                }
                else
                    minimizedNodes.Add(child);

                uncheckedNodes.Remove(uncheckedNodes[^1]);
            }
        }

        public IEnumerable<string> MapToString(Node root) {
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

                yield return content.ToString();
            }
        }
    }
}