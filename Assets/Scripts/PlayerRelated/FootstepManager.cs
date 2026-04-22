using System;
using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [SerializeField] private float stepDist = 1.0f;
    private Vector3 lastRegisteredPos;
    public event Action<Vector3> OnStepPerformed;
    private LayerMask groudLayer;
    private AudioManager audioManager;

    [SerializeField] private List <AudioClip> stepSounds;

    private void Awake()
    {
        GetComponent<PlayerMovement>().OnPositionUpdated += HandlePositionUpdate;
        lastRegisteredPos = transform.position;
        groudLayer = LayerMask.GetMask("GroundLayer");
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void OnDestroy()
    {
        GetComponent<PlayerMovement>().OnPositionUpdated -= HandlePositionUpdate;
    }

    private void HandlePositionUpdate(Vector3 newPos)
    {
        if (Vector3.Distance(lastRegisteredPos, newPos) >= stepDist)
        {
            lastRegisteredPos = newPos;
            OnStepPerformed?.Invoke(newPos);

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, groudLayer))
            {

                switch (hit.collider.tag) {
                    case ("Wood"):
                        audioManager.PlaySFX(stepSounds[0]);
                        break;
                    case ("Sand"):
                        audioManager.PlaySFX(stepSounds[1]);
                        break;
                    case ("Rock"):
                        audioManager.PlaySFX(stepSounds[2]);
                        break;
                    case ("Water"):
                        audioManager.PlaySFX(stepSounds[3]);
                        break;
                    case ("Grass"):
                        audioManager.PlaySFX(stepSounds[4]);
                        break;
                    case ("Metal"):
                        audioManager.PlaySFX(stepSounds[5]);
                        break;
                    default:
                        audioManager.PlaySFX(stepSounds[0]);
                        break;
                }               
                //Debug.Log(hit.collider.tag);
            }
        }
    }
}
