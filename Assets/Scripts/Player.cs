using Dreamteck.Forever;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UrbanDrift
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private bool RandomLane = false;

        private InputAction IA_Move;

        [SerializeField]
        Runner runner;

        public GameObject Arrow;
        Animator Anim_Arrow, Anim_Player;

        public float speed;

        int acceptedDir;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            IA_Move = InputSystem.actions.FindAction("Move");
            Anim_Arrow=Arrow.GetComponent<Animator>();
            Arrow.SetActive(false);
            Anim_Player = gameObject.GetComponent<Animator>();

            GameManager.instance.OnCrash += Crash;
            GameManager.instance.OnReset += ResetAnim;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.instance.isPaused)
            {
                runner.followSpeed = 0;
            }

            if (IA_Move.IsPressed() && !RandomLane && GameManager.instance.IsSliderActivated && !GameManager.instance.isPaused)
            {
                if (GameManager.instance.limit > 0.009 && !GameManager.instance.HasInput)
                {
                    Vector2 input = IA_Move.ReadValue<Vector2>();

                    if (Vector2.Dot(input, Vector2.right) >= 0.98)
                    {
                        Debug.Log("Changing Lane to Right");
                        GameManager.instance.levelGenerator.ChangeLane(1);
                        GameManager.instance.HasInput = true;
                        Arrow.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
                        acceptedDir = (int)Direction.Right;
                        StartCoroutine(InputArrow());
                    }
                    else if (Vector2.Dot(input, Vector2.left) >= 0.98)
                    {
                        Debug.Log("Changing Lane to Left");
                        GameManager.instance.levelGenerator.ChangeLane(2);
                        GameManager.instance.HasInput = true;
                        Arrow.transform.rotation = Quaternion.Euler(60f, 0f, 180f);
                        acceptedDir = (int)Direction.Left;
                        StartCoroutine(InputArrow());
                    }
                    else if(Vector2.Dot(input,Vector2.up) >= 0.98)
                    {
                        Debug.Log("Changing Lane to Forward");
                        GameManager.instance.levelGenerator.ChangeLane(0);
                        GameManager.instance.HasInput = true;
                        Arrow.transform.rotation = Quaternion.Euler(60f, 0f, 90f);
                        acceptedDir = (int)Direction.Forward;
                        StartCoroutine(InputArrow());
                    }
                }
            }

            if (GameManager.instance.IsGaming && !GameManager.instance.isPaused)
            {
                if (!GameManager.instance.IsGameOver)
                {
                    if (runner.followSpeed != 20)
                    {
                        runner.followSpeed = 20;
                    }

                    if (GameManager.instance.endTutorial)
                    {
                        GameManager.instance.score += runner.followSpeed * Time.deltaTime / 5f;
                    }
                }
                else if (runner.followSpeed > 0)
                {
                    runner.followSpeed -= 5 * Time.deltaTime;
                }
                else if (runner.followSpeed < 0)
                {
                    runner.followSpeed = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "ExitVolume")
            {
                if (RandomLane)
                {
                    Debug.Log("Change Lane Random");
                    int lane = UnityEngine.Random.Range(0, 3);
                    GameManager.instance.levelGenerator.ChangeLane(lane);
                }
                else if (!GameManager.instance.HasInput)
                {
                    Debug.Log("Set Lane Default");
                    GameManager.instance.levelGenerator.ChangeLane(0);
                }

                GameManager.instance.IsSliderActivated = false;

                StartCoroutine(Drift());
            }

            if(other.gameObject.tag == "TrafficVolume")
            {
                GameManager.instance.UpdateRoute();
            }
        }

        IEnumerator InputArrow()
        {
            GameManager.instance.IsSliderActivated = false;
            Arrow.SetActive(true);
            Anim_Arrow.SetTrigger("OnInput");

            while(Anim_Arrow.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                yield return null;
            }

            Arrow.SetActive(false);
        }

        IEnumerator Drift()
        {
            Anim_Player.SetInteger("Direction", acceptedDir);
            runner.isDrifting = false;

            if (acceptedDir == (int)Direction.Forward)
            {
                GameManager.instance.SE.clip = GameManager.instance.SE_Accel;
                GameManager.instance.SE.Play();
            }
            else
            {
                while (Anim_Player.GetCurrentAnimatorStateInfo(0).IsName("Drive"))
                {
                    yield return null;
                }

                GameManager.instance.SE.clip = GameManager.instance.SE_Drift;
                GameManager.instance.SE.time = 0.2f;
                GameManager.instance.SE.Play();

                if (!GameManager.instance.IsGameOver)
                {
                    while (Anim_Player.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                    {
                        yield return null;
                    }

                    acceptedDir = 1;
                    Anim_Player.SetInteger("Direction", 1);
                }
            }
        }

        public void Crash(object obj,EventArgs e)
        {
            Anim_Player.SetBool("IsCrashed", true);
            GameManager.instance.CarSound.Stop();
            GameManager.instance.SE.clip = GameManager.instance.SE_Crash;
            GameManager.instance.SE.time = 0f;
            GameManager.instance.SE.Play();
        }

        public void ResetAnim(object obj,EventArgs e)
        {
            Anim_Player.SetBool("IsCrashed", false);
        }
    }
}
