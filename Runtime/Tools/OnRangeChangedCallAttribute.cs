using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace FogOfWarPackage
{
    [AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class OnRangeChangedCallAttribute : PropertyAttribute
    {
        public string methodName;
        
        public readonly int min;
        public readonly int max;
        
        public OnRangeChangedCallAttribute(int min, int max, string methodNameNoArguments)
        {
            methodName = methodNameNoArguments;
            this.min = min;
            this.max = max;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OnRangeChangedCallAttribute))]
    public class OnChangedCallAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            OnRangeChangedCallAttribute rangeAttribute = (OnRangeChangedCallAttribute)base.attribute;
 
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int value = property.intValue;
                value = EditorGUI.IntSlider(position, label, value, rangeAttribute.min, rangeAttribute.max);
                property.intValue = value;
            }
            else
            {
                EditorGUI.LabelField (position, label.text, "Use Range with float or int.");
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                OnRangeChangedCallAttribute at = attribute as OnRangeChangedCallAttribute;
                MethodInfo[] methods = property.serializedObject.targetObject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                MethodInfo method = methods.First(m => m.Name == at.methodName);
                
                if (method != null && method.GetParameters().Count() == 0) // Only instantiate methods with 0 parameters
                    method.Invoke(property.serializedObject.targetObject, null);
            }
        }
    }

#endif
}