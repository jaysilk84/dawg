using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using JaySilk.Dawg.Lib;

// TEMP

namespace JaySilk.Dawg.Scrabble
{
    public class Scrabble
    {
        private readonly Lib.Dawg dawg = new Lib.Dawg();
        public const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int MAX_ROWS = 17;
        private static readonly int MAX_COLS = 17;
        private static readonly int MIN_ROW = 1;
        private static readonly int MIN_COL = 1;
        private static readonly int MAX_ROW = MAX_ROWS - 1;
        private static readonly int MAX_COL = MAX_COLS - 1;
        private bool transposed_hack = false;

        public Square[,] Board = new Square[MAX_ROWS, MAX_COLS];
        public HashSet<Point> Anchors = new HashSet<Point>();
        public List<WordModel> PlayableWords = new List<WordModel>();
        public string Rack = "KVTYISO";

        public Scrabble(Lib.Dawg wordList = null) {
            if (wordList == null)
                BuildDawg();
            else
                dawg = wordList;

            BuildBlankBoard();

            // Board[1, 2].Tile = 'J'; 
            // Board[2, 2].Tile = 'A';
            // Board[3, 2].Tile = 'Y';
            // Board[2, 3].Tile = 'S';
            // Board[2, 4].Tile = 'S';
            // Board[4, 2].Tile = 'S';
            // Board[4, 3].Tile = 'A';
            // Board[4, 4].Tile = 'D';

            // Board[1, 1].Tile = 'B';
            // Board[1, 2].Tile = 'R';
            // Board[1, 3].Tile = 'I';
            // Board[1, 4].Tile = 'C';
            // Board[1, 5].Tile = 'K';

            // Board[2, 1].Tile = 'A';
            // Board[3, 1].Tile = 'D';

            // Board[3, 2].Tile = 'I';
            // Board[3, 3].Tile = 'E';

            Board[7, 3].Tile = 'T';
            Board[7, 4].Tile = 'U';
            Board[7, 5].Tile = 'B';

            Board[8, 4].Tile = 'S';
            Board[8, 5].Tile = 'A';
            Board[8, 6].Tile = 'I';
            Board[8, 7].Tile = 'N';
            Board[8, 8].Tile = 'T';

            Board[7, 8].Tile = 'E';
            Board[7, 9].Tile = 'N';
            Board[7, 10].Tile = 'A';
            Board[7, 11].Tile = 'M';
            Board[7, 12].Tile = 'E';
            Board[7, 13].Tile = 'L';

            Board[4, 14].Tile = 'F';
            Board[5, 14].Tile = 'O';
            Board[6, 14].Tile = 'G';
            Board[7, 14].Tile = 'S';

            Board[3, 15].Tile = 'M';
            Board[4, 15].Tile = 'I';
            Board[5, 15].Tile = 'X';

            var transposedBoard = Transpose(Board);
            BuildAnchorList(Board);
            BuildAnchorList(transposedBoard);

            ApplyAnchorsToBoard(Board);
            ApplyAnchorsToBoard(transposedBoard, true);

            CalculateAllCrossChecks(Board);
            CalculateAllCrossChecks(transposedBoard);

            PrintBoard(transposedBoard);

            for (var r = 0; r < MAX_ROWS; r++) {
                ProcessRow(Board, r);
                ProcessRow(transposedBoard, r);
            }

            //Console.WriteLine("Found " + wordCount + " words");
            // ProcessRow(Board, 2);
            // ProcessRow(transposedBoard, 2);
        }

        private void ApplyAnchorsToBoard(Square[,] board, bool transpose = false) {
            foreach (var a in Anchors) {
                var r = a.Y;
                var c = a.X;
                if (transpose)
                    board[c, r].IsAnchor = true;
                else
                    board[r, c].IsAnchor = true;
            }
        }



        private void BuildDawg() {
            var db = new Database();
            foreach (var w in db.GetWords())
                dawg.Insert(w.ToUpper());
        }

