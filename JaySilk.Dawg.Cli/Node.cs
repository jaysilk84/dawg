using System.Collections.Generic;

public class Node
{
    public Node(char key) => Key = key;

    public char Key { get; }
    public Dictionary<char, Node> Children { get; } = new Dictionary<char, Node>();

    public Node AddChild(char key) {
        var n = new Node(key);
        Children.Add(key, n);
        return n;

    }
    public Node AddChild(Node node) {
        Children.Add(node.Key, node);
        return node;
    }
}