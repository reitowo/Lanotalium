using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection.Emit;
using System.Reflection;
using System;

namespace EasyRequest
{
#if UNITY_EDITOR
    public class RequestTest
    {
        [Name("测试")]
        public int TestFieldInt;

        [Name("测试2")]
        [Range(1, 10)]
        public int TestFieldIntRange;

        [Range(2, 20)]
        public float TestFieldFloat;

        [Name("Bool")]
        public bool TestFieldBool;
    }
#endif
    public class NameAttribute : Attribute
    {
        public string Name { get; private set; }
        public NameAttribute(string Name)
        {
            this.Name = Name;
        }
    }
    public class Request<T> where T : class, new()
    {
        public T Object;
        public bool Succeed;
        public Request()
        {
            Object = new T();
        }
    }
    public class EasyRequestManager : MonoBehaviour
    {
        public static EasyRequestManager Instance { get; set; }
        public Canvas RequestCanvas;
        public RectTransform RequestScrollView, RequestListContent;
        public Text Title, ConfirmText, CancelText;
        public GameObject RequestBoolPrefab, RequestIntPrefab, RequestFloatPrefab, RequestStringPrefab;

        private float contentY;
        private bool confirmed, canceled;
        private List<GameObject> requestGameObjects = new List<GameObject>();
        private List<IRequest> requests = new List<IRequest>();

        private void Start()
        {
            Instance = this;
        }

        private void Initialize()
        {
            foreach (GameObject g in requestGameObjects)
            {
                Destroy(g);
            }
            requestGameObjects.Clear();
            requests.Clear();
            confirmed = false;
            canceled = false;
            contentY = 0;
            ConfirmText.text = LimLanguageManager.TextDict["EasyRequest_Confirm"];
            CancelText.text = LimLanguageManager.TextDict["EasyRequest_Cancel"];
        }
        private void AppendRequestBool(FieldInfo field, object obj)
        {
            GameObject g = Instantiate(RequestBoolPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestBool>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            g.GetComponentInChildren<Toggle>().isOn = (bool)field.GetValue(obj);
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestInt(FieldInfo field, object obj)
        {
            GameObject g = Instantiate(RequestIntPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestInt>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                RequestInt i = g.GetComponent<RequestInt>();
                i.ShouldThreshold = true;
                i.Min = (int)range.min;
                i.Max = (int)range.max;
                r.Description += $" ({i.Min} - {i.Max})";
            }
            g.GetComponentInChildren<InputField>().text = ((int)field.GetValue(obj)).ToString();
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestFloat(FieldInfo field, object obj)
        {
            GameObject g = Instantiate(RequestFloatPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestFloat>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                RequestFloat f = g.GetComponent<RequestFloat>();
                f.ShouldThreshold = true;
                f.Min = range.min;
                f.Max = range.max;
                r.Description += $" ({f.Min} - {f.Max})";
            }
            g.GetComponentInChildren<InputField>().text = ((float)field.GetValue(obj)).ToString();
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestString(FieldInfo field, object obj)
        {
            GameObject g = Instantiate(RequestStringPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestString>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            g.GetComponentInChildren<InputField>().text = (string)field.GetValue(obj);
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void ResolveAndBuildRequests(object obj)
        {
            Type type = obj.GetType();
            if (type.IsPrimitive)
            {
                Title.text = LimLanguageManager.TextDict["EasyRequest_PrimitiveTypes"];
            }
            TypeInfo typeInfo = type.GetTypeInfo();
            foreach (FieldInfo fieldInfo in typeInfo.GetFields())
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType == typeof(bool)) AppendRequestBool(fieldInfo, obj);
                else if (fieldType == typeof(int)) AppendRequestInt(fieldInfo, obj);
                else if (fieldType == typeof(float)) AppendRequestFloat(fieldInfo, obj);
                else if (fieldType == typeof(string)) AppendRequestString(fieldInfo, obj);
                else continue;
                contentY -= 50;
            }
            RequestListContent.sizeDelta = new Vector2(0, -contentY);
        }

        public IEnumerator Request<T>(Request<T> request, string description = null) where T : class, new()
        {
            RequestCanvas.gameObject.SetActive(true);

            Initialize();
            ResolveAndBuildRequests(request.Object);
            Title.text = (description ?? LimLanguageManager.TextDict["EasyRequest_Title_Default"]);
            HasInvalid:
            RequestCanvas.gameObject.SetActive(true);
            while (!confirmed && !canceled) yield return null;
            if (confirmed)
            {
                foreach (IRequest r in requests)
                {
                    if (!r.Validate())
                    {
                        confirmed = false;
                        goto HasInvalid;
                    }
                }
                foreach (IRequest r in requests)
                {
                    r.FieldInfo.SetValue(request.Object, r.Value);
                }
                request.Succeed = true;
            }
            else if (canceled)
            {
                request.Succeed = false;
            }

            RequestCanvas.gameObject.SetActive(false);
        }

        public void Confirm()
        {
            confirmed = true;
            RequestCanvas.gameObject.SetActive(false);
        }
        public void Cancel()
        {
            canceled = true;
            RequestCanvas.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public void Test()
        {
            RequestTest i = new RequestTest();
            StartCoroutine(Request(new EasyRequest.Request<RequestTest>()));
        }
#endif
    }
}
