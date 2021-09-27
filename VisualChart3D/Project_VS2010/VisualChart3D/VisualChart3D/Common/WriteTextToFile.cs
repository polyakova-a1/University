// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.IO;

namespace VisualChart3D.Common
{

    class WriteTextToFile : IDisposable
    {
        #region Fields
        /// <summary>
        /// strean writer
        /// </summary>
        private StreamWriter _stWrite;

        /// <summary>
        /// locker
        /// </summary>
        private readonly object _locker = new object();

        /// <summary>
        /// files names
        /// </summary>
        private readonly List<StreamWriter> _streams;
        #endregion

        #region Constructor
        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="fileName">file name log</param>
        /// <param name="fileNames">others file names</param>
        public WriteTextToFile(string fileName, params string[] fileNames)
        {
            _stWrite = new StreamWriter(File.Create(fileName));
            if (fileNames != null && fileNames.Length != 0)
            {
                _streams = new List<StreamWriter>(fileNames.Length);
                foreach (string name in fileNames)
                {
                    _streams.Add(new StreamWriter(File.Create(name)));
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Write line Format string
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">args</param>
        public void WriteLineFormat(string format, params object[] args)
        {
            lock (_locker)
            {
                _stWrite.WriteLine(format, args);
                if (_streams != null)
                {
                    foreach (StreamWriter sw in _streams)
                    {
                        sw.WriteLine(format, args);
                    }
                }
            }
        }

        /// <summary>
        /// Write line Format string
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args1">arg 1</param>
        /// <param name="args2">arg 2</param>
        /// <param name="args3">arg 3</param>
        public void WriteLineFormat(string format, string args1, string args2, string args3)
        {
            lock (_locker)
            {
                _stWrite.WriteLine(format, args1, args2, args3);
                if (_streams != null)
                {
                    foreach (StreamWriter sw in _streams)
                    {
                        sw.WriteLine(format, args1, args2, args3);
                    }
                }
            }
        }

        /// <summary>
        /// Write line
        /// </summary>
        /// <param name="str">string</param>
        public void WriteLine(string str)
        {
            lock (_locker)
            {
                _stWrite.WriteLine(str);
                if (_streams != null)
                {
                    foreach (StreamWriter sw in _streams)
                    {
                        sw.WriteLine(str);
                    }
                }
            }
        }

        /// <summary>
        /// Write string to file
        /// </summary>
        /// <param name="str">string</param>
        public void Write(string str)
        {
            lock (_locker)
            {
                _stWrite.Write(str);
                if (_streams != null)
                {
                    foreach (StreamWriter sw in _streams)
                    {
                        sw.Write(str);
                    }
                }
            }
        }

        /// <summary>
        /// Close file
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_stWrite != null)
            {
                _stWrite.Close();
                _stWrite = null;
            }
            if (_streams != null)
            {
                for (int i = 0; i < _streams.Count; i++)
                {
                    if (_streams[i] != null)
                    {
                        _streams[i].Close();
                        _streams[i] = null;
                    }
                }
            }
        }
        #endregion
    }
}
