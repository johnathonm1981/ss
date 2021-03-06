﻿using System;
using System.Linq;
using System.Collections.Generic;

using Interfaces.ActivityContext;
using Interfaces.ActivityDefinitions;
using Interfaces.Models.Entities;

using Models.Separators;

using AC = System.ValueTuple<Interfaces.ActivityDefinitions.Activity, Interfaces.Models.Entities.Entity>;

namespace Controllers.ActivityContext
{
    public class ActivityContextController : IActivityContextController
    {
        IAliasController aliasController;
        IWhitelistController whitelistController;
        IPrerequisiteController prerequisiteController;
        ISupplementaryController supplementaryController;

        public ActivityContextController(
            IAliasController aliasController,
            IWhitelistController whitelistController,
            IPrerequisiteController prerequisiteController,
            ISupplementaryController supplementaryController)
        {
            this.aliasController = aliasController;
            this.whitelistController = whitelistController;
            this.prerequisiteController = prerequisiteController;
            this.supplementaryController = supplementaryController;
        }

        public AC ParseSingle(string activityContext)
        {
            if (string.IsNullOrEmpty(activityContext))
                throw new ArgumentNullException();

            var context = Entity.None;

            var parts = activityContext.Split(
                new string[] { Separators.ActivityContext },
                StringSplitOptions.RemoveEmptyEntries);

            var activity = (Activity)Enum.Parse(typeof(Activity), parts[0], true);
            if (parts.Length > 1)
                context = (Entity)Enum.Parse(typeof(Entity), parts[1], true);

            return (activity, context);
        }

        public IEnumerable<AC> GetQueue(string[] args)
        {
            var activityContextQueue = new List<AC>();

            // if nothing was requested - show help
            if (args.Length < 1)
            {
                activityContextQueue.Add((Activity.Help, Entity.None));
                return activityContextQueue;
            }

            var requestedActivityContext = args[0];

            // check if the requested operation is alias or known activity-context, expand alias as needed

            if (aliasController.IsAlias(requestedActivityContext))
                activityContextQueue.AddRange(
                    aliasController.ExpandAlias(requestedActivityContext));
            else
            {
                var activityContext = ParseSingle(requestedActivityContext);
                if (whitelistController.IsWhitelisted(activityContext))
                    activityContextQueue.Add(activityContext);
            }

            // add prerequisites for the queued activity-context items

            var prerequisites = new List<AC>();

            foreach (var activityContext in activityContextQueue)
                if (prerequisiteController.HasPrerequisite(activityContext))
                {
                    var activityContextPrerequisites = prerequisiteController.GetPrerequisites(activityContext);
                    foreach (var prerequisite in activityContextPrerequisites)
                        if (!prerequisites.Contains(prerequisite))
                            prerequisites.Add(prerequisite);
                }

            activityContextQueue.InsertRange(0, prerequisites);

            // finally, adding supplementary entries
            activityContextQueue.AddRange(supplementaryController.GetSupplementary());

            return activityContextQueue;
        }

        public IEnumerable<string> GetParameters(string[] args)
        {
            if (args == null) return new string[0];
            if (args.Length < 2) return new string[0];

            return args.Skip(1);
        }

        public string ToString(AC activityContext)
        {
            var activityString = activityContext.Item1.ToString();
            var context = activityContext.Item2;

            return
                context != Entity.None ?
                activityString + Separators.ActivityContext + context :
                activityString;
        }
    }
}
