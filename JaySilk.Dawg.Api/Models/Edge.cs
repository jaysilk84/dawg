using System.Text.Json;
using System.Text.Json.Serialization;

namespace JaySilk.Dawg.Api.Models
{
    public struct Edge
    {
        public Edge(Vertex source, Vertex? target, char? key) {
            Source = source;
            Target = target;
            Key = key;
        }

        [JsonPropertyName("source")]
        public Vertex Source { get; }
        [JsonPropertyName("target")]
        public Vertex? Target { get; }
        [JsonPropertyName("key")]
        public char? Key { get; }
    }
}