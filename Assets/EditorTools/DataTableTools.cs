using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;


// defining namespace to copy its name without strings
namespace DataStorage.Generated
{
}

namespace DataStorage
{
    public static class DataTableTools
    {

#if UNITY_EDITOR

        private static readonly string GENERATED_PATH = "Assets/Scripts/DataStorage/Generated";

        [UnityEditor.MenuItem("UpdateDataTableTemplates", menuItem = "Tools/Update DataTable templates")]
        public static void UpdateDataTableTemplates()
        {
            string path = $"{GENERATED_PATH}/DataTableTypes.cs";
            var derivedTypes = typeof(TableRowBase).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(TableRowBase)) && !t.IsAbstract);
            using (StreamWriter writer = new(path))
            {
                writer.WriteLine("using UnityEngine;\n");
                writer.WriteLine($"namespace {nameof(DataStorage)}.{nameof(Generated)}");
                writer.WriteLine("{");

                foreach (Type type in derivedTypes)
                {
                    Type genericTableClass = typeof(DataTable<>);
                    string genericTableTypeName = GetGenericTypeName(genericTableClass, type);
                    string createdTableClassName = $"{type.Name}Table";

                    writer.WriteLine($"\n    [CreateAssetMenu(fileName = \"{createdTableClassName}\", menuName = \"DataTables/{type.Name}\")]");
                    writer.WriteLine($"    public class {createdTableClassName}: {genericTableTypeName} {{ }}");

                    Type genericTableSetClass = typeof(DataTablesSet<>);
                    string genericTableSetTypeName = GetGenericTypeName(genericTableSetClass, type);
                    string createdTableSetClassName = $"{type.Name}TableSet";

                    writer.WriteLine($"\n    [CreateAssetMenu(fileName = \"{createdTableSetClassName}\", menuName = \"DataTablesSets/{type.Name}\")]");
                    writer.WriteLine($"    public class {createdTableSetClassName}: {genericTableSetTypeName} {{ }}");


                }

                writer.WriteLine("}");
            }

            AssetDatabase.Refresh();

        }

        [MenuItem("Assets/Generate table IDs", priority = 0)]
        public static void GenerateIDs()
        {
            string path = EditorUtility.SaveFilePanel(
                "Enter name for generated file and class:",
                GENERATED_PATH,
                Selection.activeObject.name,
                "cs"
                );

            if (path == "")
            {
                return;
            }

            IIdentifiable identifiable = Selection.activeObject as IIdentifiable;
            if (identifiable == null)
            {
                Debug.LogWarning("Selected asset is not IIdentifiable.");
                return;
            }
            string className = Path.GetFileNameWithoutExtension(path);
            string identation = "    ";

            List<TableID> idsToGenerate = identifiable.Identifiers.ToList();
            List<string> variableNames = idsToGenerate.Select(id => id.ToString().Replace(" ", "_")).ToList();

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"namespace {nameof(DataStorage)}.{nameof(Generated)}");
                writer.WriteLine("{");
                writer.WriteLine(identation + "[System.Serializable]");
                writer.WriteLine(identation + $"public class {className}: {nameof(TableID)}");
                writer.WriteLine(identation + "{");

                string doubleIdentation = identation + identation;
                for (int i = 0; i < idsToGenerate.Count; i++)
                {
                    writer.WriteLine(doubleIdentation + $"public static readonly {className} {variableNames[i]} = new {className}(\"{idsToGenerate[i]}\");");
                }

                writer.WriteLine(doubleIdentation + $"public {className}(string id): base(id){{}}");

                writer.WriteLine(identation + "}");
                writer.WriteLine(identation + $"[UnityEditor.CustomPropertyDrawer(typeof({className}))]");
                writer.WriteLine(identation + $"public class {className}PropertyDrawer : TableIDProperyDrawer<{className}> {{ }}");

                writer.WriteLine("}");

            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Generate table IDs", true)]
        public static bool IDGenerationTableValidation()
        {
            return Selection.activeObject is IIdentifiable;
        }

        private static string GetGenericTypeName(Type genericType, Type argType)
        {
            Type constructedType = genericType.MakeGenericType(argType);
            string typeName = constructedType.Name.Substring(0, constructedType.Name.IndexOf('`'));
            string genericArgs = string.Join(", ", constructedType.GetGenericArguments().Select(t => t.Name));
            return $"{typeName}<{genericArgs}>";
        }

#endif

    }

    public class TableIDProperyDrawer<T> : PropertyDrawer
    where T : TableID
    {

#if UNITY_EDITOR

        private static List<TableID> _ids;
        private static string[] _options;
        private static bool _requiresInitialization = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_requiresInitialization)
            {
                _ids = GetIDs();
                _options = _ids.Select(id => id.ToString()).ToArray();
                _requiresInitialization = false;

                EditorApplication.projectChanged -= RequestInitialization;
                EditorApplication.projectChanged += RequestInitialization;

                Debug.Log("Init");
            }

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty idProperty = property.FindPropertyRelative("_id");
            TableID currentID = new (idProperty.stringValue);

            int selectedIndex = _ids.FindIndex((id) => id == currentID);
            if (selectedIndex == -1)
            {
                selectedIndex = _ids.FindIndex((id) => id == TableID.NONE);
                idProperty.stringValue = TableID.NONE.ToString();
            }

            if (_options.Length > 0)
            {
                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, _options);
                idProperty.stringValue = _options[selectedIndex];
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "No values defined");
            }

            EditorGUI.EndProperty();
        }

        private static List<FieldInfo> GetIDFields<U>() where U : TableID
        {
            return typeof(U)
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(U)).ToList();
        }

        private static List<TableID> GetIDs()
        {
            List<FieldInfo> idFields = GetIDFields<T>();
            idFields.AddRange(GetIDFields<TableID>());
            return idFields
                      .Select(f => (TableID)f.GetValue(null))
                      .ToList();
        }

        private static void RequestInitialization()
        {
            _requiresInitialization = true;
        }
#endif
    }


}