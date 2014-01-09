using System;

namespace Cecer.ConsoleSplit
{
    public abstract class LogicalConsole
    {
        #region Console Properties

        public bool Hidden
        {
            get;
            set;
        }

        public int Top
        {
            get;
            private set;
        }
        public int Left
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }
        public int Width
        {
            get;
            private set;
        }

        public ConsoleColor BackgroundColor
        {
            get;
            set;
        }
        public ConsoleColor ForegroundColor
        {
            get;
            set;
        }
        public ConsoleContainer Container
        {
            get;
            set;
        }

        #endregion

        protected LogicalConsole(int width, int height, int left, int top, ConsoleColor initalBackground = ConsoleColor.Black, ConsoleColor initalForeground = ConsoleColor.Gray)
        {
            Hidden = true;

            BackgroundColor = initalBackground;
            ForegroundColor = initalForeground;
            Left = left;
            Top = top;
            Width = width;
            Height = height;

            Container = ConsoleContainer.Instance;
        }

        public abstract void Clear();

        public abstract void Draw();
    }
}
