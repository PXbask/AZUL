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
        private Button m_ResetButtonWithAI = null;

        [SerializeField]
        private Button m_QuitButton = null;

        private Camera m_MainCamera;

        private void Start()
        {
            m_MainCamera = Camera.main;
            m_ResetButton.onClick.AddListener(OnClickReset);
            m_ResetButtonWithAI.onClick.AddListener(OnClickResetWithAI);
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
                //高度与相机平齐
                //var pos = m_Canvas.transform.position;
                //pos.y = m_MainCamera.transform.position.y;
                //m_Canvas.transform.position = pos;

                //只绕y轴旋转
                var rot = m_MainCamera.transform.rotation.eulerAngles;
                m_Canvas.transform.rotation = Quaternion.Euler(0, rot.y, 0);
            }
        }

        private void OnClickReset()
        {
            if (GameEntry.AI.IsActive())
            {
                GameEntry.AI.Stop();
            }
            GameEntry.Event.Fire(this, GameResetEventArgs.Create());
        }

        private void OnClickResetWithAI()
        {
            if (!GameEntry.AI.IsActive())
            {
                GameEntry.AI.Run();
            }
            GameEntry.Event.Fire(this, GameResetEventArgs.Create());
        }

        private void OnClickQuit()
        {
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
        }
    }
}
