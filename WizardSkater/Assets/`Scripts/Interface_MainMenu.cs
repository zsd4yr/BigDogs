﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Interface_MainMenu : MonoBehaviour
{
    public LevelNumberMessenger m_levelNumber;
    public EventSystem m_events;
    public Animator m_animator;

    public bool m_transitioning;

    public int m_menuSelector;
    public Button[] m_menuSelectButtons;

    public int m_levelSelector;
    public Button m_levelSelectBackButton;
    public Button[] m_levelSelectButtons;
    public List<Text> m_levelSelectBestTimes;

    public bool m_menuInputReady = false;
    public float m_menuInputTimer;
    public float m_menuInputDelayTime = 0.1f;

    public CanvasGroup groupTitle;
    public CanvasGroup groupLevelSelect;
    public CanvasGroup groupSplashScreen;

    public enum MenuState
    {
        Splash,
        MainMenu,
        LevelSelect
    };

    public MenuState m_state;

    void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        m_animator = GetComponentInChildren<Animator>();

        m_levelNumber = FindObjectOfType<LevelNumberMessenger>();

        m_events = EventSystem.current;

        //groupLevelSelect.alpha = 1f;
        //groupLevelSelect.gameObject.SetActive(false);
    }

    void Start()
    {
        SystemsManager.m_Score.Init();

        for (int i = 0; i < m_levelSelectBestTimes.Count; i++)
        {
            if (SystemsManager.m_Score.m_levelTopScores[i] != -1)
                m_levelSelectBestTimes[i].text = SystemsManager.m_Score.m_levelTopScores[i].ToString();
            else
                m_levelSelectBestTimes[i].text = " ";
        }

        m_state = MenuState.Splash;
        StartCoroutine("FadeSplash");
    }

    private IEnumerator FadeSplash()
    {
        m_animator.SetTrigger("FadeSplash");
        yield return  new WaitForSeconds(0.7f);
        groupSplashScreen.alpha = 0f;
        groupSplashScreen.gameObject.SetActive(false);
        m_state = MenuState.MainMenu;
        m_menuSelectButtons[0].Select();
        m_events.firstSelectedGameObject = m_menuSelectButtons[0].gameObject;
    }

    void Update()
    {
        if (m_events.currentSelectedGameObject == null)
        {
            Debug.Log("Reselecting first input");
            m_events.SetSelectedGameObject(m_events.firstSelectedGameObject);
        }

        if (m_menuInputReady == false)
        {
            if (m_menuInputTimer < m_menuInputDelayTime)
            {
                m_menuInputTimer += Time.deltaTime;
            }
            else
            {
                if(SystemsManager.m_Input.IsInputClear())
                {
                    m_menuInputReady = true;
                    m_menuInputTimer = 0;
                }
            }
        }
        else
        {
            if (SystemsManager.m_Input.inp_Skate)
            {
                if (m_state == MenuState.MainMenu)
                {
                    if (m_menuSelector == 0 && SystemsManager.m_Game.m_startedTheGame == false)
                    {
                        m_menuInputReady = false;
                        SystemsManager.m_SoundFX.uiOneShot_Submit();
                        GoLevelSelect();
                    }
                    else if (m_menuSelector == 1 && Application.isEditor == false)
                    {
                        if (m_transitioning == false)
                        {
                            SystemsManager.m_SoundFX.uiOneShot_Submit();
                            StartCoroutine("QuitGame");
                        }
                    }
                }
                else if (m_state == MenuState.LevelSelect)
                {
                    if (m_levelSelector != -1)
                    {
                        if(m_transitioning == false)
                            StartCoroutine("StartGame");
                    }
                    else
                    {
                        m_menuInputReady = false;
                        SystemsManager.m_SoundFX.uiOneShot_Submit();
                        GoMainMenu();
                    }
                }
            }
            else
            {
                if (SystemsManager.m_Input.inp_D_Up)
                {
                    StartCoroutine("ChangeMenuSelector", "up");
                    m_menuInputReady = false;
                }
                else if (SystemsManager.m_Input.inp_D_Down)
                {
                    StartCoroutine("ChangeMenuSelector", "down");
                    m_menuInputReady = false;
                }
                else if (SystemsManager.m_Input.inp_D_Left)
                {
                    StartCoroutine("ChangeMenuSelector", "left");
                    m_menuInputReady = false;
                }
                else if (SystemsManager.m_Input.inp_D_Right)
                {
                    StartCoroutine("ChangeMenuSelector", "right");
                    m_menuInputReady = false;
                }
            }
        }
    }

    private IEnumerator QuitGame()
    {
        groupLevelSelect.gameObject.SetActive(false);
        groupSplashScreen.gameObject.SetActive(false);
        m_transitioning = true;
        m_animator.SetTrigger("QuitGame");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    private IEnumerator StartGame()
    {
        m_transitioning = true;
        m_levelNumber.m_level = m_levelSelector + 1;
        m_animator.SetTrigger("StartGame");
        SystemsManager.m_SoundFX.uiOneShot_Submit();
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(1);
    }

    public IEnumerator MenuTransitionMainMenu(bool fromLS)
    {
        m_transitioning = true;

        m_levelSelector = 0;

        groupTitle.gameObject.SetActive(true);
        groupTitle.alpha = 0f;

        m_menuSelectButtons[m_menuSelector].Select();
        m_events.firstSelectedGameObject = m_menuSelectButtons[m_menuSelector].gameObject;

        m_animator.SetTrigger("LevelSelectOut");

        yield return new WaitForSeconds(0.25f);

        groupTitle.alpha = 1f;

        groupLevelSelect.alpha = 0f;
        groupLevelSelect.gameObject.SetActive(false);
        
        m_state = MenuState.MainMenu;

        m_transitioning = false;
    }

    public void GoMainMenu()
    {
        if (m_transitioning == false)
            StartCoroutine("MenuTransitionMainMenu", true);
    }

    public IEnumerator MenuTransitionLevelSelect()
    {
        m_transitioning = true;

        groupLevelSelect.gameObject.SetActive(true);
        groupLevelSelect.alpha = 0f;

        m_animator.SetTrigger("LevelSelectIn");

        for (int i = 0; i < m_levelSelectButtons.Length; i++) //buttons
        {
            m_levelSelectButtons[i].enabled = true;
        }

        m_levelSelectButtons[0].Select();
        m_events.firstSelectedGameObject = m_levelSelectButtons[0].gameObject;

        //for (int i = 0; i < m_levelSelectBestTimes.Count - 1; i += 1) //best times
        //{
        //    float top = SystemsManager.m_Score.m_levelTopScores[i];
        //    string topS = top.ToString("0.00");
        //    if (SystemsManager.m_Score.m_levelTopScores.Count >= i && top != -1)
        //    {
        //        m_levelSelectBestTimes[i].text = topS;
        //        m_levelSelectBestTimesShadows[i].text = topS;
        //    }
        //}
        //for (int i = 1; i < m_levelSelectBestPerfectTimes.Count - 1; i += 1) //best perfect time
        //{
        //    float top = SystemsManager.m_Score.BestLevelPerfectTimes[i];
        //    string topS = top.ToString("0.00");
        //    if (SystemsManager.m_Score.BestLevelPerfectTimes.Count >= i && top != -1)
        //    {
        //        m_levelSelectBestPerfectTimes[i].text = topS;
        //        m_levelSelectBestPerfectTimesShadows[i].text = topS;
        //    }
        //}

        yield return new WaitForSeconds(0.25f);

        groupLevelSelect.alpha = 1f;

        groupTitle.alpha = 0f;
        groupTitle.gameObject.SetActive(false);

        m_state = MenuState.LevelSelect;

        Debug.Log("ls in");
       
        yield return null;

        m_transitioning = false;
    }

    public void GoLevelSelect()
    {
        if (m_transitioning == false)
            StartCoroutine("MenuTransitionLevelSelect");
    }

    public IEnumerator ChangeMenuSelector(string dir)
    {
        if (dir == "up")
        {
            if (m_state == MenuState.MainMenu)
            {
                //if (m_menuSelector == 0)
                //{
                //    m_menuSelector = 3;
                //}
                //else if (m_menuSelector == 3)
                //{
                //    m_menuSelector = 0;
                //}
                //m_menuSelectButtons[m_menuSelector].Select();
            }
            else if (m_state == MenuState.LevelSelect)
            {
                if (m_levelSelector != -1)
                {
                    m_levelSelector -= 2;
                    if (m_levelSelector < 0)
                        m_levelSelector += 4;
                    m_levelSelectButtons[m_levelSelector].Select();
                    m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                    SystemsManager.m_SoundFX.uiOneShot_Move();
                }
            }
        }
        else if (dir == "down")
        {
            if (m_state == MenuState.MainMenu)
            {
                //if (m_menuSelector == 0)
                //{
                //    m_menuSelector = 3;
                //}
                //else if (m_menuSelector == 3)
                //{
                //    m_menuSelector = 0;
                //}
                //m_menuSelectButtons[m_menuSelector].Select();
            }
            else if (m_state == MenuState.LevelSelect)
            {
                if (m_levelSelector != -1)
                {
                    m_levelSelector += 2;
                    if (m_levelSelector > 3)
                        m_levelSelector -= 4;
                    m_levelSelectButtons[m_levelSelector].Select();
                    m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                    SystemsManager.m_SoundFX.uiOneShot_Move();
                }
            }
        }
        else if (dir == "left")
        {
            if (m_state == MenuState.MainMenu)
            {
                if (m_menuSelector == 0)
                {
                    m_menuSelector = 1;
                }
                else if (m_menuSelector == 1)
                {
                    m_menuSelector = 0;
                }
                //else if (m_menuSelector == 2)
                //{
                //    m_menuSelector = 1;
                //}
                m_menuSelectButtons[m_menuSelector].Select();
                m_events.firstSelectedGameObject = m_menuSelectButtons[m_menuSelector].gameObject;
                SystemsManager.m_SoundFX.uiOneShot_Move();
            }
            else if (m_state == MenuState.LevelSelect)
            {
                if(m_levelSelector != -1)
                { 
                    if (m_levelSelector-1 < 0 || m_levelSelector-1 == 1)
                    {
                        m_levelSelectButtons[m_levelSelector].Select();
                        m_events.firstSelectedGameObject = m_levelSelectBackButton.gameObject;
                        m_levelSelectBackButton.Select();
                        m_levelSelector = -1;
                        SystemsManager.m_SoundFX.uiOneShot_Move();
                    }
                    else
                    {
                        m_levelSelector--;
                        m_levelSelectButtons[m_levelSelector].Select();
                        m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                        SystemsManager.m_SoundFX.uiOneShot_Move();
                    }
                }
                else
                {
                    m_levelSelector = 1;
                    m_levelSelectButtons[m_levelSelector].Select();
                    m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                    SystemsManager.m_SoundFX.uiOneShot_Move();
                }
            }
        }
        else if (dir == "right")
        {
            if (m_state == MenuState.MainMenu)
            {
                if (m_menuSelector == 0)
                {
                    m_menuSelector = 1;
                }
                else if (m_menuSelector == 1)
                {
                    m_menuSelector = 0;
                }
                //else if (m_menuSelector == 2)
                //{
                //    m_menuSelector = 0;
                //}
                m_menuSelectButtons[m_menuSelector].Select();
                m_events.firstSelectedGameObject = m_menuSelectButtons[m_menuSelector].gameObject;
                SystemsManager.m_SoundFX.uiOneShot_Move();
            }
            else if (m_state == MenuState.LevelSelect)
            {
                if (m_levelSelector != -1)
                {
                    if (m_levelSelector + 1 > 3 || m_levelSelector + 1 == 2)
                    {
                        m_levelSelectButtons[m_levelSelector].Select();
                        m_levelSelectBackButton.Select();
                        m_events.firstSelectedGameObject = m_levelSelectBackButton.gameObject;
                        m_levelSelector = -1;
                        SystemsManager.m_SoundFX.uiOneShot_Move();
                    }
                    else
                    {
                        m_levelSelector++;
                        m_levelSelectButtons[m_levelSelector].Select();
                        m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                        SystemsManager.m_SoundFX.uiOneShot_Move();
                    }
                }
                else
                {
                    m_levelSelector = 0;
                    m_levelSelectButtons[m_levelSelector].Select();
                    m_events.firstSelectedGameObject = m_levelSelectButtons[m_levelSelector].gameObject;
                    SystemsManager.m_SoundFX.uiOneShot_Move();
                }
            }
        }

        yield return null;
    }

    public void Quit()
    {
        Application.Quit();
    }
}