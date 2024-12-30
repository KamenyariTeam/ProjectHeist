using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Characters.AI
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AIStateWrapper))]
    public class AIStateWrapperDrawer : PropertyDrawer
    {
        private static List<Type> _stateTypes;
        private static bool _requiresInitialization = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start the property
            EditorGUI.BeginProperty(position, label, property);

            if (_requiresInitialization)
            {
                _stateTypes = GetAIStateTypes();
                _requiresInitialization = false;

                EditorApplication.projectChanged -= RequestInitialization;
                EditorApplication.projectChanged += RequestInitialization;

            }

            // Get the 'StateLogic' field
            var stateLogicProperty = property.FindPropertyRelative(nameof(AIStateWrapper.StateLogic));

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);


            if (stateLogicProperty.managedReferenceValue == null)
            {
                if (GUI.Button(dropdownRect, "Select State Type") && _stateTypes.Count > 0)
                {
                    // Show a dropdown menu to select the state type
                    GenericMenu menu = new();
                    foreach (var type in _stateTypes)
                    {
                        menu.AddItem(new GUIContent(type.Name), false, () =>
                        {
                            stateLogicProperty.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.ShowAsContext();
                }
            }
            else
            {
                // Display the selected state's type
                EditorGUI.LabelField(dropdownRect, stateLogicProperty.managedReferenceValue.GetType().Name);

                // Draw the fields of the selected state using a property field
                EditorGUI.PropertyField(contentRect, stateLogicProperty, true);

                // Allow the user to clear the current selection
                if (GUI.Button(new Rect(position.x + position.width - 100, position.y, 90, EditorGUIUtility.singleLineHeight), "Clear State"))
                {
                    stateLogicProperty.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // End the property
            EditorGUI.EndProperty();
        }

        // Override GetPropertyHeight to account for dynamic height of the property
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty stateLogicProperty = property.FindPropertyRelative(nameof(AIStateWrapper.StateLogic));

            // Calculate the height: one line for the dropdown + additional height for state properties if selected
            float height = EditorGUIUtility.singleLineHeight + 4;

            if (stateLogicProperty.managedReferenceValue != null)
            {
                // Include the height for the serialized properties of the current state
                height += EditorGUI.GetPropertyHeight(stateLogicProperty, true);
            }

            return height;
        }

        private static List<Type> GetAIStateTypes()
        {
            Type interfaceType = typeof(IAIStateLogic);
            return interfaceType.Assembly
                .GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
        }

        private static void RequestInitialization()
        {
            _requiresInitialization = true;
        }
    }
#endif
}