using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DecalPool : MonoBehaviour
{

    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject decalPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private void Awake()
    {
        if (pool == null)
        {
            pool = new Queue<GameObject>();
        }
    }
    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(decalPrefab, spawnPoint.position, Quaternion.identity, null);
    }

    public void ReturnObject(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.None;
        rb.angularVelocity = Vector3.zero;
        //obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}