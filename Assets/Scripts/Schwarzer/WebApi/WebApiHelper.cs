using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Schwarzer.Lanotalium.WebApi
{
    public class WebApiHelper
    {
        public static string WebApiUri = "https://lanotaliumapi.schwarzer.wang/";
        public static IEnumerator PostObjectCoroutine(string Route, object Object, ObjectWrap<string> Response = null)
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
                        if (Response != null)
                        {
                            Response.Reference = result;
                        }
                    }
                }
                catch (Exception Ex)
                {
                    Debug.Log(Ex);
                }
            }
        }
        public static async Task<string> PostObjectAsync(string Route, object Object)
        {
            try
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

                    var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        //Debug.Log(result);
                        return result;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.Log(Ex);
                return null;
            }
        }
        public static async Task<string> PostStringAsync(string Route, string String)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(WebApiUri + Route);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write("=" + String);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        //Debug.Log(result);
                        return result;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.Log(Ex);
                return null;
            }
        }
        public static IEnumerator PostStringCoroutine(string Route,string String)
        {
            Task<string> task = PostStringAsync(Route, String);
            while (!task.IsCompleted) yield return null;
        }
        public static IEnumerator PostFormCoroutine(string Route, WWWForm Data, ObjectWrap<string> Response = null, Action<float> ProgressCallback = null)
        {
            WWW Post = new WWW(WebApiUri + Route, Data);
            while (!Post.isDone)
            {
                ProgressCallback?.Invoke(Post.progress);
                yield return null;
            }
            Response.Reference = Post.text;
        }
    }
}