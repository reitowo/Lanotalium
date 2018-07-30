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
        [Default(12)]
        public int TestFieldInt;

        [Name("测试2")]
        [Range(1, 10)]
        [Default(8)]
        public int TestFieldIntRange;

        [Range(2, 20)]
        [Default(2)]
        public float TestFieldFloat;

        [Name("Bool")]
        [Default(false)]
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
    public class DefaultAttribute : Attribute
    {
        public object Value { get; private set; }
        public DefaultAttribute(bool Value)
        {
            this.Value = Value;
        }
        public DefaultAttribute(float Value)
        {
            this.Value = Value.ToString();
        }
        public DefaultAttribute(string Value)
        {
            this.Value = Value;
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
        private void AppendRequestBool(FieldInfo field)
        {
            GameObject g = Instantiate(RequestBoolPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestBool>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            if (field.GetCustomAttribute<DefaultAttribute>() != null)
            {
                try
                {
                    bool value = (bool)field.GetCustomAttribute<DefaultAttribute>().Value;
                    g.GetComponentInChildren<Toggle>().isOn = value;
                }
                catch (Exception)
                {

                }
            }
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestInt(FieldInfo field)
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
            if (field.GetCustomAttribute<DefaultAttribute>() != null)
            {
                try
                {
                    g.GetComponentInChildren<InputField>().text = (string)field.GetCustomAttribute<DefaultAttribute>().Value;
                }
                catch (Exception)
                {

                }
            }
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestFloat(FieldInfo field)
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
            if (field.GetCustomAttribute<DefaultAttribute>() != null)
            {
                try
                {
                    g.GetComponentInChildren<InputField>().text = (string)field.GetCustomAttribute<DefaultAttribute>().Value;
                }
                catch (Exception)
                {

                }
            }
            requestGameObjects.Add(g);
            requests.Add(r);
        }
        private void AppendRequestString(FieldInfo field)
        {
            GameObject g = Instantiate(RequestStringPrefab, RequestListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentY);
            IRequest r = g.GetComponent<RequestString>() as IRequest;
            r.FieldInfo = field;
            r.Description = (field.GetCustomAttribute<NameAttribute>() == null ? field.Name : field.GetCustomAttribute<NameAttribute>().Name);
            if (field.GetCustomAttribute<DefaultAttribute>() != null)
            {
                try
                {
                    g.GetComponentInChildren<InputField>().text = (string)field.GetCustomAttribute<DefaultAttribute>().Value;
                }
                catch (Exception)
                {

                }
            }
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
                if (fieldType == typeof(bool)) AppendRequestBool(fieldInfo);
                else if (fieldType == typeof(int)) AppendRequestInt(fieldInfo);
                else if (fieldType == typeof(float)) AppendRequestFloat(fieldInfo);
                else if (fieldType == typeof(string)) AppendRequestString(fieldInfo);
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
        }
        public void Cancel()
        {
            canceled = true;
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
