using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


// defining namespace to copy its name without strings
namespace DataStorage.Generated
{
}

namespace DataStorage
{
    public class DataTableIDGeneratorWindow : EditorWindow
    {
        private List<DataTable> _dataTables = new List<DataTable>();
        private string _className = "";
        private Vector2 _scrollPos;

        [MenuItem("Tools/Data Table IDs Generator")]
        public static void ShowWindow()
        {
            _ = GetWindow<DataTableIDGeneratorWindow>("Data Tables ID Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("ID Generator", EditorStyles.boldLabel);

            // Display and manage the list of EnumNameList objects
            EditorGUILayout.LabelField("Data Tables List:");
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            for (int i = 0; i < _dataTables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _dataTables[i] = EditorGUILayout.ObjectField(_dataTables[i], typeof(DataTable), false) as DataTable;
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _dataTables.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Data Table"))
            {
                _dataTables.Add(null);
            }

            _className = EditorGUILayout.TextField("Generated Class Name:", _className);

            // Generate Enum button
            if (GUILayout.Button("Generate IDs"))
            {
                GenerateIDs();
            }

        }

        private void GenerateIDs()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save IDs", _className + ".cs", "cs", "Please enter a file name to save the generated IDs to");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string identation = "    ";

            List<string> idsToGenerate = new List<string>();
            _dataTables.ForEach((table) => {
                foreach (TableRowBase row in table.Rows)
                {
                    idsToGenerate.Add(row.id);
                }
            });

            List<string> variableNames = idsToGenerate.Select(id => id.Replace(" ", "_")).ToList();
            
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"namespace {nameof(DataStorage)}.{nameof(Generated)}");
                writer.WriteLine("{");
                writer.WriteLine(identation + "[System.Serializable]");
                writer.WriteLine(identation + $"public class {_className}: {nameof(TableID)}");
                writer.WriteLine(identation + "{");

                string doubleIdentation = identation + identation;
                for (int i = 0; i < idsToGenerate.Count; i++)
                { 
                    writer.WriteLine(doubleIdentation + $"public static readonly {_className} {variableNames[i]} = new {_className}(\"{idsToGenerate[i]}\");");
                }

                writer.WriteLine(doubleIdentation + $"public {_className}(string id): base(id){{}}");

                writer.WriteLine(identation + "}");
                writer.WriteLine(identation + $"[UnityEditor.CustomPropertyDrawer(typeof({_className}))]");
                writer.WriteLine(identation + $"public class {_className}PropertyDrawer : TableIDProperyDrawer<{_className}> {{ }}");

                writer.WriteLine("}");

    }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "IDs generated successfully!", "OK");
        }
    }

    public class TableID
    {
        public static readonly TableID NONE = new("NONE");

        public string ID;

        public TableID(string id)
        {
            ID = id;
        }
    }

    public class TableIDProperyDrawer<T> : PropertyDrawer
        where T: TableID
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty idProperty = property.FindPropertyRelative(nameof(TableID.ID));

            EditorGUI.BeginProperty(position, label, property);

            List<string> ids = GetIDs();
            int selectedIndex = ids.FindIndex((id) => id == idProperty.stringValue);
            if (selectedIndex == -1)
            {
                selectedIndex = ids.FindIndex((id) => id == TableID.NONE.ID);
                idProperty.stringValue = TableID.NONE.ID;
            }

            string[] options = ids.ToArray();


            if (options.Length > 0)
            {
                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
                idProperty.stringValue = options[selectedIndex];
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "No values defined");
            }

            EditorGUI.EndProperty();
        }

        private static List<FieldInfo> GetIDFields<U>() where U: TableID
        {
            return typeof(U)
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(U)).ToList();
        }

        private static List<string> GetIDs()
        {
            List<FieldInfo> idFields = GetIDFields<T>();
            idFields.AddRange(GetIDFields<TableID>());
            return idFields
                      .Select(f => ((TableID)f.GetValue(null)).ID)
                      .ToList();
        }
    }

}
