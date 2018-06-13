﻿using System;
using System.Collections.Generic;
using System.Text;

using Interfaces.Delegates.GetDirectory;
using Interfaces.Delegates.GetFilename;

using Interfaces.Controllers.Stash;
using Interfaces.Controllers.Serialization;
using Interfaces.Controllers.Storage;

using Interfaces.Status;

using Interfaces.Models.Entities;

using Controllers.Stash;

using Ghost.Factories.Delegates;

namespace Ghost.Factories.Controllers
{
    public static class StashControllerFactory
    {
        public static IStashController<List<long>> CreateStashController(
            Entity entity,
            IGetDirectoryDelegate getRootDirectoryDelegate,
            IGetFilenameDelegate getFilenameDelegate,
            ISerializationController<string> serializationController,
            IStorageController<string> storageController,
            IStatusController statusController)
        {
            return new StashController<List<long>>(
                GetPathDelegateFactory.CreatePathDelegate(
                    entity,
                    getRootDirectoryDelegate,
                    getFilenameDelegate),
                serializationController,
                storageController,
                statusController);
        }
    }
}