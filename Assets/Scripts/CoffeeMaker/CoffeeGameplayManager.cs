using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoffeeGameplayManager : MonoBehaviour
{
    [Header("Layer Flip Settings")]
    public List<float> layerChangeIntervals = new List<float>() { 5f }; // sequence of intervals
    public float layerHeight = 1f;           // N: step height
    public GridMesh gridMesh;                // grid reference for base Y

    [Header("Game Time")]
    public float gameDuration = 60f;         // total duration in seconds
    public UnityEvent onGameStart;           // event when game starts
    public UnityEvent onGameEnd;             // event when game ends

    private float currentThreshold = 0f;     // how high is currently "converted"
    private List<CoffeeDrop> allDrops = new List<CoffeeDrop>();
    private int intervalIndex = 0;
    private bool isGameRunning = false;

    private void Start()
    {
        if (layerChangeIntervals.Count == 0)
            layerChangeIntervals.Add(5f); // default safeguard

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        // Start game
        isGameRunning = true;
        onGameStart?.Invoke();

        // Start flipping layers
        StartCoroutine(ConvertLayersOverTime());

        // Wait until game duration expires
        yield return new WaitForSeconds(gameDuration);

        // End game
        isGameRunning = false;
        onGameEnd?.Invoke();
    }

    public void RegisterDrop(CoffeeDrop drop)
    {
        allDrops.Add(drop);

        // Immediately harmful if it already sits under the current threshold
        if (IsBelowThreshold(drop))
            drop.SetState(CoffeeDropState.Harmful);
    }

    private IEnumerator ConvertLayersOverTime()
    {
        while (isGameRunning)
        {
            // Pick the current interval
            float waitTime = layerChangeIntervals[Mathf.Min(intervalIndex, layerChangeIntervals.Count - 1)];
            yield return new WaitForSeconds(waitTime);

            if (!isGameRunning)
                yield break;

            // Raise threshold
            currentThreshold += layerHeight;

            // Flip all drops at or below threshold
            for (int i = allDrops.Count - 1; i >= 0; i--)
            {
                CoffeeDrop drop = allDrops[i];
                if (drop == null)
                {
                    allDrops.RemoveAt(i);
                    continue;
                }

                if (IsBelowThreshold(drop))
                    drop.SetState(CoffeeDropState.Harmful);
            }

            intervalIndex++;
        }
    }

    private bool IsBelowThreshold(CoffeeDrop drop)
    {
        float gridBaseY = gridMesh != null ? gridMesh.transform.position.y : 0f;
        float relativeY = drop.transform.position.y - gridBaseY;
        return relativeY <= currentThreshold + 0.001f;
    }

    public bool IsGameRunning()
    {
        return isGameRunning;
    }
}
