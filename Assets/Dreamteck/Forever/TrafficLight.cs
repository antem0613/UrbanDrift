using System.Collections.Generic;
using UnityEngine;

namespace UrbanDrift
{
    public class TrafficLight : MonoBehaviour
    {
        MeshRenderer meshRenderer;
        List<Material> materials;
        public TrafficLevel level;
        public TrafficState pattern;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            materials=new List<Material>() { GameManager.instance.M_TrafficBase,GameManager.instance.M_TrafficBase };
        }

        // Update is called once per frame
        void Update()
        {

        }

        public TrafficLight(TrafficLevel level, TrafficState pattern)
        {
            this.level = level;
            this.pattern = pattern;
        }

        public void UpdateLight()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            materials = new List<Material>() { GameManager.instance.M_TrafficBase, GameManager.instance.M_TrafficBase };
            materials[1] = GameManager.instance.M_Traffics[(int)GameManager.instance.timeState][(int)level + (int)pattern];
            meshRenderer.SetMaterials(materials);
        }
    }
}
