using GameFramework.Event;
using System;
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

        private bool m_ResetStart = false;
        private bool m_ResetCompleted = false;
        private bool m_Flag = false;
        private bool m_GameReset = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(ClearTableDoneEventArgs.EventId, OnClearTableDone);
            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);

            m_BoardGameComponent = GameEntry.BoardGame;

            m_Flag = false;
            m_ResetStart = false;
            m_ResetCompleted = false;
            m_GameReset = false;
        }

        private void OnGameReset(object sender, GameEventArgs e)
        {
            m_GameReset=true;
        }

        private void OnClearTableDone(object sender, GameEventArgs e)
        {
            m_ResetCompleted = true;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(ClearTableDoneEventArgs.EventId, OnClearTableDone);
            GameEntry.Event.Unsubscribe(GameResetEventArgs.EventId, OnGameReset);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_GameReset)
            {
                ChangeState<ProcedureGameReset>(procedureOwner);
            }

            if (!m_ResetStart)
            {
                m_ResetStart = true;
                m_BoardGameComponent.BoardReset();
            }

            if (m_ResetCompleted)
            {
                if (!m_Flag)
                {
                    m_Flag = true;
                    ChangeState<ProcedureGameDealCards>(procedureOwner);
                }
            }
        }
    }
}
