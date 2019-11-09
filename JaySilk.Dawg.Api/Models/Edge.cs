using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace JaySilk.Dawg.Api.Models
{
    public struct Edge
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
}