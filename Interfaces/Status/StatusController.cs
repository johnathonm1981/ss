﻿using System.Threading.Tasks;

namespace Interfaces.Status
{
    public interface ICreateAsyncDelegate
    {
        Task<IStatus> CreateAsync(IStatus status, string title, bool notifyStatusChanged = true);
    }

    // TODO: Consider using IDisposale to allow using pattern
    public interface ICompleteAsyncDelegate
    {
        Task CompleteAsync(IStatus status, bool notifyStatusChanged = true);
    }

    public interface IUpdateProgressAsyncDelegate
    {
        Task UpdateProgressAsync(IStatus status, long current, long total, string target, string unit = "");
    }

    public interface IFailAsyncDelegate
    {
        Task FailAsync(IStatus status, string failureMessage);
    }

    public interface IWarnAsyncDelegate
    {
        Task WarnAsync(IStatus status, string warningMessage);
    }

    public interface IInformAsyncDelegate
    {
        Task InformAsync(IStatus status, string informationMessage);
    }

    public interface IPostSummaryResultsAsyncDelegate
    {
        Task PostSummaryResultsAsync(IStatus status, params string[] summaryResults);
    }

    public delegate Task StatusChangedNotificationAsyncDelegate();

    public interface INotifyStatusChangedAsyncEvent
    {
        event StatusChangedNotificationAsyncDelegate NotifyStatusChangedAsync;
    }

    public interface IStatusController:
        ICreateAsyncDelegate,
        ICompleteAsyncDelegate,
        IUpdateProgressAsyncDelegate,
        IFailAsyncDelegate,
        IWarnAsyncDelegate,
        IInformAsyncDelegate,
        IPostSummaryResultsAsyncDelegate,
        INotifyStatusChangedAsyncEvent
    {
        // ...
    }
}
