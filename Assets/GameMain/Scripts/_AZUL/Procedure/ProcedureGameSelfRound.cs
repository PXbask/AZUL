using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameSelfRound : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_ShouldChange = false;
        private bool m_ResetGame = false;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.CurrentPlayer = PlaceAreaCamp.Self;

            m_BoardGameComponent.m_Interactive = true;
            m_ShouldChange = false;
            m_ResetGame = false;

            // 订阅棋子移动完成事件
            GameEntry.Event.Subscribe(MovePieceCompleteEventArgs.EventId, OnMovePieceComplete);
            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);

            GameEntry.Referee.ShowTip("现在是你的回合");
        }

        private void OnGameReset(object sender, GameEventArgs e)
        {
            m_ResetGame = true;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            m_BoardGameComponent.m_Interactive = false;
            // 取消订阅棋子移动完成事件
            if (GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(GameResetEventArgs.EventId, OnGameReset);
                GameEntry.Event.Unsubscribe(MovePieceCompleteEventArgs.EventId, OnMovePieceComplete);
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_ResetGame)
            {
                ChangeState<ProcedureGameReset>(procedureOwner);
            }

            if (m_ShouldChange)
            {
                //先判断当前桌子上是否还有棋子，如果没有则将双方填满的手动区移动到颜色区
                if (m_BoardGameComponent.MidFactoryAreaEmpty())
                {
                    ChangeState<ProcedureGameStepSettle>(procedureOwner);
                    return;
                }

                if (m_BoardGameComponent.CurrentPlayer == PlaceAreaCamp.Self)
                {
                    ChangeState<ProcedureGameOtherRound>(procedureOwner);
                    return;
                }

                if (m_BoardGameComponent.CurrentPlayer == PlaceAreaCamp.Other)
                {
                    ChangeState<ProcedureGameSelfRound>(procedureOwner);
                    return;
                }
            }
        }

        /// <summary>
        /// 棋子移动完成事件回调
        /// </summary>
        private void OnMovePieceComplete(object sender, GameEventArgs e)
        {
            MovePieceCompleteEventArgs ne = (MovePieceCompleteEventArgs)e;

            if(ne.Camp == PlaceAreaCamp.Self)
            {
                m_ShouldChange=true;
                m_BoardGameComponent.m_Interactive = false;
            }
            else
            {
                Log.Error("当前流程存在错误");
            }
        }
    }
}
