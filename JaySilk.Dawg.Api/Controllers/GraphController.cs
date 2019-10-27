using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using JaySilk.Dawg.Api.Models;
using JaySilk.Dawg.Lib;

namespace JaySilk.Dawg.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GraphController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Edge> Get(int numWords = 10, int batchSize = 2 ) {
            var dawg = new Lib.Dawg();
            var db = new Data.Database();

            foreach (var w in db.GetRandomWords(numWords, batchSize)) {
                dawg.Insert(w);
            }
            dawg.Finish();

            return Serialize(dawg.Root);
        }


        private IEnumerable<Edge> Serialize(Node root) {
            var stack = new Stack<Node>();
            var done = new HashSet<int>();
            var links = new List<Edge>();

            stack.Push(root);

            while (stack.Count() > 0) {
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