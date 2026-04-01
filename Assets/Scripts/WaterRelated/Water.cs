using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class Water : MonoBehaviour
{
    [SerializeField] private float maxWaterSpeed = 0.1f;
    [SerializeField] private Vector2 changeInterval = new(10, 20);
    [SerializeField] private float interpolationSpeed = 1.0f;
    private Material waterMat;
    private Vector2 currentSpeed = Vector2.zero;
    private Vector2 Offset {
        get { return waterMat.GetTextureOffset("_Displacement"); }
        set { waterMat.SetTextureOffset("_Displacement", value); }
    }

    private void OnEnable()
    {
        waterMat = GetComponent<MeshRenderer>().sharedMaterial;
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        //Para que el agua se vea siempre y no la oculte el frustum culling:
        float scale = GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Scale") * 100;
        mesh.bounds = new(Vector3.zero, new(scale, 1, scale));
        StartCoroutine(WaterSpeedCorroutine());
    }

    private void Update() => Offset += Time.deltaTime * currentSpeed;

    IEnumerator WaterSpeedCorroutine()
    {
        Vector2 speedRange = new(-maxWaterSpeed, maxWaterSpeed);
        while (true)
        {
            //Velocidad aleatoria:
            Vector2 targetSpeed = new(Utils.RandomInRange(speedRange), 
                Utils.RandomInRange(speedRange));
            //Interpolación:
            while(currentSpeed != targetSpeed)
            {
                currentSpeed = Vector2.MoveTowards(currentSpeed, targetSpeed, 
                    interpolationSpeed * Time.deltaTime);
                yield return null;
            }
            //Una vez alcanzada, la velocidad se mantiene por un intervalo aleatorio
            yield return new WaitForSeconds(Utils.RandomInRange(changeInterval));
        }
    }
}
