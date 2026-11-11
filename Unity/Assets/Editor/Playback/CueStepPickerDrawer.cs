using System;
using System.Collections.Generic;
using System.Linq;
using Squarely.Presentation;
using UnityEditor;
using UnityEngine;

namespace Squarely.Editor
{
    /// <summary>
    /// Draws a <see cref="CueStepPickerAttribute"/> field with a dropdown to pick its step type, then the step's settings.
    /// </summary>
    [CustomPropertyDrawer(typeof(CueStepPickerAttribute))]
    public sealed class CueStepPickerDrawer : PropertyDrawer
    {
        private static List<Type> s_stepTypes;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float pickerX = position.x + EditorGUIUtility.labelWidth + 2f;
            var pickerRect = new Rect(pickerX, position.y, position.xMax - pickerX, EditorGUIUtility.singleLineHeight);

            if (EditorGUI.DropdownButton(pickerRect, new GUIContent(TypeName(property.managedReferenceValue)), FocusType.Keyboard))
                ShowTypeMenu(property);

            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        private static void ShowTypeMenu(SerializedProperty property)
        {
            SerializedObject serializedObject = property.serializedObject;
            string path = property.propertyPath;
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () => SetStep(serializedObject, path, null));

            foreach (Type type in StepTypes())
            {
                bool current = property.managedReferenceValue != null && property.managedReferenceValue.GetType() == type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), current,
                    () => SetStep(serializedObject, path, Activator.CreateInstance(type)));
            }

            menu.ShowAsContext();
        }

        private static void SetStep(SerializedObject serializedObject, string path, object step)
        {
            serializedObject.Update();
            serializedObject.FindProperty(path).managedReferenceValue = step;
            serializedObject.ApplyModifiedProperties();
        }

        private static List<Type> StepTypes()
        {
            if (s_stepTypes != null)
                return s_stepTypes;

            // Test assemblies define fake steps; they are not for assets.
            s_stepTypes = TypeCache.GetTypesDerivedFrom<CueStep>()
                .Where(type => !type.IsAbstract && !type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null)
                .Where(type => !type.Assembly.GetName().Name.EndsWith(".Tests"))
                .OrderBy(type => type.Name)
                .ToList();

            return s_stepTypes;
        }

        private static string TypeName(object step)
        {
            return step == null ? "None (pick a step)" : ObjectNames.NicifyVariableName(step.GetType().Name);
        }
    }
}
