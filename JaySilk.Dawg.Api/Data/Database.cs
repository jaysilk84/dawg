using System;
using System.IO;
using System.Collections.Generic;

namespace JaySilk.Dawg.Api.Data
{
    public class Database
    {
        private readonly string TargetFile = Path.Combine("Data", "enable.txt");

        public IEnumerable<string> GetRandomWords(int wordCount, int batchSize) {
            using (var r = new StreamReader(TargetFile)) {
                var rand = new Random();
                var count = 0;
                var batch = batchSize;

                while (r.Peek() >= 0 && count < wordCount) {
                    if (rand.Next(1, 2001) > 1 && batch == 0) {
                        r.ReadLine();
                        continue;
                    }

                    if (batch == 0) batch = batchSize;

                    var word = r.ReadLine();
                    yield return word;
                    count++;
                    batch--;
                }

                Console.WriteLine($"Wrote {count} of {wordCount} words.");
            }
        }

        public IEnumerable<string> GetWords() {
            using var r = new StreamReader(TargetFile);

            while (r.Peek() >= 0)
                yield return r.ReadLine();

            r.Close();
        }
    }


}