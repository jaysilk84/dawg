using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JaySilk.Dawg.Scrabble;

namespace JaySilk.Dawg.Scrabble.Tests.Data
{

    public static class WordGenerator
    {
        public static List<Square> ParellelWordWithBlankOnDoubleLetterWithDownWords() {
            //   1 2 3  <-- existing words
            //   1 2 3   
            //   1 2 3  
            //   X X X  <-- played word
            //
            var playedWord = "EMS".ToSquares();

            playedWord[0].CrossChecks = TileHelper.BuildCrossCheckFromWord(playedWord[0], "DEE");
            // First position is a double letter
            playedWord[0].SetLastLetterAsPlayed().SetIsPlayed().SetMultiplierDoubleLetter();

            playedWord[1].CrossChecks = TileHelper.BuildCrossCheckFromWord(playedWord[1], "ISM");
            // Second letter is a blank (M)
            playedWord[1].SetLastLetterAsPlayed().SetIsBlank().SetIsPlayed();
            
            playedWord[2].CrossChecks = TileHelper.BuildCrossCheckFromWord(playedWord[2], "WHIPPETS");
            playedWord[2].SetLastLetterAsPlayed().SetIsPlayed();

            return playedWord;
        }

        public static List<Square> ExtendVerticalWithHorizontalOnDoubleWord() {
            // 1      <-- existing word
            // 1      
            // 1
            // X X X  <-- played word
            //
            var playedWord = "SHAME".ToSquares();

            playedWord[0].CrossChecks = TileHelper.BuildCrossCheckFromWord(playedWord[0], "BETS");
            // First position is a double word and extends the down word
            playedWord[0].SetLastLetterAsPlayed().SetMultiplierDoubleWord();

            return playedWord;
        }

      

    }


}