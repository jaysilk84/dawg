using System;
using System.Collections.Generic;
using System.Linq;

namespace JaySilk.Dawg.Scrabble
{

    public class Tile
    {
        public char Letter { get; }
        public char OriginalLetter { get; }
        public short Value { get; }
        public bool IsBlank { get; }

        public Tile(char letter, short value, bool isBlank) {
            Letter = letter;
            Value = value;
            IsBlank = isBlank;
            OriginalLetter = Letter;
        }

        private Tile(Tile t) {
            //return new Tile()
        }

    }


}