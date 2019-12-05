using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Trie
{
    public class Program
    {
        /// <summary>
        /// On my machine adding to a List took ~250 ticks
        /// Adding to a trie took ~438
        /// I could remove some overhead by reading the stream out to an array first then to the trie / list
        /// However samples stayed consistent to the point of stabilizing on the same exact number of ticks per run.
        /// 
        /// Taking twice as long as an list, isn't great, but remember the trie is always ordered while a list has to be 
        /// reordered after every add
        /// 
        /// Grab the words file from: https://raw.githubusercontent.com/dwyl/english-words/master/words.txt
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var start = Environment.TickCount;
            Console.WriteLine(start);
            var trie = new TrieHarder();
            //var list = new List<string>();
            var reader = new StreamReader(@"C:\Git\Trie\Words.txt");
            string line;
            int count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                count++;
                trie.Add(line);

                /*if (!Regex.IsMatch(line, "[^a-z]"))
                {
                    list.Add(line);
                }*/
                
                //Uncomment to get progress during adds
                /*if (count % 1000 == 0)
                {

                    Console.WriteLine(trie.Records.Length);
                    Console.WriteLine(line);
                }*/
            }

            var end = Environment.TickCount;
            Console.WriteLine(end);
            Console.WriteLine(end - start);
            //Console.WriteLine(trie.GetTotalRecords());
            //trie.Out(new int[0]);
            Console.WriteLine("Fin");
        }
    }

    public class TrieHarder
    {
        private const int GrowBy = NumberOfLetters * 1126006;
        private const int NumberOfLetters = 26;
        private const int FirstLetterCode = 97;

        public int _nextInsert = 1;

        public int[] Records = new int[GrowBy];

        public void Add(string record)
        {
            if (record.Length == 0)
            {
                return;
            }

            //Algorithm can only handle lowercase letters. That means no spaces, no capitals, and no numbers.
            if (Regex.IsMatch(record, "[^a-z]"))
            {
                return;
            }

            var encodedRecord = record.ToLower().Select(x => x - FirstLetterCode);
            var enumerator = encodedRecord.GetEnumerator();
            Add(enumerator);
        }

        private bool Add(IEnumerator<int> enumerator, int section = 0)
        {
            if (section < 0)
            {
                section = section * -1;
            }

            if (!enumerator.MoveNext())
            {
                //if we've reach the last value of the enumerator we must be on a leaf.
                return false;
            }

            var subIndexToUpdate = enumerator.Current;
            var fullIndexToUpdate = (section * NumberOfLetters) + subIndexToUpdate;
            var nextSection = Records[fullIndexToUpdate];

            if (nextSection == 0)
            {
                ExpandRecords();
                nextSection = _nextInsert++;
                Records[fullIndexToUpdate] = nextSection;
            }

            var couldAdd = Add(enumerator, nextSection);
            if (!couldAdd && nextSection > 0)
            {
                Records[fullIndexToUpdate] = nextSection * -1;
            }
            return true;
        }

        private void ExpandRecords()
        {
            if ((_nextInsert + 1) * NumberOfLetters > Records.Length)
            {
                var expandedRecords = new int[Records.Length + GrowBy];
                Records.CopyTo(expandedRecords, 0);

                for (int i = Records.Length; i < expandedRecords.Length; i++)
                {
                    expandedRecords[i] = 0;
                }
                Records = expandedRecords;
            }
        }

        public int GetTotalRecords()
        {
            return _nextInsert * NumberOfLetters;
        }

//Need to change to a yield return
        public void Out(int[] path, int section = 0)
        {
            if (section < 0)
            {
                var fullPath = string.Concat(path.Select(x => (char)(x + FirstLetterCode)));
                Console.WriteLine(fullPath);
                section = section * -1;
            }

            var low = section * NumberOfLetters;
            var high = ((section + 1) * NumberOfLetters) - 1;
            for (int i = low; i <= high; i++)
            {
                var nextSection = Records[i];
                if (Records[i] != 0)
                {
                    var newPath = new int[path.Length + 1];
                    path.CopyTo(newPath, 0);
                    newPath[path.Length] = i - low;

                    Out(newPath, nextSection);
                }
            }
        }
    }
}
