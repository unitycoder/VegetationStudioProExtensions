using System;
using UnityEngine;
using UnityEditor;
using System.Linq.Expressions;

namespace VegetationStudioProExtensions
{
    public class BaseEditor<T> : Editor where T : MonoBehaviour
    {
        protected T m_Target
        {
            get { return (T)target; }
        }

        public SerializedProperty FindProperty<TValue>(Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty(RuntimeUtilities.GetFieldPath(expr));
        }

        public void SetErrorBackgroundColor()
        {
            GUI.backgroundColor = GUIStyles.ErrorBackgroundColor;
        }

        public void SetDefaultBackgroundColor()
        {
            GUI.backgroundColor = GUIStyles.DefaultBackgroundColor;
        }

        public void SetGuiEnabled( bool enabled)
        {
            GUI.enabled = enabled;
        }
    }
}