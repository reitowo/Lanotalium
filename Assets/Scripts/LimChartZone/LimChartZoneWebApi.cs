using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Schwarzer.Lanotalium.WebApi;
using System.Net;
using System.IO;

public class Ref<T>
{
    public T Reference;
}

public static class LimChartZoneWebApi
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

    public static IEnumerator GetAllCharts(Ref<List<ChartDto>> charts)
    {
        WWW Get = new WWW("http://api.lanotalium.cn/chartzone/charts/enum/" + SystemInfo.deviceUniqueIdentifier);
        yield return Get;
        //Debug.Log(Get.text);
        charts.Reference = JsonConvert.DeserializeObject<List<ChartDto>>(Get.text);
    }
    public static IEnumerator GetChartById(int id, Ref<ChartDto> chartDto)
    {
        WWW Get = new WWW("http://api.lanotalium.cn/chartzone/charts/get/" + SystemInfo.deviceUniqueIdentifier + "/" + id.ToString());
        yield return Get;
        //Debug.Log(Get.text);
        chartDto.Reference = JsonConvert.DeserializeObject<ChartDto>(Get.text);
    }
    public static IEnumerator AddChart(ChartDto chart)
    {
        yield return WebApiHelper.Post("chartzone/charts/add", chart);
    }
    public static IEnumerator PostRating(int id, Rating rating)
    {
        yield return WebApiHelper.Post("chartzone/charts/rating/set/" + id.ToString(), rating);
    }
}
