//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using DG.Tweening;
using System;
using System.Net;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class MenuForm : UGuiForm
    {
        [SerializeField]
        private Button m_StartButtonSingle = null;

        [SerializeField]
        private Button m_StartButtonMulti = null;

        [SerializeField]
        private TMP_InputField m_InputFieldMulti = null;

        [SerializeField]
        private Toggle m_ToggleMulti = null;

        [SerializeField]
        private TextMeshProUGUI m_ToggleTextServer = null;

        [SerializeField]
        private TextMeshProUGUI m_ToggleTextClient = null;

        [SerializeField]
        private Button m_QuitButton = null;

        [SerializeField]
        private CanvasGroup m_FormCanvasGroup = null;

        private Tween m_StartTween = null;
        private Tween m_FadeTween = null;

        private ProcedureMenu m_ProcedureMenu = null;

        private static readonly float FADE_ANIM_INTERVAL = 0.5f;
        private static readonly string DEFAULT_IP = "127.0.0.1:7777";

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_StartButtonSingle.onClick.AddListener(OnSingleStartButtonClick);
            m_StartButtonMulti.onClick.AddListener(OnMultiStartButtonClick);
            m_QuitButton.onClick.AddListener(OnQuitButtonClick);

            m_ToggleMulti.onValueChanged.AddListener(OnToggleMultiValueChanged);
            m_ToggleMulti.onValueChanged.Invoke(true);

            m_InputFieldMulti.placeholder.GetComponent<TextMeshProUGUI>().text = DEFAULT_IP;
        }

        private void OnToggleMultiValueChanged(bool v)
        {
            m_ToggleTextClient.gameObject.SetActive(!v);
            m_ToggleTextServer.gameObject.SetActive(v);
        }

        private void OnMultiStartButtonClick()
        {
            //解析输入的IP地址
            string text = m_InputFieldMulti.text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                text = DEFAULT_IP;
            }

            string ipPart = string.Empty;
            int port = 0;
            if (text.Contains(":"))
            {
                var parts = text.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    ipPart = parts[0];
                    if (!int.TryParse(parts[1], out port) || port < 0 || port > 65535)
                    {
                        Log.Error("Invalid port, using default: " + port);
                        return;
                    }
                }
            }
            else
            {
                return;
            }

            // 验证 IP（支持 IPv4 / IPv6）
            if (!IPAddress.TryParse(ipPart, out IPAddress address))
            {
                Log.Error("Invalid IP address: " + ipPart);
                return;
            }

            ApplyToUnityTransport(ipPart, (ushort)port);
        }

        public void OnSingleStartButtonClick()
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
                m_StartButtonSingle = null;
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

        private void ApplyToUnityTransport(string ip, ushort port)
        {
            var ut = GameEntry.NgoNet.Transport;
            if (ut != null)
            {
                ut.SetConnectionData(ip, port);
                Log.Info($"Set UnityTransport connection data to IP: {ip}, Port: {port}");
            }
            else
            {
                Log.Error("UnityTransport is not being used as the network transport.");
            }
        }
    }
}
