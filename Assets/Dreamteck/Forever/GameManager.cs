using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Forever;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using Dreamteck.Utilities;
using UnityEngine.EventSystems;

namespace UrbanDrift
{
    [Flags]
    public enum Direction
    {
        None = 0,
        Forward = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Any = Forward | Left | Right 
    }

    public enum TrafficLevel { Green, Yellow = 5, Red = 10}
    public enum TrafficState { Normal, Forward, Right, Left, Off}
    public enum TimeState { Day, Night }

    public class GameManager : Singleton<GameManager>
    {
        public Slider Sld_InputLimit;
        [SerializeField]
        AnimationCurve SliderColor;
        [SerializeField]
        Image SliderFill;

        public float timeSpeed;
        public bool TimeGoing;
        public bool DontFail;
        bool _timeGoing;
        int argsIndex = 0;

        [SerializeField]
        Material M_Window;

        public Material M_TrafficBase;

        [SerializeField]
        List<Material> M_DayTraffics, M_NightTraffics;

        public List<List<Material>> M_Traffics;

        public int maxSignCount;

        [SerializeField]
        List<Sign> Signs;

        List<List<Sign>> allSigns;

        [HideInInspector]
        public TimeState timeState;

        public float windowIntensity;

        [HideInInspector]
        public float limit = 0;
        [HideInInspector]
        public float trafficIntensity;
        [HideInInspector]
        public LevelGenerator levelGenerator;
        [HideInInspector]
        public bool HasInput = false;
        [HideInInspector]
        public bool IsSliderActivated = false;
        [HideInInspector]
        public bool isPaused = false;
        [HideInInspector]
        public int highScore;

        GameObject Sun;

        public GameObject InitialCamera, GameCamera, Logo, StartButton, TitlePanel,OnGamingPanel,GameOverPanel, LoadingPanel,PausePanel, SettingCamera, SettingPanel;
        public GameObject[] TutorialPanels;

        public Toggle toggleSkip;
        public Slider sliderBGM,sliderSE;

        public GameObject defaultSelect;

        public EventSystem eventSystem;

        public AudioSource BGM, SE, CarSound;

        public AudioClip Au_Engine ,CityEnv, MainBGM,SE_Drift,SE_Accel,SE_Crash;

        [HideInInspector]
        public List<TrafficLightSystem> trafficLightSystems = new List<TrafficLightSystem>();
        [HideInInspector]
        List<Sign> ForwardSigns, ForwardLeftSigns, ForwardRightSigns, LeftSigns, RightSigns, LeftRightSigns, OnewaySigns, DummySigns;
        CanvasGroup LoadingCanvas;
        [HideInInspector]
        public bool IsInitialized = false;

        [HideInInspector]
        public bool IsGameOver,IsGaming,IsNight;
        [HideInInspector]
        public float score;
        [HideInInspector]
        public List<TrafficArgs> trafficArgs = new List<TrafficArgs>();
        [HideInInspector]
        public bool isBusy;
        [HideInInspector]
        public bool endTutorial;
        [HideInInspector]
        public int currentPage;

        public event EventHandler Morning;
        public event EventHandler Evening;
        public event EventHandler OnCrash;
        public event EventHandler OnReset;
        public event EventHandler EnablePlayerInput;
        public event EventHandler EnableUIInput;
        public event EventHandler OnEndTutorial;

