using UnityEngine;

public class GroundCollision : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            RaycastHit hit;
            Vector3 spawnPoint = transform.position;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                spawnPoint = hit.point;
            }

            Debug.Log(gameObject.name + " hit the " + collision.gameObject.name);
            GameObject plant = PlantPoolManager.PlantPoolManagerInstance.GetRandomPlant();

            Growing growingScript = plant.GetComponent<Growing>();

            plant.transform.position = hit.point;
            plant.transform.rotation = Quaternion.Euler(0f, Random.Range(0,359), 0f);

            GameObject mound = MoundPool.instance.Get();
            mound.transform.position = hit.point;
            mound.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            if (growingScript != null && growingScript != null)
            {
                float plantMaxScale = growingScript.profile.maxScale;
                mound.transform.localScale = Vector3.one * (plantMaxScale * 10);

                growingScript.SetSpawnedMound(mound);
            }
            SeedManager.SMInstance.pool.ReturnObject(this.gameObject);
        }
    }
}