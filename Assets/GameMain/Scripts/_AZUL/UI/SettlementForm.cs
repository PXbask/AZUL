using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class SettlementForm : UGuiForm
    {
        [SerializeField]
        private TextMeshProUGUI m_ResultText = null;

        private ProcedureGameSettlePanel m_ProcedureSettlePanel = null;

        public void OnRestartButtonClick()
        {
            m_ProcedureSettlePanel.RestartGame();
        }

        public void OnQuitButtonClick()
        {
            //GameEntry.UI.OpenDialog(new DialogParams()
            //{
            //    Mode = 2,
            //    Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
            //    Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
            //    OnClickConfirm = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            //});
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_ProcedureSettlePanel = (ProcedureGameSettlePanel)userData;
            if (m_ProcedureSettlePanel == null)
            {
                Log.Warning("ProcedureGameSettlePanel is invalid when open SettlementForm.");
                return;
            }

            var winner = GameEntry.BoardGame.GetWinner();
            if (winner == null)
            {
                m_ResultText.text = "No Winner";
            }
            else
            {
                if(winner.camp == PlaceAreaCamp.Self)
                {
                    m_ResultText.text = "You Win!Score:" + winner.Score;
                }
                else
                {
                    m_ResultText.text = "You Lose!Score:" + winner.Score;
                }
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            m_ProcedureSettlePanel = null;

            base.OnClose(isShutdown, userData);
        }
    }
}
