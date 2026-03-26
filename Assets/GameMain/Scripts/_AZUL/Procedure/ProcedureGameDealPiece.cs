using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameDealPiece : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_DealCompleted = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.GameReset();
            m_DealCompleted = false;

            m_BoardGameComponent.DealPiece();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_DealCompleted)
            {

            }
        }

        public void DealCompleted()
        {
            m_DealCompleted = true;
        }
    }
}
