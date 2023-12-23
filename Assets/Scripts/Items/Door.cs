using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private static readonly int IsOpen = Animator.StringToHash("IsOpen");

    [SerializeField]
    private Interactable _interactableArea;

    [SerializeField]
    private float _maxTimeOpened;

    [SerializeField]
    private List<Animator> _doorAnimators;

    private bool _isOpened;
    private float _timeOpened;
    private void Start()
    {
        _interactableArea.OnInteractEvent += ChangeState;

        _isOpened = false;
        _timeOpened = 0.0f;
    }

    private void Update()
    {
        if (_isOpened)
        {
            _timeOpened += Time.deltaTime;
            if (_timeOpened > _maxTimeOpened)
            {
                ChangeState();
            }
        }
    }

    private void OnDestroy()
    {
        _interactableArea.OnInteractEvent -= ChangeState;
    }

    public void ChangeState()
    {
        _isOpened = !_isOpened;
        foreach (Animator animator in _doorAnimators)
        {
            animator.SetBool(IsOpen, _isOpened);
        }
        _timeOpened = 0.0f;
    }

}
