using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EasyRequest
{
    public interface IRequest
    {
        string Description { get; set; }
        object Value { get; }
        Type Type { get; }
        bool Validate();
        FieldInfo FieldInfo { get; set; }
    }
}
