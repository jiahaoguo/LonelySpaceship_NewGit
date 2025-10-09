using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingBlock : MonoBehaviour
{
    [Header("Ground detection")]
    [Tooltip("Layers considered ground (include the 'Ground' layer here).")]
    public LayerMask groundMask;

    [Header("Landing Settings")]
    public float ignoreGroundTime = 0.5f; // seconds to ignore ground after spawn

    private List<Collider> myCols = new List<Collider>();
    private List<Collider> playerCols = new List<Collider>();
    private bool hasLanded;
    private float spawnTime;

    private void Awake()
    {
        GetComponentsInChildren(true, myCols);

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            var cols = p.GetComponentsInChildren<Collider>();
            playerCols.AddRange(cols);
        }
    }

    private void Start()
    {
        spawnTime = Time.time;

        // Ignore collisions with players while falling
        foreach (var blockCol in myCols)
        {
            foreach (var playerCol in playerCols)
            {
                if (blockCol && playerCol)
                    Physics.IgnoreCollision(blockCol, playerCol, true);
            }
        }

        // Freeze X/Z motion
        var rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (hasLanded) return;

        // Still in ignore time? -> skip ground collisions
        if (Time.time - spawnTime < ignoreGroundTime)
        {
            if ((groundMask.value & (1 << c.gameObject.layer)) != 0)
                return; // ignore ground
        }

        bool hitGround = (groundMask.value & (1 << c.gameObject.layer)) != 0;
        bool hitOtherBlock = c.gameObject.GetComponent<FallingBlock>() != null;

        if (hitGround || hitOtherBlock)
        {
            hasLanded = true;
            Debug.Log($"FallingBlock landed on {c.gameObject.name}");

            // Restore collisions with players
            foreach (var blockCol in myCols)
            {
                foreach (var playerCol in playerCols)
                {
                    if (blockCol && playerCol)
                        Physics.IgnoreCollision(blockCol, playerCol, false);
                }
            }

            // Freeze in place
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }
}
