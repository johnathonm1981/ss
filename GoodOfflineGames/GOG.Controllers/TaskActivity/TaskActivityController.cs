﻿using System;
using System.Threading.Tasks;

using Interfaces.Reporting;
using Interfaces.TaskActivity;

namespace GOG.Controllers.TaskActivity
{
    public abstract class TaskActivityController: ITaskActivityController
    {
        protected ITaskReportingController taskReportingController;

        public TaskActivityController(ITaskReportingController taskReportingController)
        {
            this.taskReportingController = taskReportingController;
        }

        public virtual Task ProcessTask()
        {
            throw new NotImplementedException();
        }
    }
}