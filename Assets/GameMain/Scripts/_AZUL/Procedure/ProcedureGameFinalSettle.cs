using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameFinalSettle : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_FinalSettle = false;
        private bool m_ResetGame = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);

            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.m_Interactive = false;
            m_FinalSettle = false;
            m_ResetGame = false;

            GameEntry.Referee.ShowTip("检测到有玩家达成胜利条件,进行最终结算...");
        }

        private void OnGameReset(object sender, GameEventArgs e)
        {
            m_ResetGame = true;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(GameResetEventArgs.EventId, OnGameReset);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_ResetGame)
            {
                ChangeState<ProcedureGameReset>(procedureOwner);
            }

            if (!m_FinalSettle)
            {
                m_BoardGameComponent.FinalSettlement();
                m_FinalSettle = true;
                if (m_FinalSettle)
                {
                    ChangeState<ProcedureGameSettlePanel>(procedureOwner);
                }
            }
        }
    }
}
