using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Schwarzer.Lanotalium.WebApi;
using System.Net;
using System.IO;
using System;

namespace Lanotalium.ChartZone.WebApi
{
    public class Chart
    {
        public int Id { get; set; }
        public string ChartName { get; set; }
        public string Designer { get; set; }
        public string Size { get; set; }
        public int NoteCount { get; set; }
        public int BilibiliAvIndex { get; set; }
        public List<Rating> Ratings { get; set; }
    }
    public class ChartDto
    {
        public int Id { get; set; }
        public string ChartName { get; set; }
        public string Designer { get; set; }
        public string Size { get; set; }
        public int NoteCount { get; set; }
        public int BilibiliAvIndex { get; set; }
        public double AvgRating { get; set; }
        public int UsrRating { get; set; }
    }
    public class Rating
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Rate { get; set; }
    }

    public static class LimChartZoneWebApi
    {
        public static IEnumerator GetAllCharts(ObjectWrap<List<ChartDto>> charts)
        {
            WWW Get = new WWW("https://lanotaliumapi.schwarzer.wang/chartzone/charts/enum/" + SystemInfo.deviceUniqueIdentifier);
            yield return Get;
            //Debug.Log(Get.text);
            try
            {
                charts.Reference = JsonConvert.DeserializeObject<List<ChartDto>>(Get.text);
            }
            catch (Exception Ex)
            {
                Debug.Log(Ex);
            }
        }
        public static IEnumerator GetChartById(int id, ObjectWrap<ChartDto> chartDto)
        {
            WWW Get = new WWW("https://lanotaliumapi.schwarzer.wang/chartzone/charts/get/" + SystemInfo.deviceUniqueIdentifier + "/" + id.ToString());
            yield return Get;
            //Debug.Log(Get.text);
            chartDto.Reference = JsonConvert.DeserializeObject<ChartDto>(Get.text);
        }
        public static IEnumerator AddChart(ChartDto chart)
        {
            yield return WebApiHelper.PostObjectCoroutine("chartzone/charts/add", chart);
        }
        public static IEnumerator PostRating(int id, Rating rating)
        {
            yield return WebApiHelper.PostObjectCoroutine("chartzone/charts/rating/set/" + id.ToString(), rating);
        }
    }
}


