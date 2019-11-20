using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using JaySilk.Dawg.Scrabble;

namespace JaySilk.Dawg.Api.Controllers
{
    [ApiController]
    [Route("scrabble/[controller]")]
    public class MoveController : ControllerBase
    {
        
        [HttpGet]
        public ActionResult Get() {
            // var words = new string[] { "YOK", "YET", "ON", "KA", "ENAMELS" }.OrderBy(x => x);
            // var dawg = new Lib.Dawg();
            // foreach (var w in words)
            //     dawg.Insert(w);

            // dawg.Finish();
            // var scrabble = new Scrabble.Scrabble(dawg);
            var scrabble = new Scrabble.Scrabble(BoardController.WordList);
            return Ok(scrabble.PlayableWords.OrderByDescending(x => x.Score).Take(50));
        }

        [HttpGet("board")]
        public ActionResult GetBoard() {
            var scrabble = new Scrabble.Scrabble(BoardController.WordList);

            return Ok(scrabble.PlayableBoards.OrderByDescending(x => x.PlayedWord.Score).Take(50));

        }

        // public ActionResult Get() {
        //     var scrabble = new Scrabble.Scrabble(BoardController.WordList);
        //     return Ok(scrabble.PlayableWords.Select(w => new {
        //         start = new {x = w.Start.X, y =w.Start.Y},
        //         end = new {x = w.End.X, y =w.End.Y},
        //         word = w.Word,
        //         score = w.Score
        //     }));
        // }
    }
}