﻿using System;
using System.Threading.Tasks;

using Interfaces.Activity;
using Interfaces.Status;

namespace GOG.Activities
{
    public abstract class Activity: IActivity
    {
        protected IStatusController statusController;

        protected Activity(
            IStatusController statusController)
        {
            this.statusController = statusController;
        }

        public virtual Task ProcessActivityAsync(IStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
