﻿using System.Collections.Generic;

using AC = System.ValueTuple<Interfaces.ActivityDefinitions.Activity, Interfaces.Models.Entities.Entity>;

namespace Interfaces.ActivityContext
{
    public interface IHasPrerequisiteDelegate
    {
        bool HasPrerequisite(AC activityContext);
    }

    public interface IGetPrerequisitesDelegate
    {
        IEnumerable<AC> GetPrerequisites(AC activityContext);
    }

    public interface IPrerequisiteController:
        IHasPrerequisiteDelegate,
        IGetPrerequisitesDelegate
    {
        // ...
    }
}
