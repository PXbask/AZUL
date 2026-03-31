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

        public IEnumerator Init()
        {
            m_ResetButton.onClick.AddListener(OnClickReset);
            m_ResetButtonWithAI.onClick.AddListener(OnClickResetWithAI);
            m_QuitButton.onClick.AddListener(OnClickQuit);

            yield return null;
            //等待PlayerView加载完毕
            m_MainCamera = GameEntry.PlayerView.GetPlayerCamera();
            m_Canvas.worldCamera = m_MainCamera;
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
                //只绕y轴旋转
                var rot = m_MainCamera.transform.rotation.eulerAngles;
                m_Canvas.transform.rotation = Quaternion.Euler(0, rot.y, 0);
            }
        }

        private void OnClickReset()
        {
            if (GameEntry.AI.IsRunning())
            {
                GameEntry.AI.Stop();
            }
            GameEntry.Event.Fire(this, GameResetEventArgs.Create());
        }

        private void OnClickResetWithAI()
        {
            if (!GameEntry.AI.IsRunning())
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
