using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanDrift
{
    public class TrafficLightSystem : MonoBehaviour
    {
        [SerializeField]
        public List<TrafficLight> lamps = new List<TrafficLight>();
        List<LightState> lightStates = new List<LightState>();
        public bool turnRed;
        bool nightUpdated = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(GameManager.instance.timeState == TimeState.Night &&  !nightUpdated)
            {
                for(int i = 0; i < lamps.Count; i++)
                {
                    lamps[i].UpdateLight();
                }

                nightUpdated = true;
            }

            if(GameManager.instance.timeState == TimeState.Day && nightUpdated)
            {
                for (int i = 0;i < lightStates.Count; i++)
                {
                    lamps[i].UpdateLight();
                }

                nightUpdated = false;
            }
        }

        public void SetUp(List<LightState> states, bool red)
        {
            lightStates.Clear();
            lightStates.AddRange(states);

            for (int i = 0;i<lamps.Count;i++)
            {
                lamps[i].level = states[i].level;
                lamps[i].pattern = states[i].pattern;
                lamps[i].UpdateLight();
            }

            turnRed = red;
        }

        public void YtoR()
        {
            if (turnRed)
            {
                bool redTurned = false;

                for (int i = 0; i < lamps.Count; i++)
                {
                    if (lamps[i].level == TrafficLevel.Yellow)
                    {
                        lamps[i].pattern = TrafficState.Off;
                        lamps[i].UpdateLight();
                    }

                    if (lamps[i].level == TrafficLevel.Red)
                    {
                        if (redTurned)
                        {
                            lamps[i].pattern = TrafficState.Off;
                        }
                        else
                        {
                            lamps[i].pattern = TrafficState.Normal;
                            redTurned = true;
                        }

                        lamps[i].UpdateLight();
                    }
                }
            }
        }
    }

    public class LampLevel
    {
        public float weight;
        public TrafficLevel level;

        public LampLevel(float value, TrafficLevel level)
        {
            this.weight = value;
            this.level = level;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(LampLevel))
            {
                LampLevel temp = (LampLevel) obj;
                return this.level == temp.level;
            }
            else if(obj.GetType() == typeof(TrafficLevel))
            {
                return this.level == (TrafficLevel) obj;
            }
            return false;
        }

        public override int GetHashCode() {  return this.level.GetHashCode(); }

        public override string ToString()
        {
            return level + ":" + weight;
        }
    }
}

