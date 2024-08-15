    using UnityEditor;
    using UnityEngine;
using System;

namespace DataStorage
{

    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : Editor
    {
        private DataTable _dataTable;
        private SerializedProperty _data;
        private Vector2 _scrollPosition = Vector2.zero;
        private bool[] _foldouts;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_data, new GUIContent("Rows"), false);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
            EditorGUI.indentLevel++;
            for (int i = 0; i < _data.arraySize; i++)
            {
                var rowProperty = _data.GetArrayElementAtIndex(i);
                var row = rowProperty.objectReferenceValue as TableRowBase;
                if (row == null)
                {
                    continue;
                }

                _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i], $"ID: {row.id}, Type: {row.name}", true);
                if (_foldouts[i])
                {
                    DisplayRow(i);
                }
               
            }
            EditorGUI.indentLevel--;
            GUILayout.EndScrollView();

            if (GUILayout.Button("Add Row"))
            {
                ShowRowTypesMenu(AddRow);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _dataTable = (DataTable)target;
            _data = serializedObject.FindProperty(DataTable.RowsPropertyName);
            _foldouts = new bool[_data.arraySize];
        }

        private void DisplayRow(int index)
        {
            var rowProperty = _data.GetArrayElementAtIndex(index);
            var row = rowProperty.objectReferenceValue as TableRowBase;

            EditorGUI.indentLevel++;
            GUILayout.BeginVertical();
            SerializedObject rowSerializedObject = new SerializedObject(row);
            SerializedProperty rowPropertyIterator = rowSerializedObject.GetIterator();
            rowPropertyIterator.NextVisible(true);

            while (rowPropertyIterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(rowPropertyIterator, true);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Type"))
            {
                ShowRowTypesMenu((type) => ChangeRowType(index, type));
            }

            if (GUILayout.Button("Remove"))
            {
                RemoveRow(index);
            }
            GUILayout.EndHorizontal();

            rowSerializedObject.ApplyModifiedProperties();
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void ShowRowTypesMenu(Action<Type> onTypeSelected)
        {
            GenericMenu menu = new GenericMenu();

            var derivedTypes = typeof(TableRowBase).Assembly.GetTypes();
            foreach (var type in derivedTypes)
            {
                if (type.IsSubclassOf(typeof(TableRowBase)) && !type.IsAbstract)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () => onTypeSelected?.Invoke(type));
                }
            }

            menu.ShowAsContext();
        }

        private void AddRow(Type type)
        {
            TableRowBase newRow = CreateInstance(type) as TableRowBase;
            newRow.name = type.Name;

            AssetDatabase.AddObjectToAsset(newRow, _dataTable);
            AssetDatabase.SaveAssets();

            _data.InsertArrayElementAtIndex(_data.arraySize);
            _data.GetArrayElementAtIndex(_data.arraySize - 1).objectReferenceValue = newRow;

            Array.Resize(ref _foldouts, _data.arraySize);

            EditorUtility.SetDirty(_dataTable);
            serializedObject.ApplyModifiedProperties();
        }

        private void ChangeRowType(int index, Type type)
        {
            var rowProperty = _data.GetArrayElementAtIndex(index);
            var row = rowProperty.objectReferenceValue as TableRowBase;
            AssetDatabase.RemoveObjectFromAsset(row);

            TableRowBase newRow = CreateInstance(type) as TableRowBase;
            newRow.name = type.Name;
            AssetDatabase.AddObjectToAsset(newRow, _dataTable);
            rowProperty.objectReferenceValue = newRow;

            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(_dataTable);
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveRow(int index)
        {
            var rowProperty = _data.GetArrayElementAtIndex(index);
            var row = rowProperty.objectReferenceValue as TableRowBase;

            AssetDatabase.RemoveObjectFromAsset(row);
            AssetDatabase.SaveAssets();

            _data.DeleteArrayElementAtIndex(index);

            EditorUtility.SetDirty(_dataTable);
            serializedObject.ApplyModifiedProperties();
        }

    }

}