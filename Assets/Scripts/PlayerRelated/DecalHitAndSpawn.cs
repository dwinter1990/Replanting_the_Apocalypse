using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

public class DecalHitAndSpawn : MonoBehaviour
{
    IObjectPool<DecalProjector> decalPool;

    [SerializeField] private Material decalMat;
    [SerializeField] private LayerMask decalLayers = -1;

    private Vector3 decalSize = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField] private float decalLifetime = 5f;
    [SerializeField] private ParticleSystem waterGunParticles;
    private readonly List<ParticleCollisionEvent> collisionEvents = new();

    [SerializeField] private int particleHitCount = 0;
    private void Start()
    {
        if (waterGunParticles == null)
            waterGunParticles = GetComponent<ParticleSystem>();

        decalPool = new ObjectPool<DecalProjector>(
            createFunc: () =>
            {
                GameObject go = new GameObject("DecalProjector");
                DecalProjector dp = go.AddComponent<DecalProjector>();
                dp.material = decalMat;
                dp.fadeFactor = 1f;
                dp.fadeScale = 0.95f;
                return dp;
            },
            actionOnGet: dp => dp.gameObject.SetActive(true),
            actionOnRelease: dp => dp.gameObject.SetActive(false),
            actionOnDestroy: dp => Destroy(dp.gameObject),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 20
        );
    }
    private void OnParticleCollision(GameObject other)
    {
        if ((decalLayers.value & (1 << other.layer)) == 0)
            return;

        int eventCount = ParticlePhysicsExtensions.GetCollisionEvents(
            waterGunParticles,
            other,
            collisionEvents
        );

        for (int i = 0; i < eventCount; i++)
        {
            particleHitCount++;
            if (particleHitCount % 50 == 0)
            { // Spawn decal every 5 hits to reduce clutter
                SpawnDecal(collisionEvents[i]);
                particleHitCount = 0;
            }
        }
    }
    public void SpawnDecal(ParticleCollisionEvent particleCollisionEvent)
    {
        DecalProjector projector = decalPool.Get();

        projector.transform.position = particleCollisionEvent.intersection + particleCollisionEvent.normal * 0.5f; // Slightly offset to prevent z-fighting

        Quaternion normalRotation = Quaternion.LookRotation(-particleCollisionEvent.normal, Vector3.up);

        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        projector.transform.rotation = normalRotation * randomRotation;

        projector.size = decalSize;

        StartCoroutine(FadeAndRelease(projector, decalLifetime));
    }


    IEnumerator FadeAndRelease(DecalProjector projector, float duration)
    {
        float time = 0f;
        float initialFade = projector.fadeFactor;
        while (time < duration)
        {
            if (projector == null)
                yield break; // In case the projector was destroyed

            time += Time.deltaTime;
            float t = time / duration;
            projector.fadeFactor = Mathf.Lerp(initialFade, 0f, t);
            yield return null;
        }

        if (projector != null)
        {
            projector.fadeFactor = initialFade; // Ensure it's fully faded
            decalPool.Release(projector);
        }
    }
}


