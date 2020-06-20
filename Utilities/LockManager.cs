using System;
using System.Collections.Concurrent;

namespace Utilities
{

    // Code from https://www.codeproject.com/Tips/1190802/File-Locking-in-a-Multi-Threaded-Environment
    // The code maintains a concurrent dictionary of lock objects and keeps track of how many calls are in the queue for
    // accessing each filename. If the count goes down to 0, the key is removed from the dictionary as a cleanup mechanism.
    public static class LockManager
    {
        private static readonly ConcurrentDictionary<string, lockCount> _locks = new ConcurrentDictionary<string, lockCount>();

        public static void GetLock(string lockName, Action action)
        {
            lock (GetLock(lockName))
            {
                try
                {
                    action();
                }
                finally
                {
                    Unlock(lockName);
                }
            }
        }

        private static lockCount GetLock(string lockName)
        {
            if (_locks.TryGetValue(lockName.ToLower(), out lockCount lc))
            {
                lc.count++;
                return lc;
            }
            else
            {
                lc = new lockCount();
                _locks.TryAdd(lockName.ToLower(), lc);
                lc.count++;
                return lc;
            }
        }

        private static void Unlock(string lockName)
        {
            if (_locks.TryGetValue(lockName.ToLower(), out lockCount lc))
            {
                lc.count--;
                if (lc.count != 0) { return; }
                _locks.TryRemove(lockName.ToLower(), out lockCount _);
            }
        }
    }

    internal class lockCount
    {
        public int count = 0;
    }
}
