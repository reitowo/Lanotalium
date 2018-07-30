using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EasyRequest
{
    public class RequestString : MonoBehaviour, IRequest
    {
        public InputField StringInputField;
        public Text DescriptionText;
        public FieldInfo FieldInfo { get; set; }

        public object Value
        {
            get
            {
                return StringInputField.text;
            }
        }
        public Type Type
        {
            get
            {
                return typeof(string);
            }
        }
        public string Description
        {
            get
            {
                return DescriptionText.text;
            }
            set
            {
                DescriptionText.text = value;
            }
        }
        public bool Validate()
        {
            return true;
        }
    }
}