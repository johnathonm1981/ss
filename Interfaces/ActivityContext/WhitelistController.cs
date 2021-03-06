﻿using AC = System.ValueTuple<Interfaces.ActivityDefinitions.Activity, Interfaces.Models.Entities.Entity>;

namespace Interfaces.ActivityContext
{
    // TODO: convert to Confirm delegate
    public interface IIsWhitelistedDelegate
    {
        bool IsWhitelisted(AC activityContext);
    }

    public interface IWhitelistController:
        IIsWhitelistedDelegate
    {
        // ...
    }
}
