using System;
using System.Globalization;
using System.Text;
using Cecer.Utils;

namespace Cecer.ConsoleSplit
{
    public class InLogicalConsole : LogicalConsole
    {
        private readonly StringBuilder _inputLine;

        private int _inputCursor;

        private int _historyIndex = -1;
        private readonly RollingCollection<string> _inputHistory;

        private int _maxInputLength;

        public int MaxInputLength
        {
            get
            {
                return _maxInputLength;
            }
            set
            {
                _maxInputLength = Math.Min(value, (Width * Height) - 1);
                if (_inputLine.Length > _maxInputLength)
                    _inputLine.Length = _maxInputLength;
                if (_inputCursor > _maxInputLength)
                    _inputCursor = _maxInputLength;
            }
        }

        public InLogicalConsole(int width, int height, int left, int top, ConsoleColor initalBackground = ConsoleColor.Black, ConsoleColor initalForeground = ConsoleColor.Gray) : base(width, height, left, top, initalBackground, initalForeground)
        {
            _inputLine = new StringBuilder();

            MaxInputLength = int.MaxValue;

            _inputCursor = 0;
            _inputHistory = new RollingCollection<string>(100);
        }


        public override void Draw()
        {
            Draw(-1);
        }
        private void Draw(int historyIndex)
        {
            if (Hidden)
                return;

            string inputText = (historyIndex == -1 ? _inputLine.ToString() : _inputHistory[historyIndex]).PadRight(MaxInputLength);

            lock (Container)
            {
                Console.ForegroundColor = ForegroundColor;
                Console.BackgroundColor = BackgroundColor;

                int overflowCount = 0;
                while (inputText.Length >= Width)
                {
                    Console.SetCursorPosition(Left, Top + overflowCount);
                    Console.Write(inputText.Substring(0, Width));
                    inputText = inputText.Substring(Width);

                    overflowCount++;
                }

                Console.SetCursorPosition(Left, Top + overflowCount);
                Console.Write(inputText);

                PositionCursor();
            }
        }

        public string ReadLine()
        {
            if (Hidden)
                return null;

            PositionCursor();
            ConsoleKeyInfo keyInfo = Container.ReadKey(true);

            lock (this)
            {
                _historyIndex = -1;

                while (keyInfo.KeyChar != (char)10 && keyInfo.KeyChar != (char)13 && keyInfo.KeyChar.ToString(CultureInfo.InvariantCulture) != Environment.NewLine)
                {
                    lock (Container)
                    {
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.Delete:
                                {
                                    if (_inputCursor < _inputLine.Length)
                                    {
                                        if (_historyIndex != -1)
                                        {
                                            _inputLine.Clear().Append(_inputHistory[_historyIndex]);
                                            _historyIndex = -1;
                                        }
                                        _inputLine.Remove(_inputCursor, 1);
                                    }
                                    break;
                                }
                            case ConsoleKey.Backspace:
                                {
                                    if (_inputCursor > 0)
                                    {
                                        if (_historyIndex != -1)
                                        {
                                            _inputLine.Clear().Append(_inputHistory[_historyIndex]);
                                            _historyIndex = -1;
                                        }
                                        _inputLine.Remove(_inputCursor - 1, 1);
                                        _inputCursor--;
                                    }
                                    break;
                                }
                            case ConsoleKey.LeftArrow:
                                {
                                    if (_inputCursor > 0)
                                        _inputCursor--;
                                    break;
                                }
                            case ConsoleKey.RightArrow:
                                {
                                    if (_inputCursor < _inputLine.Length)
                                    {
                                        _inputCursor++;
                                        _inputCursor = _inputLine.Length;

                                    }
                                    break;
                                }
                            case ConsoleKey.UpArrow:
                                {
                                    if (_historyIndex < _inputHistory.CurrentSize - 1)
                                    {
                                        _historyIndex++;

                                        _inputCursor = _inputHistory[_historyIndex].Length;
                                    }
                                    break;
                                }
                            case ConsoleKey.DownArrow:
                                {
                                    if (_historyIndex > -1)
                                    {
                                        _historyIndex--;

                                        if (_historyIndex == -1)
                                            _inputCursor = _inputLine.ToString().Length;
                                        else
                                            _inputCursor = _inputHistory[_historyIndex].Length;
                                    }
                                    break;
                                }
                            case ConsoleKey.Home:
                                {
                                    _inputCursor = 0;
                                    break;
                                }
                            case ConsoleKey.End:
                                {
                                    _inputCursor = _inputLine.Length;
                                    break;
                                }
                            default:
                            {
                                WriteChar(keyInfo.KeyChar);
                                break;
                            }
                        }
                    }
                    Draw(_historyIndex);
                    PositionCursor();
                    keyInfo = Container.ReadKey(true);
                }
                if (_historyIndex != -1)
                    _inputLine.Clear().Append(_inputHistory[_historyIndex]);

                string finalInput = _inputLine.ToString();

                if (_inputLine.ToString() != _inputHistory[0] && _inputLine.Length != 0)
                    _inputHistory.Add(_inputLine.ToString());

                Clear();
                return finalInput;
            }
        }

        public override void Clear()
        {
            if (Hidden)
                return;
            lock (this)
            {
                _inputCursor = 0;
                _inputLine.Clear();
                Draw();
            }
        }

        public void PositionCursor()
        {

            lock (Container)
            {
                Console.SetCursorPosition(Left + (_inputCursor % Width), Top + (_inputCursor / Width));
            }
        }

        public void WriteChar(char c)
        {
            lock (this)
            {
                lock (Container)
                {
                    if (c != (char)0 && _inputLine.Length < MaxInputLength)
                    {
                        if (_historyIndex != -1)
                        {
                            _inputLine.Clear().Append(_inputHistory[_historyIndex]);
                            _historyIndex = -1;
                        }

                        _inputLine.Insert(_inputCursor, c);
                        _inputCursor++;
                    }
                }
                Draw(_historyIndex);
            }
        }
    }
}