using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapShadowCaster2D : MonoBehaviour
{
    [SerializeField]
    protected PolygonCollider2D m_TilemapCollider;

    [SerializeField]
    protected bool m_SelfShadows = true;

    protected virtual void Awake()
    {
        ShadowCaster2DGenerator.GenerateTilemapShadowCasters(m_TilemapCollider, m_SelfShadows);
    }

    protected virtual void Reset()
    {
        m_TilemapCollider = GetComponent<PolygonCollider2D>();
    }
}