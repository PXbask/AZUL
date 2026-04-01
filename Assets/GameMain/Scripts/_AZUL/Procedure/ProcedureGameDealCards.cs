using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameDealCards : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_DealStart = false;
        private bool m_DealCompleted = false;
        private bool m_ResetGame = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);
            GameEntry.Event.Subscribe(DealPiecesDoneEventArgs.EventId, OnDealPiecesDone);

            m_BoardGameComponent = GameEntry.BoardGame;
            m_DealStart = false;
            m_DealCompleted = false;
            m_ResetGame = false;
        }

        private void OnDealPiecesDone(object sender, GameEventArgs e)
        {
            m_DealCompleted = true;
        }

        private void OnGameReset(object sender, GameEventArgs e)
        {
            m_ResetGame = true;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(GameResetEventArgs.EventId, OnGameReset);
            GameEntry.Event.Unsubscribe(DealPiecesDoneEventArgs.EventId, OnDealPiecesDone);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_ResetGame)
            {
                ChangeState<ProcedureGameReset>(procedureOwner);
            }

            if (!m_DealStart)
            {
                m_DealStart = true;
                m_BoardGameComponent.DealPiece();
            }

            if (m_DealCompleted)
            {
                if (m_BoardGameComponent.CurrentPlayer == PlaceAreaCamp.Self)
                {
                    ChangeState<ProcedureGameSelfRound>(procedureOwner);
                }
                if (m_BoardGameComponent.CurrentPlayer == PlaceAreaCamp.Other)
                {
                    ChangeState<ProcedureGameOtherRound>(procedureOwner);
                }
            }
        }
    }
}
