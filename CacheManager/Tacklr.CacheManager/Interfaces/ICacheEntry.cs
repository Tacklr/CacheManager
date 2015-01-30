using System;
using System.Web.Caching;

namespace Tacklr.CacheManager.Interfaces
{
    internal interface ICacheEntry
    {
        DateTime AbsoluteExpiration { get; set; }
        DateTime Created { get; set; }
        string Key { get; set; }
        CacheItemPriority Priority { get; set; }
        TimeSpan SlidingExpiration { get; set; }
        object Value { get; set; }

        //Dependencies?
        //Entry state?
    }

    internal interface ICacheEntry<T>// : ICacheEntry
    {
        DateTime AbsoluteExpiration { get; set; }
        DateTime Created { get; set; }
        string Key { get; set; }
        CacheItemPriority Priority { get; set; }
        TimeSpan SlidingExpiration { get; set; }
        T Value { get; set; }

        //Dependencies?
        //Entry state?
    }

    //TODO: Move these someplace else.
    internal class CacheEntry : ICacheEntry
    {
        public DateTime AbsoluteExpiration { get; set; }
        public DateTime Created { get; set; }
        public string Key { get; set; }
        public CacheItemPriority Priority { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public object Value { get; set; }
    }

    internal class CacheEntry<T> : ICacheEntry<T>
    {
        public CacheEntry()
        {
        }

        public CacheEntry(CacheEntry entry)
        {
            Key = entry.Key;
            Value = (T)entry.Value;
            Created = entry.Created;
            AbsoluteExpiration = entry.AbsoluteExpiration;
            SlidingExpiration = entry.SlidingExpiration;
            Priority = entry.Priority;
        }

        public DateTime AbsoluteExpiration { get; set; }
        public DateTime Created { get; set; }
        public string Key { get; set; }
        public CacheItemPriority Priority { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public T Value { get; set; }
    }
}