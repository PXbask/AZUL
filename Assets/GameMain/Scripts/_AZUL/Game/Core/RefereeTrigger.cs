using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class RefereeTrigger : MonoBehaviour
    {
        [SerializeField]
        private Canvas m_Canvas = null;

        [SerializeField]
        private Button m_ResetButton = null;

        [SerializeField]
        private Button m_QuitButton = null;

        private Camera m_MainCamera;

        private void Start()
        {
            m_MainCamera = Camera.main;
            m_ResetButton.onClick.AddListener(OnClickReset);
            m_QuitButton.onClick.AddListener(OnClickQuit);

            m_Canvas.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            m_Canvas.gameObject.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            m_Canvas.gameObject.SetActive(false);
        }

        private void Update()
        {
            FaceToCamera();
        }

        /// <summary>
        /// 使 Canvas 始终朝向主摄像机
        /// </summary>
        private void FaceToCamera()
        {
            if (m_MainCamera != null && m_Canvas != null)
            {
                m_Canvas.transform.rotation = m_MainCamera.transform.rotation;
            }
        }

        private void OnClickReset()
        {
            GameEntry.Event.Fire(this, GameResetEventArgs.Create());
        }

        private void OnClickQuit()
        {
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
        }
    }
}
