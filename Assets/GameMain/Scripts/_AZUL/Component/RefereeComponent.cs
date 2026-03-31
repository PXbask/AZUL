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

        [Header("提示文本")]
        [SerializeField]
        private TextMeshProUGUI m_TipText;
        [SerializeField]
        private string m_TipTextPath;

        [Header("内置菜单")]
        [SerializeField]
        private RefereeTrigger m_Trigger;
        [SerializeField]
        private string m_TriggerPath;

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
            if(m_TipText == null)
            {
                var obj = GameObject.Find(m_TipTextPath);
                if(obj == null)
                {
                    Log.Error("Failed to find tip text object at path: {0}", m_TipTextPath);
                    return;
                }
                m_TipText = obj.GetComponent<TextMeshProUGUI>();
            }

            if(m_Trigger == null)
            {
                var obj = GameObject.Find(m_TriggerPath);
                if(obj == null)
                {
                    Log.Error("Failed to find referee trigger object at path: {0}", m_TriggerPath);
                    return;
                }
                m_Trigger = obj.GetComponent<RefereeTrigger>();
                StartCoroutine(m_Trigger.Init());
            }
        }

        public void ShowTip(string tipText)
        {
            if (!m_Running)
            {
                Log.Error("Referee is not running.");
                return;
            }

            this.m_TipText.text = tipText;
            Log.Info("Referee shows tip: {0}", tipText);
        }
    }
}
