using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using JaySilk.Dawg.Scrabble;

namespace JaySilk.Dawg.Scrabble.Tests
{

    public class ScoreTests
    {
        public ScoreTests() {

        }

        [Fact]
        public void HasCorrectScoreWithDownWords() {
            var score = Score.ScoreWord(Data.WordGenerator.ParellelWordWithBlankOnDoubleLetterWithDownWords(), new Rack("A"));
            Assert.Equal(29, score);
        }

        [Fact]
        public void HasCorrectScoreWithTwoWordsOnADoubleWordBonus() {
            var score = Score.ScoreWord(Data.WordGenerator.ExtendVerticalWithHorizontalOnDoubleWord(), new Rack("A"));
            Assert.Equal(34, score);
        }

    }


}