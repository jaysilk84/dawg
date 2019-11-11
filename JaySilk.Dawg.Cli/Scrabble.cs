using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using JaySilk.Dawg.Lib;

namespace JaySilk.Dawg.Cli
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
        //public Dictionary<Point, Square> Anchors = new Dictionary<Point, Square>();
        public HashSet<Point> Anchors = new HashSet<Point>();
        public string Rack = "SVGFKTO";
        public Scrabble() {
            BuildDawg();

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

            Board[6,3].Tile = 'T';
            Board[6,4].Tile = 'U';
            Board[6,5].Tile = 'B';
            
            Board[7,4].Tile = 'S';
            Board[7,5].Tile = 'A';
            Board[7,6].Tile = 'I';
            Board[7,7].Tile = 'N';
            Board[7,8].Tile = 'T';

            Board[6,8].Tile = 'E';
            Board[6,9].Tile = 'N';
            Board[6,10].Tile = 'A';
            Board[6,11].Tile = 'M';
            Board[6,12].Tile = 'E';
            Board[6,13].Tile = 'L';

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

            Console.WriteLine("Found " + wordCount + " words");
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
                    if (r == 0 || r == MAX_ROWS - 1 || c == 0 || c == MAX_COLS - 1) {
                        Board[r, c].IsBorderSquare = true;
                        Board[r, c].CrossChecks.Clear();
                    }
                }
        }

        public void BuildAnchorList(Square[,] board) {
            var total = 0;
            for (var r = MIN_ROW; r < MAX_ROW; r++)
                for (var c = MIN_COL; c < MAX_COL - 1; c++)
                    if (board[r, c].IsOccupied && !board[r, c + 1].IsOccupied) {
                        Anchors.Add(board[r, c + 1].AbsPosition);
                        //Anchors[board[r, c + 1].AbsPosition] = board[r, c + 1];
                        //board[r,c+1].IsAnchor = true;
                        c++;
                        total++;
                    }
                    else if (!board[r, c].IsOccupied && board[r, c + 1].IsOccupied) {
                        Anchors.Add(board[r, c].AbsPosition);
                        //Anchors[board[r, c].AbsPosition] = board[r, c];
                        //board[r, c].IsAnchor = true;
                        total++;
                    } else {
                        //board[r,c].IsAnchor = false;
                    }
            Console.WriteLine("Anchors: " + total);
        }

        private void CalculateAllCrossChecks(Square[,] board) {
            // foreach (var a in Anchors)
            //     CalculateCrossCheck(board, board[a.Value.Position.Y, a.Value.Position.X]);
            for (var r = MIN_ROW; r < MAX_ROW; r++)
                for (var c = MIN_COL; c < MAX_COL; c++)
                    if (board[r, c].IsAnchor) 
                        CalculateCrossCheck(board, board[r,c]);
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
        private static int wordCount = 0;
        private void LegalMove(string word, Square anchor, Square endSquare, Rack rack) {
            Square[,] tempBoard = CopyBoard(Board);
            //Square[,] tempBoard = Transpose(Board);
            // NOTE: endsquare is one position too far
            //if (endSquare.)
            if (anchor.AbsPosition.Y == endSquare.AbsPosition.Y) {
                // horizontal play
                var c = endSquare.AbsPosition.X - 1;
                var r = endSquare.AbsPosition.Y;
                foreach (var l in word.Reverse()) {
                    var s = tempBoard[r, c];
                    if (!s.IsOccupied) {
                        s.Tile = l;
                        s.Color = ConsoleColor.Red;
                    }
                    c--;
                }
            }
            else {
                // vertical play
                var c = endSquare.AbsPosition.X;
                var r = endSquare.AbsPosition.Y - 1;
                foreach (var l in word.Reverse()) {
                    var s = tempBoard[r, c];
                    if (!s.IsOccupied) {
                        s.Tile = l;
                        s.Color = ConsoleColor.Red;
                    }
                    r--;
                }
            }
            wordCount++;
            Console.WriteLine($"Word: {word} End Row: {endSquare.Position.Y} End Col: {endSquare.Position.X} Anchor Row: {anchor.Position.Y} Anchor Col: {anchor.Position.X}");
            PrintBoard(tempBoard);
            Console.WriteLine($"Rack letters: {new string(rack.Letters.ToArray())}");
            //Console.WriteLine(word);
        }

        private bool ValidCrossChecks(Square s, char letter) {
            return s.CrossChecks.Contains(letter);
            //return (s.HasNonTrivalCrossCheck && s.CrossChecks.Contains(letter)) || !s.HasNonTrivalCrossCheck;
        }

        // TODO: Anchor shouldnt need to be known if we know the board is transposed or not
        private void ExtendRight(string partialWord, Node root, Square[,] board, Square square, Rack rack, Square anchor) {
            if (!square.IsOccupied) {
                if (root.EndOfWord && square.Position != anchor.Position)
                    LegalMove(partialWord, anchor, square, rack);
                foreach (var e in root.Children) {
                    if (rack.HasLetter(e.Key) && ValidCrossChecks(square, e.Key)) {
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
                //if (Anchors.ContainsKey(board[r, c].AbsPosition)) {
                if (board[r,c].IsAnchor) {
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
            var crossChecks = new HashSet<char>();

            if (downParts.Prefix.Length == 0 && downParts.Suffix.Length == 0)
                return;

            //Console.WriteLine(downParts.Prefix + "?" + downParts.Suffix);
            //Console.WriteLine("anchor: " + currentSquare.Position);
            foreach (var c in ALPHABET)
                if (dawg.Exists(downParts.Prefix + c + downParts.Suffix))
                    crossChecks.Add(c);

            currentSquare.CrossChecks = crossChecks;
        }

        private string GetDownWord(Square[,] board, Square currentSquare, int step) {
            var sb = new StringBuilder();
            //var s = board[currentSquare.]
            var r = currentSquare.Position.Y + step;
            var c = currentSquare.Position.X;
            while (r < MAX_ROW && r >= MIN_ROW && board[r, c].IsOccupied) {
                //Console.WriteLine($"R: {r} C: {c} t: {board[r,c].Tile}");
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
            //if (Anchors.TryGetValue(s.AbsPosition, out var value)) return '*';
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

    public class Square
    {
        public Square(Square s) { // copy constructor
            CrossChecks = new HashSet<char>(s.CrossChecks);
            Tile = s.Tile;
            Position = s.Position;
            AbsPosition = s.AbsPosition;
            Color = s.Color;
            IsBorderSquare = s.IsBorderSquare;
            IsAnchor = s.IsAnchor;
        }
        public Square(char? tile, Point position) {
            CrossChecks = new HashSet<char>(Scrabble.ALPHABET);
            Tile = tile;
            Position = position;
            AbsPosition = position; // should never change
        }

        public bool IsAnchor { get; set; } = false;
        public ConsoleColor Color = ConsoleColor.White;
        public HashSet<char> CrossChecks;
        public char? Tile;
        public Point Position;
        public Point AbsPosition; // make immutable
        public bool IsBorderSquare = false;
        public bool IsOccupied => Tile.HasValue;
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