using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class NgoNetComponent : GameFrameworkComponent
    {
        private bool m_Running = false;

        [SerializeField]
        private NetworkManager m_NetworkManager;
        public NetworkManager NetworkManager => m_NetworkManager;

        private UnityTransport m_Transport;
        public UnityTransport Transport => m_Transport;

        protected override void Awake()
        {
            base.Awake();
            m_Running = false;

            m_NetworkManager = GameObject.FindFirstObjectByType<NetworkManager>();
            if(m_NetworkManager == null)
            {
                Log.Error("未找到NetworkManager组件");
                return;
            }

            m_Transport = m_NetworkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (m_Transport == null)
            {
                Log.Error("未找到UnityTransport组件");
                return;
            }
        }

        private void Run()
        {
            if (m_Running)
            {
                Log.Warning("NgoNet组件已经在运行中");
                return;
            }

            m_Running=true;
        }

        public void StartHost()
        {
            if (!m_Running)
            {
                Log.Error("NgoNet组件未运行，无法启动Host");
                return;
            }

            m_NetworkManager.StartHost();
        }

        public void StartClient()
        {
            if (!m_Running)
            {
                Log.Error("NgoNet组件未运行，无法启动Client");
                return;
            }

            m_NetworkManager.StartClient();
        }
    }
}
