using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cecer.ConsoleSplit
{
    public class OutLogicalConsole : LogicalConsole
    {
        public int CursorLeft
        {
            get;
            set;
        }
        public int CursorTop
        {
            get;
            set;
        }

        private List<TextSegment>[] _buffer;
        public OutLogicalConsole(int width, int height, int left, int top, ConsoleColor initalBackground = ConsoleColor.Black, ConsoleColor initalForeground = ConsoleColor.Gray) : base(width, height, left, top, initalBackground, initalForeground)
        {
            _buffer = new List<TextSegment>[Height];

            for (int line = 0; line < Height; line++)
            {
                _buffer[line] = new List<TextSegment>();
            }
        }
        
        private void AppendSegment(TextSegment segment)
        {
            lock (Container)
            lock (this)
            {
                PrintSegment(segment);
                _buffer[CursorTop].Add(segment);
            }
        }

        private void PrintSegment(TextSegment segment, ref int cursorLeft, ref int cursorTop)
        {
            if (!Hidden)
            {
                Console.ForegroundColor = segment.ForegroundColor;
                Console.BackgroundColor = segment.BackgroundColor;
                Console.SetCursorPosition(Left + cursorLeft, Top + cursorTop);
                Console.Write(segment.Text);
            }

            cursorLeft += segment.Text.Length;

            if (cursorLeft >= Width)
            {
                cursorLeft = 0;
                cursorTop++;
            }
        }
        private void PrintSegment(TextSegment segment)
        {
            if (!Hidden)
            {
                Console.ForegroundColor = segment.ForegroundColor;
                Console.BackgroundColor = segment.BackgroundColor;
                Console.SetCursorPosition(Left + CursorLeft, Top + CursorTop);
                Console.Write(segment.Text);
            }

            CursorLeft += segment.Text.Length;

            if (CursorLeft >= Width)
            {
                CursorLeft = 0;
                CursorTop++;

                if (CursorTop >= Height)
                    ShiftLine();
            }
        }

        private void ShiftLine()
        {
            lock (this)
            {
                for (int line = 1; line < Height; line++)
                {
                    _buffer[line - 1] = _buffer[line];
                }
                _buffer[_buffer.Length - 1] = new List<TextSegment>();
                CursorTop--;

                Draw(); // Maybe try Console.MoveBufferArea? It should be a lot faster
            }
        }

        /// <summary>
        /// Forces a full redraw!
        /// </summary>
        public override void Draw()
        {
            if (Hidden)
                return;

            string blankLine = String.Empty.PadRight(Width);

            lock (Container)
                lock (this)
                {
                    for (int line = 0; line < Height; line++)
                    {
                        // Blank out old text
                        Console.SetCursorPosition(Left, Top + line);
                        /*Console.BackgroundColor = BackgroundColor;
                        Console.ForegroundColor = ForegroundColor;
                        Console.Write(blankLine);*/
                    }

                    // Write new text
                    Console.SetCursorPosition(Left, Top);
                    int cursorX = 0;
                    int cursorY = 0;
                    for (int line = 0; line < Height; line++)
                    {
                        foreach (TextSegment segment in _buffer[line])
                        {
                            PrintSegment(segment, ref cursorX, ref cursorY);
                        }
                    }
                }
        }

        public void Write(string text)
        {
            lock (this)
            {
                int segmentLength = Width - CursorLeft;
                TextSegment segment;

                lock (Container)
                {
                    while (segmentLength < text.Length)
                    {
                        segment = new TextSegment
                            {
                                ForegroundColor = ForegroundColor,
                                BackgroundColor = BackgroundColor,
                                Text = text.Substring(0, segmentLength)
                            };
                        AppendSegment(segment);

                        text = text.Substring(segmentLength);
                        segmentLength = Width;
                    }
                    segment = new TextSegment
                        { ForegroundColor = ForegroundColor, BackgroundColor = BackgroundColor, Text = text };
                    AppendSegment(segment);
                }
                Container.PositionCursor(); 
            }
        }

        public void WriteLine(string text = "")
        {
            lock (this)
            {
                Write(text.PadRight((int)(Math.Ceiling((text.Length + CursorLeft) / (double)Width) * Width) - CursorLeft));
            }
        }

        public override void Clear()
        {
            lock (this)
            {
                lock (Container)
                {
                    if (!Hidden)
                    {
                        Console.ForegroundColor = ForegroundColor;
                        Console.BackgroundColor = BackgroundColor;
                        for (int line = 0; line < Height; line++)
                        {
                            Console.SetCursorPosition(Left, Top + line);
                            Console.Write(String.Empty.PadLeft(Width));
                        }
                    }
                }
                for (int line = 0; line < Height; line++)
                {
                    _buffer[line] = new List<TextSegment>();
                }
                SetCursorPosition(0, 0);

            }
        }

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }
    }
}
