using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    /// <summary>
    /// 重置游戏
    /// </summary>
    public class ProcedureGameReset : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_ResetCompleted = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_BoardGameComponent = GameEntry.BoardGame;
            m_ResetCompleted = false;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_ResetCompleted)
            {
                m_BoardGameComponent.GameReset();
                m_ResetCompleted = true;
                if (m_ResetCompleted)
                {
                    ChangeState<ProcedureGameDealCards>(procedureOwner);
                }
            }
        }
    }
}
