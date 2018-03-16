using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Schwarzer.Lanotalium.WebApi.ChartZone
{
    public class ChartDto
    {
        public int Id;
        public string ChartName;
        public string Designer;
        public string Size;
        public int NoteCount;
        public int BilibiliAvIndex;
        public double AvgRating;
        public int UsrRating;
    }

    public enum SubmissionStatus
    {
        NotUploaded,
        Pending,
        Accepted,
        Rejected,
        Modified
    }

    public class SubmitDto
    {
        public int SubmissionId;
        public string UserId;
        public SubmissionStatus Status;
        public ChartDto ChartDto;
        public string RejectReason;
    }
}