using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    [SerializeField] private Color tintColor;
    private Color originalTintColor;

    private void Awake()
    {
        originalTintColor = RenderSettings.skybox.GetColor("_Tint");
    }

    private void OnDestroy()
    {
        SetTint(originalTintColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.position.y < transform.position.y)
            SetTint(originalTintColor);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.position.y < transform.position.y)
            SetTint(tintColor);
    }

    private void SetTint(Color tint)
    {
        RenderSettings.skybox.SetColor("_Tint", tint);
    }
}