        private void BuildBlankBoard() {
            var rng = new Random();
            for (var r = 0; r < MAX_ROWS; r++)
                for (var c = 0; c < MAX_COLS; c++) {
                    Board[r, c] = new Square(null, new Point(c, r));
                    if (Score.Bonuses.TryGetValue(new Point(c - 1, r - 1), out var m))
                        Board[r, c].Multiplier = m;
                    if (r == 0 || r == MAX_ROWS - 1 || c == 0 || c == MAX_COLS - 1) {
                        Board[r, c].IsBorderSquare = true;
                        Board[r, c].CrossChecks.Clear();
                    }
                }
        }

        public void BuildAnchorList(Square[,] board) {
            for (var r = MIN_ROW; r < MAX_ROW; r++)
                for (var c = MIN_COL; c < MAX_COL - 1; c++)
                    if (board[r, c].IsOccupied && !board[r, c + 1].IsOccupied) {
                        Anchors.Add(board[r, c + 1].AbsPosition);
                        c++;
                    }
                    else if (!board[r, c].IsOccupied && board[r, c + 1].IsOccupied) {
                        Anchors.Add(board[r, c].AbsPosition);
                    }
        }

        private void CalculateAllCrossChecks(Square[,] board) {
            // can't use the anchor collection because the keys will be wrong on a transposed board
            // TODO: package up that logic in a board class to make this loop more efficient 
            for (var r = MIN_ROW; r < MAX_ROW; r++)
                for (var c = MIN_COL; c < MAX_COL; c++)
                    if (board[r, c].IsAnchor)
                        CalculateCrossCheck(board, board[r, c]);
        }

        private void LeftPart(string partialWord, Lib.Node root, int limit, Rack rack, Square[,] board, Square anchor) {
            ExtendRight(partialWord, root, board, anchor, rack, anchor);
            if (limit > 0) {
                foreach (var e in root.Children) {
                    if (rack.HasLetter(e.Key)) {
                        rack.Remove(e.Key);
                        LeftPart(partialWord + e.Key, e.Value, limit - 1, rack, board, anchor);
                        rack.Add(e.Key);
                    }
                }
            }
        }



        private void LegalMove(string word, Square start, Square end, Rack rack, Square[,] board) {
            var tempBoard = (start.Position == start.AbsPosition && end.Position == end.AbsPosition) ?
                CopyBoard(Board) :
                Transpose(Board);

            var tiles = new List<Square>();
            var c = end.Position.X;
            var r = end.Position.Y;
            foreach (var l in word.Reverse()) {
                var s = new Square(board[r, c]); // dont mutate
                if (!s.IsOccupied)
                    s.Tile = l;

                tiles.Add(s);
                c--;
            }

            PlayableWords.Add(new WordModel
            {
                Score = Score.ScoreWord(tiles),
                Word = word,
                Start = new Point(start.AbsPosition.X - 1, start.AbsPosition.Y - 1), // remove border
                End = new Point(end.AbsPosition.X - 1, end.AbsPosition.Y - 1) // remove border
            });
            //RecordWord(word, tiles, endSquare, endSquare.AbsPosition.X == anchor.AbsPosition.X);
        }

        private void RecordWord(string word, List<Square> tiles, Square endSquare, bool vertical) {
            if (vertical)
                PlayableWords.Add(new WordModel
                {
                    Score = Score.ScoreWord(tiles),
                    Word = word,
                    Start = new Point(endSquare.AbsPosition.X - 1, (endSquare.AbsPosition.Y - word.Length) - 1),
                    End = new Point(endSquare.AbsPosition.X - 1, endSquare.AbsPosition.Y - 2)
                });
            else
                PlayableWords.Add(new WordModel
                {
                    Score = Score.ScoreWord(tiles),
                    Word = word,
                    Start = new Point((endSquare.AbsPosition.X - word.Length) - 1, endSquare.AbsPosition.Y - 1),
                    End = new Point(endSquare.AbsPosition.X - 2, endSquare.AbsPosition.Y - 1)
                });
        }

