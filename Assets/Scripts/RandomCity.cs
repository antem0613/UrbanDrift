using UnityEngine;
using System.Collections.Generic;

public class RandomCity : MonoBehaviour
{
    public Transform[] pos = new Transform[2];
    public List<GameObject> blocks = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(blocks[Random.Range(0, blocks.Count)], pos[0].position, pos[0].rotation * Quaternion.AngleAxis(90f * Random.Range(0, 4), pos[0].up),gameObject.transform);
        Instantiate(blocks[Random.Range(0, blocks.Count)], pos[1].position, pos[1].rotation * Quaternion.AngleAxis(90f * Random.Range(0, 4), pos[1].up), gameObject.transform);
    }
}
