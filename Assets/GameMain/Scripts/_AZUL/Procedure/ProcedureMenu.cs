using GameFramework.Event;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureMenu : ProcedureBase
    {
        private bool m_StartGame = false;
        private bool m_Flag = false;

        private BoardGameComponent m_BoardGameComponent = null;

        public void StartGame()
        {
            m_StartGame = true;
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_StartGame = false;
            m_Flag = false;
            m_BoardGameComponent = GameEntry.BoardGame;

            GameEntry.UI.OpenUIForm((int)UIFormId.MenuForm, this);

            GameEntry.Event.Fire(this, BoardGameSceneEnterEventArgs.Create());
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_StartGame)
            {
                if (!m_Flag)
                {
                    m_Flag = true;

                    //动画播放完毕后切换流程
                    m_BoardGameComponent.PlayStartGameCameraAnim(() =>
                    {
                        ChangeState<ProcedureMain>(procedureOwner);
                    });
                }
            }
        }
    }
}
