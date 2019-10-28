using Microsoft.AspNetCore.SignalR;  
using System.Threading.Tasks;  
using System.Collections.Generic;
using JaySilk.Dawg.Api.Models;
using JaySilk.Dawg.Lib;

namespace JaySilk.Dawg.Api.Hubs  
{  
    public class GraphHub : Hub  
    {  
        public async Task NewMessage(int numWords, int batchSize)  
        {  
            var dawg = new Lib.Dawg();
            var db = new Data.Database();

            foreach (var w in db.GetRandomWords(numWords, batchSize)) {
                dawg.Insert(w);
                await Clients.Caller.SendAsync("MessageReceived", Serialize(dawg.Root));  
                await Task.Delay(2000);
            }
            dawg.Finish();

            await Clients.Caller.SendAsync("MessageReceived", Serialize(dawg.Root));  
            //return Serialize(dawg.Root);

            
        }  

        private IEnumerable<Edge> Serialize(Node root) {
            var stack = new Stack<Node>();
            var done = new HashSet<int>();
            var links = new List<Edge>();

            stack.Push(root);

            while (stack.Count > 0) {
                var node = stack.Pop();

                if (done.Contains(node.Id)) continue;

                if (node.Children.Count == 0) 
                    links.Add(new Edge(new Vertex(node), null, null));
                //yield return new Edge(new Vertex(node), null, null);

                foreach (var (key, child) in node.Children) {
                    links.Add(new Edge(new Vertex(node), new Vertex(child), key));
                    stack.Push(child);
                    done.Add(node.Id);
                    //yield return new Edge(new Vertex(node), new Vertex(child), key);
                }
            }

            return links;
        }

    }  
}  