        // TODO: Anchor shouldnt need to be known if we know the board is transposed or not
        private void ExtendRight(string partialWord, Node root, Square[,] board, Square square, Rack rack, Square anchor) {
            if (!square.IsOccupied) {
                if (root.EndOfWord && square.Position != anchor.Position) {
                    // square is off one position to the right of the word, we calc start by the end of the word - the letter count
                    var end = square.Offset(0, -1);
                    // Hard to understand, if someone uses "start" thinking it's a real tile, they will be upset
                    var start = new Square(end);
                    start.AbsPosition = new Point(start.AbsPosition.X - partialWord.Length, start.AbsPosition.Y); 
                    LegalMove(partialWord, start, end, rack, board);
                }
                foreach (var e in root.Children) {
                    if (rack.HasLetter(e.Key) && square.CrossChecks.ContainsKey(e.Key)) {
                        rack.Remove(e.Key);
                        ExtendRight(partialWord + e.Key, e.Value, board, board[square.Position.Y, square.Position.X + 1], rack, anchor);
                        rack.Add(e.Key);
                    }
                }
            }
            else {
                var key = square.Tile.Value;
                if (root.Children.ContainsKey(square.Tile.Value))
                    ExtendRight(partialWord + key, root.Children[key], board, board[square.Position.Y, square.Position.X + 1], rack, anchor);
            }
        }

        private Node FastForwardPrefix(string prefix) {
            var root = dawg.Root;
            foreach (var c in prefix) {
                if (root.Children.TryGetValue(c, out var next))
                    root = next;
                else
                    return null;
            }

            return root;
        }

        private void ProcessRow(Square[,] board, int r) {
            var limit = 0;
            var prefix = "";
            for (var c = MIN_COL; c < MAX_COL; c++) {
                if (board[r, c].IsAnchor) {
                    if (prefix.Length > 0) {
                        var root = FastForwardPrefix(prefix);
                        if (root != null)
                            ExtendRight(prefix, root, board, board[r, c], new Rack(Rack), board[r, c]);
                    }
                    else
                        LeftPart("", dawg.Root, limit, new Rack(Rack), board, board[r, c]);

                    limit = 0;
                    prefix = "";
                }
                else if (!board[r, c].IsOccupied) {
                    limit++;
                    prefix = "";
                }
                else if (board[r, c].IsOccupied)
                    prefix += board[r, c].Tile;
            }
        }

        private void CalculateCrossCheck(Square[,] board, Square currentSquare) {
            var downParts = GetDownWord(board, currentSquare);
            var crossChecks = new Dictionary<char, string>();

            if (downParts.Prefix.Length == 0 && downParts.Suffix.Length == 0)
                return;

            foreach (var c in ALPHABET)
                if (dawg.Exists(downParts.Prefix + c + downParts.Suffix))
                    crossChecks.Add(c, downParts.Prefix + c + downParts.Suffix);

            currentSquare.CrossChecks = crossChecks;
        }

        private string GetDownWord(Square[,] board, Square currentSquare, int step) {
            var sb = new StringBuilder();
            var r = currentSquare.Position.Y + step;
            var c = currentSquare.Position.X;

            while (r < MAX_ROW && r >= MIN_ROW && board[r, c].IsOccupied) {
                sb.Append(board[r, c].Tile);
                r += step;
            }

            if (sb.Length == 0) return "";

            return step < 0 ? new string(sb.ToString().Reverse().ToArray()) : sb.ToString();
        }

        private (string Prefix, string Suffix) GetDownWord(Square[,] board, Square currentSquare) =>
            (GetDownWord(board, currentSquare, -1), GetDownWord(board, currentSquare, 1));



        private char GetTileLabel(Square s) {
            if (s.IsBorderSquare) return '@';
            if (s.IsOccupied) return s.Tile.Value;
            if (s.IsAnchor) return '*';
            return '.';
        }

