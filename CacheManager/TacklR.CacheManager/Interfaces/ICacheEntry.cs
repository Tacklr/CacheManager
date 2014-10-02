﻿using System;
using System.Collections.Generic;
using System.Web.Caching;

namespace TacklR.CacheManager.Interfaces
{
    internal interface ICacheEntry
    {
        string Key { get; set; }
        object Value { get; set; }
        DateTime AbsoluteExpiration { get; set; }
        TimeSpan SlidingExpiration { get; set; }
        CacheItemPriority Priority { get; set; }
        //Dependencies?
        //Entry state?
    }

    internal interface ICacheEntry<T>
    {
        string Key { get; set; }
        T Value { get; set; }
        DateTime AbsoluteExpiration { get; set; }
        TimeSpan SlidingExpiration { get; set; }
        CacheItemPriority Priority { get; set; }
        //Dependencies?
        //Entry state?
    }

    //TODO: Move these someplace else.
    internal class CacheEntry : ICacheEntry
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; }
    }

    internal class CacheEntry<T> : ICacheEntry<T>
    {
        public string Key { get; set; }
        public T Value { get; set; }
        public DateTime AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; }
    }
}