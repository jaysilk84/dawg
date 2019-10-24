using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

public class Node
{
    private static int NodeId = -1;

    public Node() => Id = Interlocked.Increment(ref NodeId);

    //public char Key { get; }
    public Dictionary<char, Node> Children { get; } = new Dictionary<char, Node>();
    public int Id { get; }
    public bool EndOfWord { get; set; } = false;

    public Node AddChild(char key) {
        var n = new Node();
        Children.Add(key, n);
        return n;
    }

    public override string ToString() {
        var hash = new StringBuilder();
        hash.Append(EndOfWord.ToString());
        //hash.Append(this.Id);
        foreach (var (label, node) in Children.OrderBy(x => x.Key)) {
        //foreach (var (label, node) in Children) {
            hash.Append(label);
            hash.Append(node.Id);
        }

        return hash.ToString();
    }

    public override int GetHashCode() {
        return this.ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj == null) return false;

        return this.ToString() == obj.ToString();
    }

    // public Node AddChild(Node node) {
    //     Children.Add(node.Key, node);
    //     return node;
    // }
}