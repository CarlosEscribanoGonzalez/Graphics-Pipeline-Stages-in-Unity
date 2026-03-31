using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    [SerializeField] private Material surfaceSkybox;
    [SerializeField] private Material underWaterSkybox;
    [SerializeField] private GameObject waterHorizon;

    private void OnDestroy()
    {
        SetSkybox(SkyboxType.Surface);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.position.y < transform.position.y)
            SetSkybox(SkyboxType.Surface);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.position.y < transform.position.y)
            SetSkybox(SkyboxType.Underwater);
    }

    private void SetSkybox(SkyboxType type)
    {
        RenderSettings.skybox = type == SkyboxType.Surface ? surfaceSkybox : underWaterSkybox;
        waterHorizon.SetActive(type == SkyboxType.Underwater);
    }

    private enum SkyboxType
    {
        Surface,
        Underwater
    }
}