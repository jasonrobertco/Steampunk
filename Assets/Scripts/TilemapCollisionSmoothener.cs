using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
[RequireComponent(typeof(TilemapCollider2D))]
public class TilemapCollisionSmoothener : MonoBehaviour
{
    [SerializeField] private float extrusionFactor = 0.02f;
    [SerializeField] private float vertexDistance = 0.02f;

    private void Awake()
    {
        EnsureSetup();
    }

    private void OnValidate()
    {
        EnsureSetup();
    }

    private void Reset()
    {
        EnsureSetup();
    }

    private void EnsureSetup()
    {
        TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            return;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        CompositeCollider2D compositeCollider = GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = gameObject.AddComponent<CompositeCollider2D>();
        }

        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        compositeCollider.vertexDistance = vertexDistance;

        tilemapCollider.extrusionFactor = extrusionFactor;
        EnableCompositeMerge(tilemapCollider);
    }

    private static void EnableCompositeMerge(TilemapCollider2D tilemapCollider)
    {
        PropertyInfo compositeOperationProperty = typeof(Collider2D).GetProperty("compositeOperation");
        if (compositeOperationProperty != null)
        {
            object mergeValue = Enum.Parse(compositeOperationProperty.PropertyType, "Merge");
            compositeOperationProperty.SetValue(tilemapCollider, mergeValue);
            return;
        }

        PropertyInfo usedByCompositeProperty = typeof(Collider2D).GetProperty("usedByComposite");
        if (usedByCompositeProperty != null)
        {
            usedByCompositeProperty.SetValue(tilemapCollider, true);
        }
    }
}
