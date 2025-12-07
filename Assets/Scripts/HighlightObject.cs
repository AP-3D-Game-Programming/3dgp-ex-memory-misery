using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    private Material originalMat;
    public Material highlightMaterial;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalMat = rend.material;
    }

    public void Highlight(bool state)
    {
        if (state)
            rend.material = highlightMaterial;
        else
            rend.material = originalMat;
    }
}
