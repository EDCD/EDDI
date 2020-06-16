using System;
using System.Collections.Concurrent;

namespace Utilities
{

    // Code from https://www.codeproject.com/Tips/1190802/File-Locking-in-a-Multi-Threaded-Environment
    // The code maintains a concurrent dictionary of lock objects and keeps track of how many calls are in the queue for
    // accessing each filename. If the count goes down to 0, the key is removed from the dictionary as a cleanup mechanism.
    public class LockManager
    {
        private static readonly ConcurrentDictionary<string, lobj> _locks =
                new ConcurrentDictionary<string, lobj>();

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

        private static lobj GetLock(string lockName)
        {
            if (_locks.TryGetValue(lockName.ToLower(), out var o))
            {
                o.count++;
                return o;
            }
            else
            {
                o = new lobj();
                _locks.TryAdd(lockName.ToLower(), o);
                o.count++;
                return o;
            }
        }

        private static void Unlock(string lockName)
        {
            if (_locks.TryGetValue(lockName.ToLower(), out var o))
            {
                o.count--;
                if (o.count != 0) { return; }
                _locks.TryRemove(lockName.ToLower(), out lobj _);
            }
        }
    }

    class lobj
    {
        public int count = 0;
    }
}
