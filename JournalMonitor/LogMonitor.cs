﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiJournalMonitor
{
    public class LogMonitor
    {
        // What we are monitoring and what to do with it
        public string Directory;
        public Regex Filter;
        public Action<string, bool> Callback;
        public static string journalFileName;

        private const int pollingIntervalActiveMs = 100;
        private const int pollingIntervalRelaxedMs = 5000;

        // Keep track of status
        private bool running;

        public LogMonitor(string filter) { Filter = new Regex(filter); }

        public LogMonitor(string directory, string filter, Action<string, bool> callback)
        {
            Directory = directory;
            Filter = new Regex(filter);
            Callback = callback;
        }

        /// <summary>Monitor the netlog for changes, running a callback when the file changes</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public void start(bool readAllOnLoad = false)
        {
            if (Directory == null || Directory.Trim() == "")
            {
                return;
            }

            running = true;
            long lastSize = 0;
            bool activePolling = false;

            // Main loop
            while (running)
            {
                if (!string.IsNullOrEmpty(journalFileName))
                {
                    // We've already found and processed a journal file. 
                    // We'll use relaxed file system polling unless the game is running.
                    if (Processes.IsEliteRunning())
                    {
                        activePolling = true;
                    }
                    else
                    {
                        activePolling = false;
                        Thread.Sleep(pollingIntervalRelaxedMs);
                        continue;
                    }
                }

                var fileInfo = FindLatestFile(Directory, Filter);
                if (fileInfo == null)
                {
                    // A player journal file could not be found. Sleep until a player journal file is found.
                    Logging.Warn("Error locating Elite Dangerous player journal. Journal monitor is not active. Have you installed and run Elite Dangerous previously? ");
                    while (fileInfo == null)
                    {
                        Thread.Sleep(500);
                        fileInfo = FindLatestFile(Directory, Filter);
                    }
                    Logging.Info("Elite Dangerous player journal found. Journal monitor activated.");
                    return;
                }
                else if (fileInfo.Name != journalFileName)
                {
                    // We have found a player journal file that is fresher than the one we are using
                    bool isFirstLoad = journalFileName == null;
                    journalFileName = fileInfo.Name;
                    lastSize = fileInfo.Length;

                    if (readAllOnLoad)
                    {
                        // Read everything in the file into the journal monitor
                        long seekPos = 0;
                        int readLen = (int)fileInfo.Length;
                        Read(seekPos, readLen, fileInfo, isFirstLoad);
                    }
                    else
                    {
                        // Read the header and latest loaded game into the journal monitor
                        ReadLastCommanderLoad(fileInfo, isFirstLoad);
                    }
                }
                else
                {
                    // The player journal file in memory is the correct file. Look for new journal events
                    journalFileName = fileInfo.Name;

                    long thisSize = fileInfo.Length;
                    int readLen = 0;
                    long seekPos = 0;

                    if (lastSize != thisSize)
                    {
                        if (thisSize > lastSize)
                        {
                            // File has been appended - read the remaining info
                            seekPos = lastSize;
                            readLen = (int)(thisSize - lastSize);
                        }
                        else if (thisSize < lastSize)
                        {
                            // File has been truncated - read all of the info
                            seekPos = 0;
                            readLen = (int)thisSize;
                        }
                        Read(seekPos, readLen, fileInfo, false);
                    }
                    lastSize = thisSize;
                }
                Thread.Sleep(activePolling ? pollingIntervalActiveMs : pollingIntervalRelaxedMs);
            }
        }

        private void Read(long seekPos, int readLen, FileInfo fileInfo, bool isLoadEvent)
        {
            using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(seekPos, SeekOrigin.Begin);
                byte[] bytes = new byte[readLen];
                int haveRead = 0;
                while (haveRead < readLen)
                {
                    haveRead += fs.Read(bytes, haveRead, readLen - haveRead);
                    fs.Seek(seekPos + haveRead, SeekOrigin.Begin);
                }
                // Convert bytes to string
                string s = Encoding.UTF8.GetString(bytes);
                string[] lines = Regex.Split(s, "\r?\n");
                foreach (string line in lines)
                {
                    if (line != "")
                    {
                        Callback(line, isLoadEvent);
                    }
                }
            }
        }

        private void ReadLastCommanderLoad(FileInfo fileInfo, bool isLoadEvent)
        {
            long seekPos = 0;

            using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(seekPos, SeekOrigin.Begin);
                byte[] bytes = new byte[fileInfo.Length];
                int haveRead = 0;
                while (haveRead < fileInfo.Length)
                {
                    haveRead += fs.Read(bytes, haveRead, (int)fileInfo.Length - haveRead);
                    fs.Seek(seekPos + haveRead, SeekOrigin.Begin);
                }
                // Convert bytes to strings
                string s = Encoding.UTF8.GetString(bytes);
                string[] lines = Regex.Split(s, "\r?\n");

                // First line should be a file header
                string firstLine = lines.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstLine) && firstLine.Contains("Fileheader"))
                {
                    // Pass this along as an event
                    Callback(firstLine, isLoadEvent);
                }

                // Find the latest "Commander" event, written at the start of the Load Game process
                // (whenever loading from the main menu) 
                var commanderLoadLines = lines
                    .Select((text, index) => new { line = text, lineNumber = index })
                    .Where(x => x.line.Contains(@"""event"":""Commander"""));
                var lastLoadLine = commanderLoadLines.LastOrDefault();

                if (lastLoadLine != null)
                {
                    Task.Run(() => {
                        for (int i = lastLoadLine.lineNumber; i < lines.Count(); i++)
                        {
                            if (lines[i] != "")
                            {
                                Callback(lines[i], isLoadEvent);
                            }
                        }
                    });
                }
            }
        }

        public void stop()
        {
            running = false;
        }

        /// <summary>Find the latest file in a given directory matching a given expression, or null if no such file exists</summary>
        private static FileInfo FindLatestFile(string path, Regex filter = null)
        {
            if (path == null)
            {
                // Configuration can be changed underneath us so we do have to check each time...
                return null;
            }

            DirectoryInfo directory = null;
            try
            {
                directory = new DirectoryInfo(path);
            }
            catch (NotSupportedException nsex)
            {
                Logging.Error("Directory path " + path + " not supported: ", nsex);
            }

            if (directory != null)
            {
                try
                {
                    FileInfo info = directory.GetFiles().Where(f => filter == null || filter.IsMatch(f.Name)).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    // This info can be cached so force a refresh
                    info?.Refresh();
                    return info;
                }
                catch { } // Nothing to do here
            }

            return null;
        }
    }
}
