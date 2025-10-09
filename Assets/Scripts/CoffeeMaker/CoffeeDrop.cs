using System.Collections;
using UnityEngine;

public enum CoffeeDropState
{
    Safe,
    Harmful
}

public class CoffeeDrop : MonoBehaviour
{
    [Header("Renderer")]
    public MeshRenderer meshRenderer;  // assign in Inspector

    [Header("Materials")]
    public Material safeMaterial;
    public Material harmfulMaterial;

    [Header("Flash Settings")]
    public float flashDuration = 2f;   // time to flash before harmful
    public float flashSpeed = 0.2f;    // blink speed

    private CoffeeDropState currentState = CoffeeDropState.Safe;
    private Coroutine flashRoutine;

    private void Start()
    {
        if (meshRenderer != null)
            meshRenderer.material = safeMaterial;
    }

    public void SetState(CoffeeDropState newState)
    {
        if (currentState == newState) return;

        if (newState == CoffeeDropState.Harmful)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashThenHarmful());
        }
        else
        {
            currentState = newState;
            UpdateVisual();
        }
    }

    private IEnumerator FlashThenHarmful()
    {
        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < flashDuration)
        {
            toggle = !toggle;
            if (meshRenderer != null)
                meshRenderer.material = toggle ? safeMaterial : harmfulMaterial;

            yield return new WaitForSeconds(flashSpeed);
            elapsed += flashSpeed;
        }

        currentState = CoffeeDropState.Harmful;
        UpdateVisual();
        flashRoutine = null;
    }

    private void UpdateVisual()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = currentState == CoffeeDropState.Safe
                ? safeMaterial
                : harmfulMaterial;
        }
    }
    public CoffeeDropState GetCurrentState()
    {
        return currentState;
    }


}
