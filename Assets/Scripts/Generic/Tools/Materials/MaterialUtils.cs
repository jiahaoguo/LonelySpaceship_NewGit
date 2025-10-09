using UnityEngine;

public static class MaterialUtils
{
    /// <summary>
    /// Sets a float property on a material.
    /// </summary>
    /// <param name="mat">The material to edit.</param>
    /// <param name="property">The property name (e.g. "_FillAmount").</param>
    /// <param name="value">The float value to assign.</param>
    public static void SetMaterialFloat(Material mat, string property, float value)
    {
        if (mat == null)
        {
            Debug.LogWarning("Material is null. Cannot set property: " + property);
            return;
        }

        if (!mat.HasProperty(property))
        {
            Debug.LogWarning($"Material '{mat.name}' does not have property '{property}'");
            return;
        }

        mat.SetFloat(property, value);
    }

    /// <summary>
    /// Sets a Vector4 property on a material.
    /// </summary>
    public static void SetMaterialVector(Material mat, string property, Vector4 value)
    {
        if (mat == null || !mat.HasProperty(property)) return;
        mat.SetVector(property, value);
    }

    /// <summary>
    /// Sets a Color property on a material.
    /// </summary>
    public static void SetMaterialColor(Material mat, string property, Color value)
    {
        if (mat == null || !mat.HasProperty(property)) return;
        mat.SetColor(property, value);
    }
}