        public void PrintBoard(Square[,] board) {
            for (var r = 0; r < MAX_ROWS; r++) {
                if (r > 0) Console.WriteLine("");
                for (var c = 0; c < MAX_COLS; c++) {
                    var square = board[r, c];
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = square.Color;
                    Console.Write($"{GetTileLabel(square)} ");
                    Console.ForegroundColor = originalColor;
                }
            }

            Console.WriteLine("");
            Console.WriteLine($"Anchors: {Anchors.Count}");
        }

        public List<SquareModel> SerializeBoard(Square[,] board) {
            var result = new List<SquareModel>();

            for (var r = 0; r < MAX_ROWS; r++)
                for (var c = 0; c < MAX_COLS; c++) {
                    var square = board[r, c];
                    if (square.Tile.HasValue || square.IsAnchor)
                        result.Add(new SquareModel { Tile = square.Tile, IsAnchor = square.IsAnchor, Position = new Point(square.Position.X - 1, square.Position.Y - 1) });
                }

            return result;
        }

        public Square[,] CopyBoard(Square[,] board) {
            var newBoard = new Square[MAX_ROWS, MAX_COLS];
            for (var r = 0; r < MAX_ROWS; r++)
                for (var c = 0; c < MAX_COLS; c++)
                    newBoard[r, c] = new Square(board[r, c]);
            return newBoard;
        }



        public Square[,] Transpose(Square[,] board) {
            var newBoard = new Square[MAX_ROWS, MAX_COLS];
            var boardCopy = CopyBoard(board);
            for (var r = 0; r < MAX_ROWS; r++)
                for (var c = 0; c < MAX_COLS; c++) {
                    newBoard[c, r] = boardCopy[r, c];
                    newBoard[c, r].Position.X = r;
                    newBoard[c, r].Position.Y = c;
                }
            return newBoard;
        }

    }

    public static class Score
    {
        public enum MultiplierType
        {
            Letter,
            Word
        }

