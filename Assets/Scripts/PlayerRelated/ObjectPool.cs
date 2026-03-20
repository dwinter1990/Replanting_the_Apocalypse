using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ObjectPool : MonoBehaviour
{
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private void Awake()
    {
        if(pool == null)
        {
            pool = new Queue<GameObject>();
        }
    }
    public GameObject GetObject()
    {
        if(pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab, shootPoint.position, Quaternion.identity, null);
    }

    public void ReturnObject(GameObject obj) 
    {
        var growing = obj.GetComponent<Growing>();
        if (growing != null)
        {
            obj.transform.localScale = growing.profile.startScale;
            obj.transform.localRotation = growing.startRotation;
        }
        else
        {
            obj.transform.localScale = Vector3.one * .25f;
            obj.transform.localRotation = Quaternion.identity;
        }
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
