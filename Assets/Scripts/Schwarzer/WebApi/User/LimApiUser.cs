using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Schwarzer.Lanotalium.WebApi.User
{
    public class UserDto
    {
        public string UserId;
        public string Version;
        public bool Is64Bit;
        public string Language;
    }
    public static class LimApiUser
    {
        public static IEnumerator ReportAlive(string Version)
        {
            yield return WebApiHelper.PostStringCoroutine("user/alive", Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new UserDto()
            {
                UserId = SystemInfo.deviceUniqueIdentifier,
                Version = Version,
                Is64Bit = Environment.Is64BitProcess,
                Language = Application.systemLanguage.ToString()
            }))));
        }
    }
}