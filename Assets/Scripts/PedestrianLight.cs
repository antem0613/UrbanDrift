using UnityEngine;
using UrbanDrift;
using System.Collections.Generic;

public class PedestrianLight : MonoBehaviour
{
    public Material M_Day, M_Night;
    public bool IsRed;
    bool nightUpdated = false;
    [SerializeField]
    MeshRenderer meshRenderer;
    List<Material> materials = new List<Material> ();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.timeState == TimeState.Night && !nightUpdated)
        {
            materials.Clear ();
            materials.AddRange(meshRenderer.materials);

            if (IsRed)
            {
                materials[1] = M_Night;
            }
            else
            {
                materials[2] = M_Night;
            }

            meshRenderer.SetMaterials(materials);
            Debug.Log("Turn Night");
            nightUpdated = true;
        }

        if (GameManager.instance.timeState == TimeState.Day && nightUpdated)
        {
            materials.Clear();
            materials.AddRange(meshRenderer.materials);

            if (IsRed)
            {
                materials[1] = M_Day;
            }
            else
            {
                materials[2] = M_Day;
            }

            meshRenderer.SetMaterials(materials);
            Debug.Log("Turn Day");
            nightUpdated = false;
        }
    }
}
