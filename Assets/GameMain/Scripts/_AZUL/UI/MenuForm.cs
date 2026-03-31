//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class MenuForm : UGuiForm
    {
        [SerializeField]
        private Button m_StartButton = null;

        [SerializeField]
        private Button m_QuitButton = null;

        [SerializeField]
        private CanvasGroup m_FormCanvasGroup = null;

        private Tween m_StartTween = null;
        private Tween m_FadeTween = null;

        private ProcedureMenu m_ProcedureMenu = null;

        private static readonly float FADE_ANIM_INTERVAL = 0.5f;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_StartButton.onClick.AddListener(OnStartButtonClick);
            m_QuitButton.onClick.AddListener(OnQuitButtonClick);
        }

        public void OnStartButtonClick()
        {
            m_ProcedureMenu.StartGame();
            PlayCloseAnim();
        }

        public void OnQuitButtonClick()
        {
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_ProcedureMenu = (ProcedureMenu)userData;
            if (m_ProcedureMenu == null)
            {
                Log.Warning("ProcedureMenu is invalid when open MenuForm.");
                return;
            }

            PlayStartAnim();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            m_ProcedureMenu = null;

            if (m_StartTween != null)
            {
                m_StartTween.Kill();
                m_StartButton = null;
            }

            if (m_FadeTween != null)
            {
                m_FadeTween.Kill();
                m_FadeTween = null;
            }

            base.OnClose(isShutdown, userData);
        }

        private void PlayStartAnim()
        {
            this.m_FormCanvasGroup.alpha = 0;
            m_FormCanvasGroup.interactable = false;
            m_StartTween = m_FormCanvasGroup.DOFade(1, FADE_ANIM_INTERVAL);
            m_StartTween.OnComplete(() =>
            {
                m_FormCanvasGroup.interactable = true;
            });
        }

        private void PlayCloseAnim()
        {
            this.m_FormCanvasGroup.alpha = 1;
            m_FormCanvasGroup.interactable = false;
            m_FadeTween = m_FormCanvasGroup.DOFade(0, FADE_ANIM_INTERVAL);
            m_FadeTween.OnComplete(() =>
            {
                Close();
            });
        }
    }
}
