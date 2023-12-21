using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashScript : MonoBehaviour
{
    public void OnAnimationEndPlay()
    {
        Destroy(gameObject);
    }
}
