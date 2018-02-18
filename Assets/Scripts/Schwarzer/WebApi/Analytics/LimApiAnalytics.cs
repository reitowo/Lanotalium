using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schwarzer.Lanotalium.WebApi;

namespace Schwarzer.Lanotalium.WebApi.Analytics
{
    public class AnaDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
    }
    public static class LimApiAnalytics
    {
        public static async void GatherAnalysis(string AnalysisName)
        {
            await WebApiHelper.PostObjectAsync("analytics/gather", new AnaDto()
            {
                UserId = SystemInfo.deviceUniqueIdentifier,
                Name = AnalysisName,
                Value = 1
            });
        }
    }
}