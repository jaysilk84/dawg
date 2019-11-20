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
    public class BoardController : ControllerBase
    {
        public static Lib.Dawg WordList = BuildDawg(); // Dangerous: share this for now
        
        [HttpGet]
        public ActionResult Get() {
            var scrabble = new Scrabble.Scrabble(WordList);
            return Ok(scrabble.SerializeBoard(scrabble.Board));
        }

        [HttpGet("rules")]
        public ActionResult GetLetters() {
            return Ok(new { 
                letters = Scrabble.Score.Letters,
                bonuses = Scrabble.Score.Bonuses
            });
        }

        private static Lib.Dawg BuildDawg() {
            var db = new Data.Database();
            var dawg = new Lib.Dawg();

            foreach (var w in db.GetWords())
                dawg.Insert(w.ToUpper());

            dawg.Finish();
            return dawg;
        }




    }
}