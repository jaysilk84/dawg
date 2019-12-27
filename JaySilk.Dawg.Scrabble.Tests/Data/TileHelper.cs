using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using JaySilk.Dawg.Scrabble;

namespace JaySilk.Dawg.Scrabble.Tests.Data
{

    public static class TileHelper
    {
        // extensions
        public static List<Square> ToSquares(this string word) {
            return BuildVerticalSquareListFromWord(word, new Point(0, 0));
        }

        public static Square SetLastLetterAsPlayed(this Square s) {
            var check = s.CrossChecks[s.Tile.Value];
            check.Squares[^1].Position = s.Position;
            return s;
        }

        public static Square SetIsPlayed(this Square s) {
            s.IsPlayed = true;
            return s;
        }
        public static Square SetIsBlank(this Square s) {
            s.HasBlank = true;
            return s;
        }

        public static Square SetMultiplierDoubleLetter(this Square s) {
            s.Multiplier = new Score.Multiplier(2, Score.MultiplierType.Letter);
            return s;
        }

        public static Square SetMultiplierDoubleWord(this Square s) {
            s.Multiplier = new Score.Multiplier(2, Score.MultiplierType.Word);
            return s;
        }

        public static List<Square> BuildVerticalSquareListFromWord(string word, Point start) {
            return word.Select((c, i) => new Square(c, new System.Drawing.Point(start.X, start.Y + i))).ToList();
        }

        public static List<Square> BuildHorizontalSquareListFromWord(string word, Point start) {
            return word.Select((c, i) => new Square(c, new System.Drawing.Point(start.X + 1, start.Y))).ToList();
        }

        public static CrossCheck BuildCrossCheckFromWord(string word) {
            // give cross checks a different starting coordinate for uniqueness
            return new CrossCheck(new Word(word), BuildVerticalSquareListFromWord(word, new Point(1, 0)));
        }

        public static Dictionary<char, CrossCheck> BuildCrossCheckFromWord(Square square, string word) {
            return BuildCrossCheckFromWord(square.Tile.Value, word);
        }
        public static Dictionary<char, CrossCheck> BuildCrossCheckFromWord(char key, string word) {
            return new Dictionary<char, CrossCheck>() { { key, BuildCrossCheckFromWord(word) } };
        }

        public static int GetWordValue(string word) {
            return word.Aggregate<char, int>(0, (x, c) => x += Score.Letters[c]);
        }

    }


}