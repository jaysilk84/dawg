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
        public Square[,] Board = new Square[MAX_ROWS, MAX_COLS];
        public HashSet<Point> Anchors = new HashSet<Point>();
        public List<WordModel> PlayableWords = new List<WordModel>();
        public string Rack = "HACKERS";

        public Scrabble(Lib.Dawg wordList = null) {
            if (wordList == null)
                BuildDawg();
            else
                dawg = wordList;

            BuildBlankBoard();
            Board[1, 4].Tile = 'Z';
            Board[2, 4].Tile = 'A';
            Board[2, 5].Tile = 'S';


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

            // Board[7, 3].Tile = 'T';
            // Board[7, 4].Tile = 'U';
            // Board[7, 5].Tile = 'B';

            // Board[8, 4].Tile = 'S';
            // Board[8, 5].Tile = 'A';
            // Board[8, 6].Tile = 'I';
            // Board[8, 7].Tile = 'N';
            // Board[8, 8].Tile = 'T';

            // Board[7, 8].Tile = 'E';
            // Board[7, 9].Tile = 'N';
            // Board[7, 10].Tile = 'A';
            // Board[7, 11].Tile = 'M';
            // Board[7, 12].Tile = 'E';
            // Board[7, 13].Tile = 'L';

            // Board[4, 14].Tile = 'F';
            // Board[5, 14].Tile = 'O';
            // Board[6, 14].Tile = 'G';
            // Board[7, 14].Tile = 'S';

            // Board[3, 15].Tile = 'M';
            // Board[4, 15].Tile = 'I';
            // Board[5, 15].Tile = 'X';

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

        private void LeftPart(Word partialWord, Lib.Node root, int limit, Rack rack, Square[,] board, Square anchor) {
            ExtendRight(partialWord, root, board, anchor, rack, anchor);
            if (limit > 0) {
                foreach (var e in root.Children) {
                    if (rack.HasLetter(e.Key)) {
                        rack.Remove(e.Key);
                        LeftPart(partialWord.Append(e.Key, false), e.Value, limit - 1, rack, board, anchor);
                        rack.Add(e.Key);
                    }
                    else if (rack.HasBlank) {
                        rack.Remove('?');
                        //rack.Add((char)('?' + e.Key));
                        LeftPart(partialWord.Append(e.Key, true), e.Value, limit - 1, rack, board, anchor);
                        //rack.Remove((char)('?' + e.Key));
                        rack.Add('?');
                    }
                }
            }
        }

        private void LegalMove(Word word, Square start, Square end, Square[,] board, Rack rack) {
            var tiles = new List<Square>();
            var tempBoard = CopyBoard(board);
            var c = start.Position.X;
            var r = start.Position.Y;
            var i = 0;
            foreach (var l in word.ToString()) {
                var s = tempBoard[r, c];

                if (!s.IsOccupied) {
                    s.HasBlank = word.HasBlank(i);
                    s.Tile = l;
                }

                s.IsPlayed = true;
                tiles.Add(s);
                c++;
                i++;
            }

            var boardModel = new BoardModel()
            {
                Tiles = SerializeBoard(board).ToArray(), // issue: transposed board?
                Rack = rack.ToString(),
                PlayedWord = new WordModel
                {
                    Score = Score.ScoreWord(tiles, rack),
                    Word = word.ToString(),
                    End = new Point(end.AbsPosition.X - 1, end.AbsPosition.Y - 1) // remove border
                }
            };

            PlayableWords.Add(new WordModel
            {
                Score = Score.ScoreWord(tiles, rack),
                Word = word.ToString(),
                //Blanks = word.Blanks,
                Start = new Point(start.AbsPosition.X - 1, start.AbsPosition.Y - 1), // remove border
                End = new Point(end.AbsPosition.X - 1, end.AbsPosition.Y - 1) // remove border
            });
        }

        // TODO: Anchor shouldnt need to be known if we know the board is transposed or not. Find another way to 
        // figure out if we placed a tile?
        private void ExtendRight(Word partialWord, Node root, Square[,] board, Square square, Rack rack, Square anchor) {
            if (!square.IsOccupied) {
                if (root.EndOfWord && square.Position != anchor.Position)
                    // square is off one position to the right of the word, we calc start by the end of the word minus the letter count
                    // start is offset by 1 because it should be partialWord.Length - 1, leaving it just .Length accomplishes the same
                    LegalMove(partialWord, square.Offset(0, -(partialWord.Length)), square.Offset(0, -1), board, rack);

                foreach (var e in root.Children) {
                    if (rack.HasLetter(e.Key) && square.CrossChecks.ContainsKey(e.Key)) {
                        rack.Remove(e.Key);
                        ExtendRight(partialWord.Append(e.Key, false), e.Value, board, board[square.Position.Y, square.Position.X + 1], rack, anchor);
                        rack.Add(e.Key);
                    }
                    else if (rack.HasBlank && square.CrossChecks.ContainsKey(e.Key)) {
                        rack.Remove('?');
                        ExtendRight(partialWord.Append(e.Key, true), e.Value, board, board[square.Position.Y, square.Position.X + 1], rack, anchor);
                        rack.Add('?');
                    }
                }
            }
            else {
                var key = square.Tile.Value;
                if (root.Children.ContainsKey(square.Tile.Value))
                    ExtendRight(partialWord.Append(key, square.HasBlank), root.Children[key], board, board[square.Position.Y, square.Position.X + 1], rack, anchor);
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
                            ExtendRight(new Word(prefix), root, board, board[r, c], new Rack(Rack), board[r, c]);
                    }
                    else
                        LeftPart(new Word(""), dawg.Root, limit, new Rack(Rack), board, board[r, c]);

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
            var downWord = GetDownWord(board, currentSquare);
            var crossChecks = new Dictionary<char, Word>();

            // this is weird, GetDownWord should always be 1 if there is no downword (just the anchor square)
            // TODO: Revisit to make it more intuitive
            if (downWord.Length == 1)
                return;

            foreach (var c in ALPHABET) {
                var word = downWord.ToString().Replace('?', c);
                if (dawg.Exists(word))
                    crossChecks.Add(c, new Word(word, downWord.Blanks));
            }
            currentSquare.CrossChecks = crossChecks;
        }

        private Word GetDownWord(Square[,] board, Square currentSquare) {
            var word = new Word("");

            // get upper bound of the potential downword
            var r = currentSquare.Position.Y - 1;
            var c = currentSquare.Position.X;
            while (r < MAX_ROW && r >= MIN_ROW && board[r, c].IsOccupied) r--;

            for (r = r + 1; r < MAX_ROW; r++) {
                var s = board[r, c];
                if (currentSquare.Position.Y == r)
                    word = word.Append('?', s.HasBlank);
                else if (s.IsOccupied)
                    word = word.Append(s.Tile.Value, s.HasBlank);
                else
                    break;
            }

            return word;
        }

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
                        result.Add(new SquareModel
                        {
                            Tile = square.Tile,
                            IsAnchor = square.IsAnchor,
                            Position = new Point(square.AbsPosition.X - 1, square.AbsPosition.Y - 1),
                            IsBlank = square.HasBlank,
                            Value = square.Value
                        });
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

            for (var r = 0; r < MAX_ROWS; r++)
                for (var c = 0; c < MAX_COLS; c++)
                    newBoard[c, r] = board[r, c].Transpose();

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


        public static int ScoreWord(List<Square> squares, Rack rack) {
            var total = 0;
            var downWordTotal = 0;
            var tripleWords = 0;
            var doubleWords = 0;
            foreach (var s in squares) {
                if (s.Multiplier != null && s.Multiplier.Type == MultiplierType.Word && s.Multiplier.Value == 2)
                    doubleWords++;
                else if (s.Multiplier != null && s.Multiplier.Type == MultiplierType.Word && s.Multiplier.Value == 3)
                    tripleWords++;
                var value = s.HasBlank ? 0 : Letters[s.Tile.Value];

                total += ApplyLetterMultiplier(value, s.Multiplier);

                if (s.CrossChecks.TryGetValue(s.Tile.Value, out var downWord))
                    downWordTotal += ScoreDownWord(downWord, s);
            }

            var multipliers = (total * tripleWords * 3) + (total * doubleWords * 2);
            var bonus = rack.Count == 0 ? 50 : 0;
            return total = (multipliers == 0 ? total + downWordTotal : multipliers + downWordTotal) + bonus;
        }

        private static int ApplyLetterMultiplier(int value, Multiplier multiplier) {
            if (multiplier == null) return value;

            return multiplier.Type switch
            {
                MultiplierType.Letter => value * multiplier.Value,
                _ => value
            };
        }

        private static int ScoreDownWord(Word word, Square square) {
            var total = 0;
            var multiplier = square.Multiplier;
            var i = 0;
            foreach (var c in word.ToString()) {
                // if the letter is the square, apply its multiplier if any, otherwise just use the letter value
                var value = word.HasBlank(i) ? 0 : Letters[c];
                total += c == square.Tile.Value ? ApplyLetterMultiplier(value, multiplier) : value;
                i++;
            }

            // apply a word multiplier if one exists on the placed tile creating the down word
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
        //public HashSet<int> Blanks { get; set; }
    }

    public class BoardModel
    {
        public SquareModel[] Tiles { get; set; }
        public string Rack { get; set; }
        public WordModel PlayedWord { get; set; }
    }

    public class SquareModel
    {
        public Point Position { get; set; }
        public char? Tile { get; set; }
        public bool IsAnchor { get; set; }
        public bool IsBlank { get; set; }
        public int Value { get; set; }
        public bool IsPlayed { get; set; }
    }

    /// <summary>
    /// Need to be immutable, also the word needs to be represented with tiles
    /// that contain the information like the value of the tile.
    /// </summary>
    public class Word
    {
        private StringBuilder _word;
        private HashSet<int> _blanks;

        private Word(Word w) : this(w._word, new HashSet<int>(w._blanks)) {
        }

        private Word(StringBuilder word, HashSet<int> blanks) {
            _word = word;
            _blanks = blanks;
        }

        public Word(string word, HashSet<int> blanks) : this(new StringBuilder(word), new HashSet<int>(blanks)) {
        }

        public Word(string word) : this(new StringBuilder(word), new HashSet<int>()) {
        }

        public Word Replace(char oldChar, char newChar) => new Word(_word.Replace(oldChar, newChar), _blanks);
        public HashSet<int> Blanks => new HashSet<int>(_blanks);

        public bool HasBlank(int pos) {
            return _blanks.Contains(pos);
        }

        public int Length => _word.Length;

        public Word Append(char letter, bool isBlank) {
            var h = new HashSet<int>(_blanks);
            if (isBlank)
                h.Add(_word.Length);
            var word = _word.ToString();
            word += letter;
            return new Word(new StringBuilder(word), h);
        }

        public override string ToString() => _word.ToString();
    }

    public class Square
    {
        public Square(Square s) { // copy constructor
            CrossChecks = new Dictionary<char, Word>(s.CrossChecks);
            Tile = s.Tile;
            Position = s.Position;
            //AbsPosition = s.AbsPosition;
            Color = s.Color;
            IsBorderSquare = s.IsBorderSquare;
            IsAnchor = s.IsAnchor;
            IsTransposed = s.IsTransposed;
            HasBlank = s.HasBlank;

            if (s.Multiplier != null)
                Multiplier = new Score.Multiplier(s.Multiplier);
        }

        private Square(Square s, Point p, bool isTransposed) : this(s) {
            Position = p;
            IsTransposed = isTransposed;
        }

        public Square(Square s, int offsetX, int offsetY) : this(s) {
            Position.Offset(offsetX, offsetY);
        }

        public Square(char? tile, Point position) {
            CrossChecks = new Dictionary<char, Word>(Scrabble.ALPHABET.Select(x => new KeyValuePair<char, Word>(x, new Word(""))));
            Tile = tile;
            Position = position;
            //AbsPosition = position; // should never change
        }

        public Square Offset(int row, int col) => new Square(this, col, row);

        public bool HasBlank = false;
        public bool IsAnchor { get; set; } = false;
        public ConsoleColor Color = ConsoleColor.White;
        public Dictionary<char, Word> CrossChecks;
        public char? Tile;
        public Point Position;
        public Point AbsPosition => this.IsTransposed ? new Point(this.Position.Y, this.Position.X) : this.Position;
        public bool IsBorderSquare = false;
        public bool IsOccupied => Tile.HasValue;
        public Score.Multiplier Multiplier { get; set; }
        private bool IsTransposed { get; set; } = false;
        public bool IsPlayed { get; set; } = false;
        public int Value => HasBlank ? 0 : IsOccupied ? Score.Letters[Tile.Value] : 0;
        public Square Transpose() {
            return new Square(this, new Point(this.Position.Y, this.Position.X), !this.IsTransposed);
        }
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

        public void Delete(char letter) => _letters.Remove(letter);
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

        public bool HasBlank => _letters.ContainsKey('?') && _letters['?'] > 0;

        public bool HasLetter(char letter) => _letters.ContainsKey(letter) && _letters[letter] > 0;
        public int Count => _letters.Sum(x => x.Value);
    }
}