        private void Start()
        {
            levelGenerator = GameObject.Find("Level Generator").GetComponent<LevelGenerator>();
            Sun = GameObject.Find("Sun");

            ForwardSigns = new List<Sign>();
            LeftSigns = new List<Sign>();
            RightSigns = new List<Sign>();
            ForwardLeftSigns = new List<Sign>();
            ForwardRightSigns = new List<Sign>();
            LeftRightSigns = new List<Sign>();
            OnewaySigns = new List<Sign>();
            DummySigns = new List<Sign>();


            //標識を進行可能な方向ごとに分類
            foreach (var sign in Signs)
            {
                if (sign.Oneway)
                {
                    OnewaySigns.Add(sign);
                }
                else
                {
                    switch (sign.constraint)
                    {
                        case Direction.Forward | Direction.Left:
                            ForwardLeftSigns.Add(sign);
                            break;
                        case Direction.Forward | Direction.Right:
                            ForwardRightSigns.Add(sign);
                            break;
                        case Direction.Left | Direction.Right:
                            LeftRightSigns.Add(sign);
                            break;
                        case Direction.Forward:
                            ForwardSigns.Add(sign);
                            break;
                        case Direction.Left:
                            LeftSigns.Add(sign);
                            break;
                        case Direction.Right:
                            RightSigns.Add(sign);
                            break;
                        case Direction.None:
                            DummySigns.Add(sign);
                            break;
                    }
                }
            }

            InitialCamera.SetActive(true);
            GameCamera.SetActive(false);
            SettingCamera.SetActive(false);
            TitlePanel.SetActive(false);
            OnGamingPanel.SetActive(false);
            GameOverPanel.SetActive(false);
            PausePanel.SetActive(false);
            SettingPanel.SetActive(false);
            TutorialPanels[0].SetActive(false);
            _timeGoing = false;

            LoadingCanvas = LoadingPanel.GetComponent<CanvasGroup>();

            DayNightControl();

            score = 0;
            IsGameOver = false;
            IsGaming = false;

            //昼と夜の信号マテリアルリストを作成
            M_Traffics = new List<List<Material>>() { M_DayTraffics, M_NightTraffics };

            BGM.clip = CityEnv;
            StartCoroutine(WhileLoading());
            TitlePanel.SetActive(true);
            eventSystem.SetSelectedGameObject(defaultSelect);
            EnableUIInput?.Invoke(this,EventArgs.Empty);
            IsInitialized = true;
        }

        //ロード中のアニメーションと遷移処理
        IEnumerator WhileLoading()
        {
            BGM.Stop();
            SE.Stop();
            CarSound.Stop();
            LoadingPanel.SetActive(true);
            LoadingCanvas.alpha = 1f;

            while (!levelGenerator.ready)
            {
                yield return null;
            }

            while(LoadingCanvas.alpha > 0)
            {
                LoadingCanvas.alpha -= 0.02f;
                yield return null;
            }

            LoadingPanel.SetActive(false);
            BGM.time = 0;
            BGM.Play();

            if (IsGaming)
            {
                CarSound.Play();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!levelGenerator.ready)
            {
                StartCoroutine(WhileLoading());
            }

            if (Sld_InputLimit.gameObject.activeSelf != IsSliderActivated)
            {
                Sld_InputLimit.gameObject.SetActive(IsSliderActivated);
            }
            Sld_InputLimit.value = limit;
            SliderFill.color = Color.Lerp(Color.red, Color.green, SliderColor.Evaluate(limit));

            if (_timeGoing)
            {
                DayNightControl();
            }
        }

        //ディレクショナルライトの回転で時間を定義
        //昼夜の切り替えと窓の光度調整
        void DayNightControl()
        {
            Sun.transform.Rotate(Time.deltaTime * timeSpeed, 0f, 0f);

            if (Sun.transform.rotation.eulerAngles.x >= 270f && (timeState == TimeState.Day || !IsInitialized))
            {
                M_Window.SetFloat("_EmissiveIntensity", windowIntensity);

                OnMorning();

                timeState = TimeState.Night;
            }

            if (Sun.transform.rotation.eulerAngles.x <= 90f && (timeState == TimeState.Night || !IsInitialized))
            {
                M_Window.SetFloat("_EmissiveIntensity", 0f);

                OnEvening();

                timeState = TimeState.Day;
            }
        }

        public void OnPause()
        {
            if (IsGaming)
            {
                if (isPaused)
                {
                    isPaused = false;
                    _timeGoing = true;
                    PausePanel.SetActive(false);
                    CarSound.UnPause();
                    BGM.UnPause();
                    EnablePlayerInput?.Invoke(this,EventArgs.Empty);
                }
                else
                {
                    isPaused = true;
                    _timeGoing = false;
                    PausePanel.SetActive(true);
                    CarSound.Pause();
                    BGM.Pause();
                    eventSystem.SetSelectedGameObject(defaultSelect);
                    EnableUIInput?.Invoke(this,EventArgs.Empty);
                }
            }
            else
            {
                Application.Quit();
            }
        }

