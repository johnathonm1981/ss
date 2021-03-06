﻿using Interfaces.Delegates.Hash;
using Interfaces.Delegates.Itemize;

using Interfaces.Controllers.Data;

namespace Interfaces.Controllers.Hash
{
    public interface IStoredHashController:
        IGetHashAsyncDelegate<string>,
        ISetHashAsyncDelegate<string>,
        IItemizeAllAsyncDelegate<string>,
        ICommitAsyncDelegate
    {
        // ...
    }
}
