using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cecer.ConsoleSplit
{
    public class ConsoleContainer
    {
        #region Singleton
        private static ConsoleContainer _instance;
        public static ConsoleContainer Instance
        {
            get
            {
                return _instance;
            }
        }
        private ConsoleContainer(int width, int height)
        {
            Console.WindowWidth = width;
            Console.BufferWidth = width;

            Console.WindowHeight = height;
            Console.BufferHeight = height;
        }
        public static ConsoleContainer Initialize(int width, int height)
        {
            if(_instance != null)
                    throw new Exception("Container is already initialised!");

            _instance = new ConsoleContainer(width, height);

            new Thread(_instance.ReadKeyFiller)
            {
                IsBackground = true,
                Name = "ConsoleContainer_ReadKeyFiller"
            }.Start();

            return _instance;
        }
        #endregion

        private HashSet<LogicalConsole> _logicalConsoles = new HashSet<LogicalConsole>();

        private InLogicalConsole _focusedConsole;
        public InLogicalConsole FocusedConsole
        {
            get
            {
                return _focusedConsole;
            }
            set
            {
                lock (this)
                {
                    _focusedConsole = _logicalConsoles.Contains(value) ? value : null;
                    PositionCursor();
                }
            }
        }

        public void SilentAllConsoles()
        {
            lock (this)
            {
                foreach (LogicalConsole console in _logicalConsoles)
                {
                    console.Hidden = true;
                }
            }
        }

        public void AddLogicalConsole(LogicalConsole console)
        {
            lock (this)
            {
                console.Container = this;
                _logicalConsoles.Add(console);
            }
        }
        public void RemoveLogicalConsole(LogicalConsole console)
        {
            lock (this)
            {
                console.Container = null;
                _logicalConsoles.Remove(console);
            }
        }

        public void PositionCursor()
        {
            if (_focusedConsole != null)
                _focusedConsole.PositionCursor();
        }

        #region ReadKey
        private ConsoleKeyInfo _nextKey;
        private readonly AutoResetEvent KeyWritten = new AutoResetEvent(false);
        private readonly AutoResetEvent KeyRead = new AutoResetEvent(true);
        public ConsoleKeyInfo ReadKey(bool intercept = false, int millisecondTimeout = -1)
        {
            if (millisecondTimeout == -1)
                KeyWritten.WaitOne();
            else
                KeyWritten.WaitOne(millisecondTimeout);

            ConsoleKeyInfo returnedKey = _nextKey;
            if (!intercept)
                _focusedConsole.WriteChar(returnedKey.KeyChar);

            KeyRead.Set();

            return returnedKey;
        }
        internal void ReadKeyFiller()
        {
            while (true)
            {
                KeyRead.WaitOne();
                _nextKey = Console.ReadKey(true);
                KeyWritten.Set();
            }
        }
        #endregion
    }
}
