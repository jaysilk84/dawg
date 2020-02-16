using System;
using System.Collections.Generic;
using System.IO;

namespace JaySilk.Dawg.Scrabble
{
    public class Database
    {
        private readonly string TargetFile = "enable.txt";
        public IEnumerable<string> GetWords() {
            using var r = new StreamReader(TargetFile);

            while (r.Peek() >= 0)
                yield return r.ReadLine();

            r.Close();
        }

    }
}