using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private InputReader _input;

        private void Start()
        {
            InitializeInput();
            
            LoadGame();
        }

        private void InitializeInput()
        {
            _input = ScriptableObject.CreateInstance<InputReader>();
            _input.SaveGameEvent += SaveGame;
            _input.LoadGameEvent += LoadGame;
        }

        private ISavableComponent[] GetOrderedSavableComponents()
        {
            ISavableComponent[] savableComponents = FindObjectsOfType<MonoBehaviour>()
                .OfType<ISavableComponent>()
                .ToArray();

            return savableComponents;
        }

        public void SaveGame()
        {
            string folderPath = "Assets/Saves/";
            string fileName = SceneManager.GetActiveScene().name + "_save";
            string fileFormat = ".dat";

            Save(folderPath, fileName, fileFormat);
            Debug.Log("Game saved");
        }

        public void LoadGame()
        {
            string folderPath = "Assets/Saves/";
            string fileName = SceneManager.GetActiveScene().name + "_save";
            string fileFormat = ".dat";

            Load(folderPath, fileName, fileFormat);
            Debug.Log("Game loaded");
        }

        public void Save(string folderPath, string fileName, string fileFormat)
        {
            string fullPath = PrepareFilePath(folderPath, fileName, fileFormat);
            EnsureDirectoryExists(folderPath);

            var componentsData = GetOrderedSavableComponents()
                .ToDictionary(savableComponent => savableComponent.GetHashCode(), savableComponent => savableComponent.Serialize());

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                formatter.Serialize(stream, componentsData);
            }
        }

        public void Load(string folderPath, string fileName, string fileFormat)
        {
            string fullPath = PrepareFilePath(folderPath, fileName, fileFormat);
            if (File.Exists(fullPath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Dictionary<int, ComponentData> componentsData;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    componentsData = (Dictionary<int, ComponentData>)formatter.Deserialize(stream);
                }

                ApplyLoadedData(componentsData);
            }
            else
            {
                //On first scene launch creates save file
                Save(folderPath, fileName, fileFormat);
            }
        }

        private string PrepareFilePath(string folderPath, string fileName, string fileFormat)
        {
            return Path.Combine(folderPath, fileName + EnsureFileFormatPrefix(fileFormat));
        }

        private string EnsureFileFormatPrefix(string fileFormat)
        {
            return fileFormat.StartsWith(".") ? fileFormat : "." + fileFormat;
        }

        private void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void ApplyLoadedData(Dictionary<int, ComponentData> componentsData)
        {
            foreach (var savableComponent in GetOrderedSavableComponents())
            {
                if (componentsData.TryGetValue(savableComponent.GetHashCode(), out var data))
                {
                    savableComponent.Deserialize(data);
                }
            }
        }
    }
}
