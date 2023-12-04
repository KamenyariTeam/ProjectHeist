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

    private Animator _animator;

    private bool _isOpened;
    private float _timeOpened;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _interactableArea.OnInteractEvent += ChangeState;

        _isOpened = true;
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
        _animator.SetBool(IsOpen, _isOpened);
        _timeOpened = 0.0f;
    }

}
