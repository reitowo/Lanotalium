using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
namespace Schwarzer.Lanotalium.WebApi
{
    public class WebApiHelper
    {
        public static string WebApiUri = "http://api.lanotalium.cn/";
        public static IEnumerator Post(string Route, object Object)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(WebApiUri + Route);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(Object));
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponseTask = httpWebRequest.GetResponseAsync();
                while (!httpResponseTask.IsCompleted)
                {
                    yield return null;
                }
                try
                {
                    var httpResponse = (HttpWebResponse)httpResponseTask.Result;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        //Debug.Log(result);
                    }
                }
                catch (Exception Ex)
                {
                    Debug.Log(Ex);
                }
            }
        }
    }
}