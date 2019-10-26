using System.Text.Json;
using System.Text.Json.Serialization;
using JaySilk.Dawg.Lib;

namespace JaySilk.Dawg.Api.Models
{
    public struct Vertex
    {
        public Vertex(int id, bool endOfWord, bool isRoot) {
            Id = id;
            EndOfWord = endOfWord;
            IsRoot = isRoot;
        }

        public Vertex(Node node) {
            EndOfWord = node.EndOfWord;
            Id = node.Id;
            IsRoot = node.IsRoot;
        }

        [JsonPropertyName("endOfWord")]
        public bool EndOfWord { get; }
        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("isRoot")]
        public bool IsRoot {get; }
    }

}