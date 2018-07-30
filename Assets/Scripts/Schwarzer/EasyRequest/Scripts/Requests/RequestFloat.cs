using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EasyRequest
{
    public class RequestFloat : MonoBehaviour, IRequest
    {
        [HideInInspector]
        public bool ShouldThreshold { get; set; }
        [HideInInspector]
        public float Min = float.MaxValue;
        [HideInInspector]
        public float Max = float.MinValue;

        public InputField FloatInputField;
        public Text DescriptionText;
        public FieldInfo FieldInfo { get; set; }
        public Color InvalidColor;
        public Image InputFieldImage;

        public object Value
        {
            get
            {
                float value = 0;
                bool result = float.TryParse(FloatInputField.text, out value);
                if (!result) return 0;
                return value;
            }
        }
        public Type Type
        {
            get
            {
                return typeof(float);
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
        private bool ValidateInternal()
        {
            float value = 0;
            bool result = float.TryParse(FloatInputField.text, out value);
            if (!result) return false;
            if (float.IsNaN(value) || float.IsInfinity(value)) return false;
            if (ShouldThreshold)
            {
                if (value > Max) return false;
                if (value < Min) return false;
            }
            return true;
        }
        public bool Validate()
        {
            bool result = ValidateInternal();
            if (!result)
            {
                InputFieldImage.color = InvalidColor;
                return false;
            }
            else
            {
                InputFieldImage.color = Color.white;
                return true;
            }
        }
    }
}