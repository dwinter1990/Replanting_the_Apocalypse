using UnityEngine;

public class GroundCollision : MonoBehaviour
{
    [SerializeField] float moundMultiplier = 15f;
    public void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) return;

        // Find exact ground point
        RaycastHit hit;
        Vector3 spawnPoint = transform.position;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
            spawnPoint = hit.point;

        // Get the currently selected plant from the manager (only once!)
        GameObject plant = PlantPoolManager.PlantPoolManagerInstance.GetRandomPlant(PlantPoolManager.PlantPoolManagerInstance.selectedType);
        if (plant == null) return;

        // Spawn plant
        plant.transform.position = spawnPoint;
        plant.transform.rotation = Quaternion.Euler(90f,Random.Range(0,359),0f);

        // Spawn mound at same position
        GameObject mound = MoundPool.instance.Get();
        mound.transform.position = spawnPoint;
        mound.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        // Set mound scale based on the plant's growth profile
        Growing growingScript = plant.GetComponent<Growing>();
        if (growingScript != null && growingScript.profile != null)
        {
            float maxScale = growingScript.profile.maxScale;
            mound.transform.localScale = Vector3.one * maxScale * moundMultiplier;

            // Assign mound to the plant so it can be returned later
            growingScript.SetSpawnedMound(mound);
        }

        // Return seed to pool
        SeedManager.SMInstance.pool.ReturnObject(this.gameObject);
    }
}