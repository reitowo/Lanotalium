using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EasyRequest
{
    public class RequestInt : MonoBehaviour, IRequest
    {
        [HideInInspector]
        public bool ShouldThreshold { get; set; }
        [HideInInspector]
        public int Min = int.MinValue;
        [HideInInspector]
        public int Max = int.MaxValue;

        public InputField IntInputField;
        public Text DescriptionText;
        public Color InvalidColor;
        public Image InputFieldImage;

        public object Value
        {
            get
            {
                int value = 0;
                bool result = int.TryParse(IntInputField.text, out value);
                if (!result) return 0;
                return value;
            }
        }
        public Type Type
        {
            get
            {
                return typeof(int);
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
        public FieldInfo FieldInfo { get; set; }

        private bool ValidateInternal()
        {
            int value = 0;
            bool result = int.TryParse(IntInputField.text, out value);
            if (!result) return false;
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