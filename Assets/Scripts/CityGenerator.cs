using UnityEngine;
using System.Collections.Generic;
using UrbanDrift;

public class CityGenerator : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> buildings = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Quaternion rot = transform.rotation * Quaternion.AngleAxis(90,transform.up);

        if (Random.Range(0, 2) == 1)
        {
            rot = rot * Quaternion.AngleAxis(180, transform.up);
        }

        Instantiate(buildings[Random.Range(0, buildings.Count)],transform.position,rot,transform.parent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
