using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class RefereeComponent : GameFrameworkComponent
    {
        private bool m_Running = false;
        private bool m_HasRegisterEvent = false;

        [SerializeField]
        private TextMeshProUGUI tipText;
        [SerializeField]
        private string tipTextPath;

        protected override void Awake()
        {
            base.Awake();
            m_Running = false;
            m_HasRegisterEvent = false;
        }

        private void Update()
        {
            if (!m_HasRegisterEvent)
            {
                if (GameEntry.Event != null)
                {
                    m_HasRegisterEvent = true;
                    GameEntry.Event.Subscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
                }
            }
        }

        private void OnDisable()
        {
            if(GameEntry.Event != null)
                GameEntry.Event.Unsubscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
        }

        private void OnBoardGameSceneEnter(object sender, GameEventArgs e)
        {
            this.Run();
            this.ShowTip("欢迎来到AZUL!准备好游戏了吗?");
        }

        /// <summary>
        /// 运行裁判组件
        /// </summary>
        public void Run()
        {
            m_Running = true;
            BindingSceneObjects();
        }

        private void BindingSceneObjects()
        {
            if(tipText == null)
            {
                var obj = GameObject.Find(tipTextPath);
                if(obj == null)
                {
                    Log.Error("Failed to find tip text object at path: {0}", tipTextPath);
                    return;
                }
                tipText = obj.GetComponent<TextMeshProUGUI>();
            }
        }

        public void ShowTip(string tipText)
        {
            if (!m_Running)
            {
                Log.Error("Referee is not running.");
                return;
            }

            this.tipText.text = tipText;
            Log.Info("Referee shows tip: {0}", tipText);
        }
    }
}
