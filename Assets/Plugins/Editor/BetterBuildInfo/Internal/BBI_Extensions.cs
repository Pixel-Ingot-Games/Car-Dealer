// Copyright (c) 2016 Piotr Gwiazdowski. All rights reserved.
// This file is a part of Better Build Info project.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    internal static class Extensions
    {
        public static SerializedProperty FindPropertyRelativeOrThrow(this SerializedProperty prop, string name)
        {
            var result = prop.FindPropertyRelative(name);
            if (result == null)
                throw new System.ArgumentOutOfRangeException("Property " + name + " not found (relative to " + prop.propertyPath + ")");
            return result;
        }

        public static SerializedProperty FindPropertyOrThrow(this SerializedObject obj, string name)
        {
            var result = obj.FindProperty(name);
            if (result == null)
                throw new System.ArgumentOutOfRangeException("Property " + name + " not found (object type: " + obj.targetObject.GetType() + ")");
            return result;
        }

        public static Rect Union(this Rect a, Rect b)
        {
            var xMin = Mathf.Min(a.xMin, b.xMin);
            var yMin = Mathf.Min(a.yMin, b.yMin);
            var xMax = Mathf.Max(a.xMax, b.xMax);
            var yMax = Mathf.Max(a.yMax, b.yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static void GetFiledValueOrThrow<T>(this System.Type type, object obj, string fieldName, out T variable)
        {
            variable = default(T);

            var fieldInfo = obj.GetType().GetField(fieldName);
            if ( fieldInfo == null )
            {
                throw new ArgumentOutOfRangeException("Field " + fieldName + " of type " + obj.GetType() + " not found");
            }

            var value = fieldInfo.GetValue(obj);
            try
            {
                variable = (T)value;
            }
            catch (System.InvalidCastException ex)
            {
                throw new System.InvalidOperationException("Field " + fieldName + " of type " + obj.GetType() + " is of invalid type", ex);
            }
        }

        public static PropertyInfo GetPropertyOrThrow(this Type type, string name, BindingFlags flags = BindingFlags.Default)
        {
            try
            {
                var result = type.GetProperty(name);
                if (result == null)
                {
                    throw new System.ArgumentOutOfRangeException($"Property {type.FullName}.{name} not found");
                }
                return result;
            } 
            catch (AmbiguousMatchException ex)
            {
                throw new ArgumentException($"Property {type.FullName}.{name} is ambiguous", nameof(name), ex);
            }
        }

        public static MethodInfo GetMethodOrThrow(this Type type, string methodName, BindingFlags bindingFlags, Type[] arguments = null)
        {
            var result = type.GetMethod(methodName, bindingFlags, null, arguments ?? new Type[0], null);
            if (result == null)
            {
                var methodSignature = type.FullName + "." + methodName + "(";
                if (arguments != null)
                {
                    methodSignature += string.Join(", ", arguments.Select(x => x.FullName).ToArray());
                }
                methodSignature += ")";

                throw new System.ArgumentOutOfRangeException("Method " + methodSignature + " not found");
            }
            return result;
        }

        public static void CreateMethodDelegateOrThrow<T>(this Type type, string methodName, BindingFlags bindingFlags, out T result)
        {
            result = CreateMethodDelegateOrThrow<T>(type, methodName, bindingFlags);
        }



        public static T CreateMethodDelegateOrThrow<T>(this Type type, string methodName, BindingFlags bindingFlags)
        {
            var matches = type
                .GetMethods(bindingFlags)
                .Where(x => x.Name == methodName)
                .Where(x => IsMatching(x, typeof(T)))
                .ToList();

            if (matches.Count == 0)
            {
                throw new System.ArgumentOutOfRangeException("Method " + methodName + " of " + type + " not found for delegate type " + typeof(T));
            }
            else if ( matches.Count > 1 )
            {
                throw new System.ArgumentOutOfRangeException("More than one match for method " + methodName + " of " + type + " not found for delegate type " + typeof(T));
            }

            return (T)(object)Delegate.CreateDelegate(typeof(T), matches[0]);
        }

        public static T CreateMethodDelegate<T>(this Type type, string methodName, BindingFlags bindingFlags) where T : class
        {
            var matches = type
                .GetMethods(bindingFlags)
                .Where(x => x.Name == methodName)
                .Where(x => IsMatching(x, typeof(T)))
                .ToList();

            if ( matches.Count == 0 )
            {
                return null;
            }
            else if ( matches.Count > 1 )
            {
                return null;
            }

            return (T)(object)Delegate.CreateDelegate(typeof(T), matches[0]);
        }

        public static void CreateMethodDelegate<T>(this Type type, string methodName, BindingFlags bindingFlags, out T result) where T: class
        {
            result = CreateMethodDelegate<T>(type, methodName, bindingFlags);
        }


        private static bool IsMatching(MethodInfo mi, Type delegateType)
        {
            var targetParameters = delegateType.GetMethod("Invoke").GetParameters();
            var parameters = mi.GetParameters();

            if (parameters.Length != targetParameters.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].ParameterType != targetParameters[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