        public class Multiplier
        {
            public Multiplier(Multiplier m) : this(m.Value, m.Type) { }
            public Multiplier(short value, MultiplierType type) {
                Type = type;
                Value = value;
            }
            public MultiplierType Type { get; }
            public short Value { get; }
        }
        public static readonly Dictionary<char, short> Letters = new Dictionary<char, short> {
           { 'A', 1 }, { 'B', 4 }, { 'C', 4 }, { 'D', 2 }, { 'E', 1 },
           { 'F', 4 }, { 'G', 3 }, { 'H', 3 }, { 'I', 1 }, { 'J', 10 },
           { 'K', 5 }, { 'L', 2 }, { 'M', 4 }, { 'N', 2 }, { 'O', 1 },
           { 'P', 4 }, { 'Q', 10 }, { 'R', 1 }, { 'S', 1 }, { 'T', 1 },
           { 'U', 2 }, { 'V', 5 }, { 'W', 4 }, { 'X', 8 }, { 'Y', 3 },
           { 'Z', 10 },
        };
        public static readonly Dictionary<Point, Multiplier> Bonuses = new Dictionary<Point, Multiplier> {
            { new Point(3,0), new Multiplier(3, MultiplierType.Word)}, { new Point(6,0), new Multiplier(3, MultiplierType.Letter)},
            { new Point(8,0), new Multiplier(3, MultiplierType.Letter)}, { new Point(11,0), new Multiplier(3, MultiplierType.Word)},
            { new Point(2,1), new Multiplier(2, MultiplierType.Letter)}, { new Point(5,1), new Multiplier(2, MultiplierType.Word)},
            { new Point(9,1), new Multiplier(2, MultiplierType.Word)}, { new Point(12,1), new Multiplier(2, MultiplierType.Letter)},
            { new Point(1,2), new Multiplier(2, MultiplierType.Letter)}, { new Point(4,2), new Multiplier(2, MultiplierType.Letter)},
            { new Point(10,2), new Multiplier(2, MultiplierType.Letter)}, { new Point(13,2), new Multiplier(2, MultiplierType.Letter)},
            { new Point(0,3), new Multiplier(3, MultiplierType.Word)}, { new Point(3,3), new Multiplier(3, MultiplierType.Letter)}, { new Point(7,3), new Multiplier(2, MultiplierType.Word)},
            { new Point(11,3), new Multiplier(3, MultiplierType.Letter)}, { new Point(14,3), new Multiplier(3, MultiplierType.Word)},
            { new Point(2,4), new Multiplier(2, MultiplierType.Letter)}, { new Point(6,4), new Multiplier(2, MultiplierType.Letter)},
            { new Point(8,4), new Multiplier(2, MultiplierType.Letter)}, { new Point(12,4), new Multiplier(2, MultiplierType.Letter)},
            { new Point(1,5), new Multiplier(2, MultiplierType.Word)}, { new Point(5,5), new Multiplier(3, MultiplierType.Letter)},
            { new Point(9,5), new Multiplier(3, MultiplierType.Letter)}, { new Point(13,5), new Multiplier(2, MultiplierType.Word)},
            { new Point(0,6), new Multiplier(3, MultiplierType.Letter)}, { new Point(4,6), new Multiplier(2, MultiplierType.Letter)},
            { new Point(10,6), new Multiplier(2, MultiplierType.Letter)}, { new Point(14,6), new Multiplier(3, MultiplierType.Letter)},
            { new Point(3,7), new Multiplier(2, MultiplierType.Word)}, { new Point(11,7), new Multiplier(2, MultiplierType.Word)},

            { new Point(3,14), new Multiplier(3, MultiplierType.Word)}, { new Point(6,14), new Multiplier(3, MultiplierType.Letter)},
            { new Point(8,14), new Multiplier(3, MultiplierType.Letter)}, { new Point(11,14), new Multiplier(3, MultiplierType.Word)},
            { new Point(2,13), new Multiplier(2, MultiplierType.Letter)}, { new Point(5,13), new Multiplier(2, MultiplierType.Word)},
            { new Point(9,13), new Multiplier(2, MultiplierType.Word)}, { new Point(12,13), new Multiplier(2, MultiplierType.Letter)},
            { new Point(1,12), new Multiplier(2, MultiplierType.Letter)}, { new Point(4,12), new Multiplier(2, MultiplierType.Letter)},
            { new Point(10,12), new Multiplier(2, MultiplierType.Letter)}, { new Point(13,12), new Multiplier(2, MultiplierType.Letter)},
            { new Point(0,11), new Multiplier(3, MultiplierType.Word)}, { new Point(3,11), new Multiplier(3, MultiplierType.Letter)}, { new Point(7,11), new Multiplier(2, MultiplierType.Word)},
            { new Point(11,11), new Multiplier(3, MultiplierType.Letter)}, { new Point(14,11), new Multiplier(3, MultiplierType.Word)},
            { new Point(2,10), new Multiplier(2, MultiplierType.Letter)}, { new Point(6,10), new Multiplier(2, MultiplierType.Letter)},
            { new Point(8,10), new Multiplier(2, MultiplierType.Letter)}, { new Point(12,10), new Multiplier(2, MultiplierType.Letter)},
            { new Point(1,9), new Multiplier(2, MultiplierType.Word)}, { new Point(5,9), new Multiplier(3, MultiplierType.Letter)},
            { new Point(9,9), new Multiplier(3, MultiplierType.Letter)}, { new Point(13,9), new Multiplier(2, MultiplierType.Word)},
            { new Point(0,8), new Multiplier(3, MultiplierType.Letter)}, { new Point(4,8), new Multiplier(2, MultiplierType.Letter)},
            { new Point(10,8), new Multiplier(2, MultiplierType.Letter)}, { new Point(14,8), new Multiplier(3, MultiplierType.Letter)},
        };


