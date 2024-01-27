using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Character;
using UnityEngine.Windows;
using static UnityEditor.Progress;
using InteractableObjects;

namespace SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private InputReader _input;

        protected ISavableComponent[] GetOrderedSavableComponents()
        {
            MonoBehaviour[] Components = GameObject.FindObjectsOfType<MonoBehaviour>();

            return Components
                .Where(c => c is ISavableComponent)
                .Select(c => (ISavableComponent)c)
                .OrderBy(c => c.executionOrder)
                .ToArray(); 
        }

        private void Start()
        {
            _input = ScriptableObject.CreateInstance<InputReader>();

            // Setup inputs
            _input.SaveGameEvent += SaveGame;
            _input.LoadGameEvent += LoadGame;
        }

        public void SaveGame()
        {
            Save("Assets/Saves/", "save", ".dat");
            Debug.Log("Game saved");
        }

        public void LoadGame()
        {
            Load("Assets/Saves/", "save", ".dat");
            Debug.Log("Game loaded");
        }

        public virtual void Save(string folderPath, string fileName, string fileFormat)
        {
            if (!folderPath.EndsWith("/"))
            {
                folderPath += "/";
            }

            if (!fileFormat.StartsWith("."))
            {
                fileFormat = "." + fileFormat;
            }

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            Dictionary<int, ComponentData> componentsData = new Dictionary<int, ComponentData>();
            
            ISavableComponent[] SavableArray = GetOrderedSavableComponents();
            
            foreach (var savableComponent in SavableArray)
            {
                componentsData.Add(savableComponent.uniqueID, savableComponent.Serialize());
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(folderPath + fileName + fileFormat, FileMode.Create))
            {
                formatter.Serialize(stream, componentsData);
            }
        }

        public virtual void Load(string folderPath, string fileName, string fileFormat)
        {
            if (!folderPath.EndsWith("/"))
            {
                folderPath += "/";
            }

            if (!fileFormat.StartsWith("."))
            {
                fileFormat = "." + fileFormat;
            }

            if (!System.IO.Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("SaveManager::Directory '" + folderPath + "' not found");
            }    

            Dictionary<int, ComponentData> componentsData = null;

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(folderPath + fileName + fileFormat, FileMode.Open))
            {
                componentsData = (Dictionary<int, ComponentData>) formatter.Deserialize(stream);
            }

            foreach (var savableComponent in GetOrderedSavableComponents())
            {
                if (componentsData.ContainsKey(savableComponent.uniqueID))
                {
                   savableComponent.Deserialize(componentsData[savableComponent.uniqueID]);
                }
            }
        }
    }
}