        public void NextTutorial()
        {
            if (!endTutorial && IsGaming && !isPaused)
            {
                currentPage++;

                if (currentPage < TutorialPanels.Length)
                {
                    TutorialPanels[currentPage - 1].SetActive(false);
                    TutorialPanels[currentPage].SetActive(true);
                }
                else
                {
                    endTutorial = true;
                    TutorialPanels[0].SetActive(false);
                    OnEndTutorial?.Invoke(this, EventArgs.Empty);

                    if (TimeGoing)
                    {
                        _timeGoing = true;
                    }
                }
            }
        }

        public void PreviousTutorial()
        {
            if (!endTutorial && IsGaming && !isPaused)
            {
                if (currentPage > 1)
                {
                    TutorialPanels[currentPage].SetActive(false);
                    currentPage--;
                    TutorialPanels[currentPage].SetActive(true);
                }
            }
        }

        protected virtual void OnMorning()
        {
            Morning?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnEvening()
        {
            Evening?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateRoute()
        {
            if(trafficLightSystems.Count > 0)
            {
                trafficLightSystems[0].YtoR();
                trafficLightSystems.RemoveAt(0);
            }
        }

        //外部から標識と信号の状態を削除
        public void RemoveTrafficArg(int i)
        {
            trafficArgs.RemoveAt(i);
        }

        public TrafficArgs GetNextArg()
        {
            return trafficArgs[0];
        }

        public Direction GetCorrectWay()
        {
            return trafficArgs[0].allowed;
        }

        //新しい信号パターンと標識を生成
        public IEnumerator SetTraffic()
        {
            //他の処理が終わるまで待機
            while (isBusy)
            {
                yield return null;
            }

            isBusy = true;
            //パターンリストを初期化
            List<TrafficLevel> levelTable = new List<TrafficLevel>() { TrafficLevel.Green, TrafficLevel.Yellow, TrafficLevel.Red };
            List<float> levelWeights = new List<float>() { 0.525f, 0.15f, 0.325f };

            List<Direction> patternPool = new List<Direction>() { Direction.Any, Direction.Forward, Direction.Right, Direction.Left };
            List<int> OnewayIndex = new List<int>() { 0, 1 };
            List<int> IntersectionIndex = new List<int>() { 2, 3, 4, 5 };
            List<int> StreetIndex = new List<int>() { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

            TrafficArgs args = new TrafficArgs();
            args.states = new List<LightState>();
            args.signs = new Dictionary<int, GameObject>();

            TrafficLevel currentLevel = TrafficLevel.Green;
            Direction currentPattern = Direction.None;
            float rand;
            bool anyUsed = false;
            int prev = 0;

            /*信号パターンを生成
             * 緑→黄→赤の順で3パターン生成
             * 必ず1つは進行可能なパターンを含むようにする
             * 信号のランプごとにランプの色を決定、色は緑→黄→緑のように戻らない
             * これまでのパターンに応じて選択肢を減らす
             * 黄信号は進行不可になる可能性があるため進行可能なパターンの確保に数えない
             */

            for (int i = 0; i < 3; i++)
            {
                currentPattern = patternPool[UnityEngine.Random.Range(0, patternPool.Count)];

                if (currentPattern != Direction.None)
                {
                    patternPool.Remove(currentPattern);
                }

                if (currentPattern != Direction.Any)
                {
                    patternPool.Remove(Direction.Any);
                }
                else
                {
                    anyUsed = true;
                }

                if (i == 0)
                {
                    patternPool.Add(Direction.None);
                }

                if (currentLevel == TrafficLevel.Red)
                {
                    if (currentPattern == Direction.Any)
                    {
                        args.denied = currentPattern & ~args.allowed & ~args.neutral;
                    }
                    else
                    {
                        args.allowed &= ~currentPattern;
                        args.denied |= currentPattern;
                    }
                }
                else if(currentLevel == TrafficLevel.Yellow)
                {
                    args.neutral |= currentPattern;
                }
                else
                {
                    args.allowed |= currentPattern;
                }

                args.states.Add(new LightState(currentLevel, ConvertToState(currentPattern)));

                if (currentLevel != TrafficLevel.Red)
                {
                    prev = levelTable.IndexOf(currentLevel);

                    if ((currentPattern == Direction.None || currentPattern == Direction.Any))
                    {
                        levelWeights.RemoveAt(prev);
                        levelTable.RemoveAt(prev);
                    }

                    rand = levelWeights.Sum() * UnityEngine.Random.value;

                    for (int j = 0; j < levelTable.Count; j++)
                    {
                        if (rand <= levelWeights[j])
                        {
                            currentLevel = levelTable[j];

                            if (prev != j)
                            {
                                levelWeights.RemoveAt(prev);
                                levelTable.RemoveAt(prev);

                                if (!patternPool.Contains(Direction.Any) && !anyUsed)
                                {
                                    patternPool.Add(Direction.Any);
                                }
                            }

                            break;
                        }

                        rand -= levelWeights[j];
                    }
                }
                else if (currentPattern == Direction.Any || currentPattern == Direction.None)
                {
                    patternPool = new List<Direction> { Direction.None };
                }
            }

            Direction result = args.allowed | args.neutral;

            List<List<Sign>> signsList = new List<List<Sign>>() { DummySigns , ForwardSigns, LeftSigns, RightSigns, ForwardLeftSigns, ForwardRightSigns, LeftRightSigns};
            List<Sign> candidatedSigns = new List<Sign>();

            //信号のパターンから進行可能なルートを残すように標識リストを調整
            switch (args.allowed)
            {
                case Direction.Forward:
                    signsList.Remove(ForwardSigns);
                    signsList.Remove(ForwardRightSigns);
                    signsList.Remove(ForwardLeftSigns);
                    break;

                case Direction.Left:
                    signsList.Remove(LeftRightSigns);
                    signsList.Remove(LeftSigns);
                    signsList.Remove(ForwardLeftSigns);
                    break;

                case Direction.Right:
                    signsList.Remove(LeftRightSigns);
                    signsList.Remove(ForwardRightSigns);
                    signsList.Remove(RightSigns);
                    break;

                case (Direction.Forward | Direction.Left):
                    signsList.Remove(ForwardLeftSigns);

                    if (UnityEngine.Random.value > 0.5f)
                    {
                        signsList.Remove(ForwardSigns);
                    }
                    else
                    {
                        signsList.Remove(LeftSigns);
                    }
                    break;

                case (Direction.Forward | Direction.Right):
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        signsList.Remove(ForwardSigns);
                    }
                    else
                    {
                        signsList.Remove(RightSigns);
                    }

                    signsList.Remove(ForwardRightSigns);
                    break;

                case (Direction.Left | Direction.Right):
                    signsList.Remove(LeftRightSigns);

                    if (UnityEngine.Random.value > 0.5f)
                    {
                        signsList.Remove(LeftSigns);
                    }
                    else
                    {
                        signsList.Remove(RightSigns);
                    }
                    break;
            }

            //選定された標識リストを統合
            for(int i = 0; i<signsList.Count;i++)
            {
                candidatedSigns.AddRange(signsList[i]);
            }

            //標識リストをシャッフル
            candidatedSigns.Shuffle();

            //重みをリセット
            int arrowTrial = 0;
            float sum = 0, val;

            for (int i = 0; i < candidatedSigns.Count; i++)
            {
                sum += candidatedSigns[i].chance;
            }

            //標識を信号の進行可能なルートに応じて配置
            //最大数まで、または配置可能な場所がなくなるまで繰り返す
            //配置可能な場所がなくなった場合はループを抜ける
            //配置可能な場所がある場合はランダムに標識を選択
            //一方通行標識は左右に1つずつまで配置可能
            for (int signUsed = 0; signUsed<maxSignCount;)
            {   
                val = UnityEngine.Random.value * sum;

                for(int i = 0; i < candidatedSigns.Count; i++) 
                {
                    Sign sign = candidatedSigns[i];

                    if (val < sign.chance)
                    {
                        sum -= sign.chance;
                        candidatedSigns.RemoveAt(i);

                        if (OnewayIndex.Count != 0 && sign.Oneway && arrowTrial < 2)
                        {
                            bool isLeftArrow = sign == Signs[0];

                            int random = UnityEngine.Random.Range(0, OnewayIndex.Count);
                            args.signs.Add(OnewayIndex[random], sign.prefab);

                            bool isLeftSide = OnewayIndex[random] == 0;
                            Direction constraint = Direction.None;

                            if (isLeftSide && !isLeftArrow)
                                constraint = Direction.Left;
                            else if (!isLeftSide && isLeftArrow)
                                constraint = Direction.Right;

                            if (constraint != Direction.None)
                            {
                                Direction before = result;
                                result &= ~constraint;

                                if (result == Direction.None)
                                {
                                    result = before;
                                    break;
                                }

                                args.allowed &= ~constraint;
                                args.neutral &= ~constraint;
                            }

                            OnewayIndex.RemoveAt(random);
                            arrowTrial++;
                            signUsed++;
                        }
                        else
                        {
                            Direction constraint = sign.constraint;

                            Direction before = result;

                            if (constraint != Direction.None)
                            {
                                result &= ~constraint;

                                if (result == Direction.None)
                                {

                                    result = before;
                                    break;
                                }

                                args.allowed &= ~constraint;
                                args.neutral &= ~constraint;
                            }

                            int random;

                            if (sign.InIntersection)
                            {
                                random = UnityEngine.Random.Range(0, IntersectionIndex.Count);
                                args.signs.Add(IntersectionIndex[random], sign.prefab);
                                IntersectionIndex.RemoveAt(random);
                            }
                            else
                            {
                                random = UnityEngine.Random.Range(0, StreetIndex.Count);
                                args.signs.Add(StreetIndex[random], sign.prefab);
                                StreetIndex.RemoveAt(random);
                            }
                            signUsed++;
                        }

                        break;
                    }
                }
            }

            //黄信号が赤になるか決定
            //黄信号が正解ルートの場合は赤にならない
            if (args.neutral != Direction.None && (result & ~args.neutral) != Direction.None)
            {
                args.turnRed = UnityEngine.Random.value < 0.5f;
            }

            //赤になる場合は進行可能なルートから中立のルートを除外
            if (args.turnRed)
            {
                args.allowed = result & ~args.neutral;
            }
            else
            {
                args.allowed = result;
            }

            args.denied = ~args.allowed;

            args.index = argsIndex;
            argsIndex++;
            
            trafficArgs.Add(args);
            isBusy = false;
        }

        public void StartGame()
        {
            endTutorial = toggleSkip.isOn;
            SE.clip = Au_Engine;
            SE.time = 0.5f;
            SE.Play();
            BGM.Stop();
            BGM.clip = MainBGM;
            BGM.time = 0;
            BGM.Play();
            CarSound.Play();
            TitlePanel.SetActive(false);
            EnablePlayerInput?.Invoke(this,EventArgs.Empty);
            Invoke("OnStartGame", 0.5f);
        }

        void OnStartGame()
        {
            if (TimeGoing && endTutorial)
            {
                _timeGoing = true;
            }
            InitialCamera.SetActive(false);
            GameCamera.SetActive(true);
            OnGamingPanel.SetActive(true);
            score = 0;
            IsGaming = true;
            Invoke("HideLogo", 0.1f);

            Invoke("StartTutorial", 1.5f);
        }

        void StartTutorial()
        {
            levelGenerator.generateSegmentsAhead = 1;
            if (!endTutorial)
            {
                if (!TutorialPanels[0].activeSelf)
                {
                    TutorialPanels[0].SetActive(true);
                    currentPage = 1;

                    for (int i = 1; i < TutorialPanels.Length; i++)
                    {
                        TutorialPanels[i].SetActive(false);
                    }

                    TutorialPanels[1].SetActive(true);
                }
            }
            else
            {
                OnEndTutorial?.Invoke(this, EventArgs.Empty);
                levelGenerator.generateSegmentsAhead = 4;
            }
        }
        
        void HideLogo()
        {
            Logo.SetActive(false);
        }

        public void ResetGame()
        {
            IsGameOver = false;
            Sld_InputLimit.gameObject.SetActive(false);
            IsSliderActivated = false;
            score = 0;
            levelGenerator.Restart();

            OnReset?.Invoke(this,EventArgs.Empty);

            if (isPaused)
            {
                PausePanel.SetActive(false);
                isPaused = false;
            }

            GameOverPanel.SetActive(false);
            OnGamingPanel.SetActive(true);
            
        }

        public void BackToTitle()
        {
            endTutorial = toggleSkip.isOn;
            TutorialPanels[0].SetActive(false);
            IsGaming = false;
            IsGameOver=false;
            _timeGoing = false;
            BGM.clip = CityEnv;
            CarSound.Stop();
            levelGenerator.Restart();
            OnReset?.Invoke(this, EventArgs.Empty);
            GameCamera.SetActive(false);
            InitialCamera.SetActive(true);
            Logo.SetActive(true);

            if (isPaused)
            {
                PausePanel.SetActive(false);
                isPaused=false;
            }

            GameOverPanel.SetActive(false);
            OnGamingPanel.SetActive(false);
            TitlePanel.SetActive(true);
            eventSystem.SetSelectedGameObject(defaultSelect);
            EnableUIInput?.Invoke(this, EventArgs.Empty);
        }

        public void ClosePause()
        {
            isPaused = false;
            _timeGoing = true;
            PausePanel.SetActive(false);
            CarSound.UnPause();
            BGM.UnPause();
            EnablePlayerInput?.Invoke(this, EventArgs.Empty);
        }

        public void GameOver()
        {
            if (PlayerPrefs.GetInt("highScore") < Mathf.CeilToInt(score))
            {
                PlayerPrefs.SetInt("highScore", Mathf.CeilToInt(score));
                PlayerPrefs.Save();
            }
            highScore = PlayerPrefs.GetInt("highScore");
            OnCrash?.Invoke(this,EventArgs.Empty);
            OnGamingPanel.SetActive(false);
            GameOverPanel.SetActive(true);
            eventSystem?.SetSelectedGameObject(defaultSelect);
            EnableUIInput?.Invoke(this,EventArgs.Empty);
        }

        public void SetBGMVolume()
        {
            BGM.volume = sliderBGM.value * 0.036f;
            PlayerPrefs.SetFloat("BGM",sliderBGM.value);
            PlayerPrefs.Save();
        }

        public void SetSEVolume()
        {
            SE.volume = sliderSE.value / 10f;
            CarSound.volume = sliderSE.value + 0.025f;
            PlayerPrefs.SetFloat("SE",sliderSE.value);
            PlayerPrefs.Save();
        }

        TrafficState ConvertToState(Direction pattern)
        {
            switch (pattern)
            {
                case Direction.Any:
                    return TrafficState.Normal;
                case Direction.Forward:
                    return TrafficState.Forward;
                case Direction.Left:
                    return TrafficState.Left;
                case Direction.Right:
                    return TrafficState.Right;
                default:
                    return TrafficState.Off;
            }
        }
    }

    public class TrafficArgs
    {
        public Direction allowed, neutral,denied;
        public bool turnRed;
        public List<LightState> states;
        public Dictionary<int, GameObject> signs;
        public int index;

        public TrafficArgs()
        {
            allowed = Direction.None;
            neutral = Direction.None;
            denied = Direction.None;
            turnRed = false;
        }
    }

    public class LightState
    {
        public TrafficLevel level;
        public TrafficState pattern;

        public LightState(TrafficLevel level, TrafficState pattern)
        {
            this.level = level;
            this.pattern = pattern;
        }
    }

    [Serializable]
    public class Sign
    {
        public GameObject prefab;
        public float chance;
        public bool InIntersection;
        public bool Oneway;
        public Direction constraint;

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(typeof(Sign)))
            {
                Sign s = (Sign)obj;
                return s.prefab == prefab;
            }

            if (obj.GetType().Equals(typeof(GameObject)))
            {
                return prefab == (GameObject)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return prefab.GetHashCode();
        }
    }
}
