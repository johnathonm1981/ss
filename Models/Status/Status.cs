﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Interfaces.Status;

namespace Models.Status
{
    [DataContract]
    [KnownType(typeof(Status))]
    [KnownType(typeof(StatusProgress))]
    public class Status: IStatus
    {
        [DataMember(Name = "complete")]
        public bool Complete { get; set; }
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "progress")]
        public IStatusProgress Progress { get; set; }
        [DataMember(Name = "started")]
        public DateTime Started { get; set; } = DateTime.UtcNow;
        [DataMember(Name = "completed")]
        public DateTime Completed { get; set; }
        [DataMember(Name = "children")]
        public IList<IStatus> Children { get; set; }
        [DataMember(Name = "warnings")]
        public IList<string> Warnings { get; set; }
        [DataMember(Name = "failures")]
        public IList<string> Failures { get; set; }
        [DataMember(Name = "information")]
        public IList<string> Information { get; set; }
        [DataMember(Name = "summaryResults")]
        public IList<string> SummaryResults { get; set; }
    }

    [DataContract]
    public class StatusProgress: IStatusProgress
    {
        [DataMember(Name = "target")]
        public string Target { get; set; }
        [DataMember(Name = "current")]
        public long Current { get; set; }
        [DataMember(Name = "total")]
        public long Total { get; set; }
        [DataMember(Name = "unit")]
        public string Unit { get; set; }
    }
}
