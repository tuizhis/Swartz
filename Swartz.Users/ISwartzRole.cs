﻿using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public interface ISwartzRole<out TKey> : IRole<TKey>
    {
    }

    public interface ISwartzRole : ISwartzRole<decimal>
    {
    }
}