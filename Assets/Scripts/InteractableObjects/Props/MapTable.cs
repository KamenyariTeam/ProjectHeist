using GameControllers;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace InteractableObjects
{
    public class MapTable : OutlinedInteractable
    {
        public MissionMap MapUI;
        private GamePauseController _pauseController;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            GameObject Manager = GameObject.FindGameObjectWithTag("ManagersOwner");
            if (Manager)
            {
                _pauseController = Manager.GetComponent<GamePauseController>();
            }       
        }

        public override void Interact(GameObject character)
        {
            if (MapUI)
            {
                _pauseController.
                MapUI.SetVisible(true);
            }
            
        }
    }
}
