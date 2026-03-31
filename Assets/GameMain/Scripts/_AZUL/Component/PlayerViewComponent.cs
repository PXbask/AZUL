using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class PlayerViewComponent : GameFrameworkComponent
    {
        private bool m_HasRegisterEvent = false;
        private bool m_Running = false;

        [SerializeField]
        private Camera m_PlayerCamera = null;

        private CameraMovement m_Movement = null;

        protected override void Awake()
        {
            base.Awake();
            m_HasRegisterEvent = false;
            m_Running = false;
        }

        private void Update()
        {
            if (!m_HasRegisterEvent)
            {
                if(GameEntry.Event != null)
                {
                    GameEntry.Event.Subscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
                    m_HasRegisterEvent = true;
                }
            }
        }

        private void OnDisable()
        {
            if (GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
            }
        }

        private void OnBoardGameSceneEnter(object sender, GameEventArgs e)
        {
            m_Running = true;
            RegisterSceneObjects();
        }

        private void RegisterSceneObjects()
        {
            if(m_PlayerCamera == null)
            {
                m_PlayerCamera = Camera.main;
                if(m_PlayerCamera == null)
                {
                    Log.Error("PlayerViewComponent can not find main camera.");
                }
                else
                {
                    m_Movement = m_PlayerCamera.gameObject.GetOrAddComponent<CameraMovement>();
                    //默认玩家视角不允许移动，直到动画播放完毕
                    SetMovementActive(false);
                }
            }
        }

        public void SetMovementActive(bool active)
        {
            if (!m_Running)
            {
                Log.Error("PlayerViewComponent is not running, can not set movement active.");
                return;
            }
            if(m_Movement != null)
            {
                m_Movement.enabled = active;
            }
            else
            {
                Log.Error("CameraMovement component is not found on player camera.");
            }
        }

        public Camera GetPlayerCamera() => m_PlayerCamera;
    }
}
