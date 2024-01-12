using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [RequireComponent(typeof(SaveMaster))]
    public class GameMaster : MonoBehaviour
    {
        public void SaveGame()
        {
            GetComponent<SaveMaster>().Save("Assets/Saves/", "save", ".dat");
            Debug.Log("Game saved");
        }

        public void LoadGame()
        {
            GetComponent<SaveMaster>().Load("Assets/Saves/", "save", ".dat");
            Debug.Log("Game loaded");
        }
    }
}