        public static int ScoreWord(List<Square> squares) { // TODO: Obviously need to include multipliers
            var total = 0;
            var downWordTotal = 0;
            var tripleWords = 0;
            var doubleWords = 0;
            foreach (var s in squares) {
                if (s.Multiplier != null && s.Multiplier.Type == MultiplierType.Word && s.Multiplier.Value == 2)
                    doubleWords++;
                else if (s.Multiplier != null && s.Multiplier.Type == MultiplierType.Word && s.Multiplier.Value == 3)
                    tripleWords++;

                total += applyLetterMultiplier(Letters[s.Tile.Value], s.Multiplier);

                if (s.CrossChecks.TryGetValue(s.Tile.Value, out var downWord))
                    downWordTotal += ScoreDownWord(downWord, s.Multiplier);
            }

            var multipliers = (total * tripleWords * 3) + (total * doubleWords * 2);
            return total = multipliers == 0 ? total + downWordTotal : multipliers + downWordTotal;

            int applyLetterMultiplier(int letterValue, Multiplier multiplier) {
                if (multiplier == null) return letterValue;

                return multiplier.Type switch
                {
                    MultiplierType.Letter => letterValue * multiplier.Value,
                    _ => letterValue
                };

            }
        }

        private static int ScoreDownWord(string word, Multiplier multiplier) {
            var total = 0;
            foreach (var c in word)
                total += Letters[c];

            // down words can only be affected by one multiplier and it has to be a word multiplier
            if (multiplier != null && multiplier.Type == MultiplierType.Word)
                total *= multiplier.Value;

            return total;
        }
    }

    public class WordModel
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public string Word { get; set; }
        public int Score { get; set; }
    }

    public class SquareModel
    {
        public Point Position { get; set; }
        public char? Tile { get; set; }
        public bool IsAnchor { get; set; }
    }

    public class Square
    {
        public Square(Square s) { // copy constructor
            CrossChecks = new Dictionary<char, string>(s.CrossChecks);
            Tile = s.Tile;
            Position = s.Position;
            AbsPosition = s.AbsPosition;
            Color = s.Color;
            IsBorderSquare = s.IsBorderSquare;
            IsAnchor = s.IsAnchor;

            if (s.Multiplier != null)
                Multiplier = new Score.Multiplier(s.Multiplier);
        }
        public Square(char? tile, Point position) {
            CrossChecks = new Dictionary<char, string>(Scrabble.ALPHABET.Select(x => new KeyValuePair<char, string>(x, "")));
            Tile = tile;
            Position = position;
            AbsPosition = position; // should never change
        }

        public Square Offset(int row, int col) {
            var s = new Square(this);
            s.Position.Offset(col, row);
            //s.AbsPosition.Offset(col, row); // bad?
            return s;
        }

        public bool IsAnchor { get; set; } = false;
        public ConsoleColor Color = ConsoleColor.White;
        public Dictionary<char, string> CrossChecks;
        public char? Tile;
        public Point Position;
        public Point AbsPosition; // make immutable
        public bool IsBorderSquare = false;
        public bool IsOccupied => Tile.HasValue;
        public Score.Multiplier Multiplier { get; set; }
        //public bool HasNonTrivalCrossCheck => IsBorderSquare || CrossChecks.Count < 26;
    }

    public class Rack
    {
        private readonly Dictionary<char, short> _letters = new Dictionary<char, short>();

        public Rack(string letters) {
            foreach (char l in letters)
                Add(l);
        }

        public void Remove(char letter) {
            if (_letters.TryGetValue(letter, out var c) && c > 0)
                _letters[letter]--;
        }

        public void Add(char letter) {
            if (_letters.ContainsKey(letter))
                _letters[letter]++;
            else
                _letters.Add(letter, 1);
        }

        public IEnumerable<char> Letters
        {
            get
            {
                var list = new List<char>();
                foreach (var x in _letters)
                    for (var n = 0; n < x.Value; n++)
                        list.Add(x.Key);
                return list;
            }
        }

        public bool HasLetter(char letter) => _letters.ContainsKey(letter) && _letters[letter] > 0;
        public int Count => _letters.Sum(x => x.Value);
    }
}
