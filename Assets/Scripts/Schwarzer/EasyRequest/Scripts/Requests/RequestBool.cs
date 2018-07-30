using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EasyRequest
{
    public class RequestBool : MonoBehaviour, IRequest
    {
        public Toggle BoolToggle;
        public Text DescriptionText;
        public FieldInfo FieldInfo { get; set; }

        public object Value
        {
            get
            {
                return BoolToggle.isOn;
            }
        }
        public Type Type
        {
            get
            {
                return typeof(bool);
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
