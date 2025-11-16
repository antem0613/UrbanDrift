using System.Collections.Generic;
using UnityEngine;

namespace UrbanDrift
{
    public class TileObjectManager : MonoBehaviour
    {
        public TrafficLightSystem trafficLightSystem;
        public List<GameObject> joints;
        [HideInInspector]
        public List<Transform> SignJoints;

        [HideInInspector]
        public int index = -1;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateJoints()
        {
            SignJoints.Clear();

            foreach (var joint in joints)
            {
                SignJoints.Add(joint.transform);
            }
        }

        public void SetUp(TrafficArgs args)
        {
            index = args.index;

            if (trafficLightSystem != null)
            {
                trafficLightSystem.SetUp(args.states,args.turnRed);

                foreach(var sign in args.signs)
                {
                    if(sign.Key < 6)
                    {
                        Instantiate(sign.Value, joints[sign.Key].transform);
                    }
                }
            }
            else
            {
                foreach (var sign in args.signs)
                {
                    if(sign.Key > 5)
                    {
                        Instantiate(sign.Value,joints[sign.Key - 6].transform);
                    }
                }
            }
        }
    }
}
