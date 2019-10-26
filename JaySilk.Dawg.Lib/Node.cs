using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace JaySilk.Dawg.Lib
{
    public class Node
    {
        private static int NodeId = -1;

        public Dictionary<char, Node> Children { get; } = new Dictionary<char, Node>();
        public int Id { get; }
        public bool EndOfWord { get; set; } = false;
        public bool IsRoot {get;}

        public Node() : this(false) {
        }

        public Node(bool isRoot) {
            IsRoot = isRoot;
            Id = Interlocked.Increment(ref NodeId);
        }

        public Node AddChild(char key) {
            var n = new Node();
            Children.Add(key, n);
            return n;
        }

        public override string ToString() {
            var hash = new StringBuilder(EndOfWord.ToString());

            foreach (var (label, node) in Children.OrderBy(x => x.Key)) {
                hash.Append(label);
                hash.Append(node.Id);
            }

            return hash.ToString();
        }

        public override int GetHashCode() => this.ToString().GetHashCode();

        public override bool Equals(object? obj) => (obj == null) ? false : this.ToString() == obj.ToString();
    }
}