using System.Collections;
using System.Collections.Generic;
using GameManagers;
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

            _pauseController = ManagersOwner.GetManager<GamePauseController>();
        }

        public override void Interact(GameObject character)
        {
            if (MapUI)
            {
                //_pauseController.
                MapUI.SetVisible(true);
            }
            
        }
    }
}
