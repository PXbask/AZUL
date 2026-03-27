using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameOtherRound : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_ShouldChange = false;
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.CanInteractive = true;
            m_ShouldChange = false;

            // 订阅棋子移动完成事件
            GameEntry.Event.Subscribe(MovePieceCompleteEventArgs.EventId, OnMovePieceComplete);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            m_BoardGameComponent.CanInteractive = false;
            // 取消订阅棋子移动完成事件
            if (GameEntry.Event != null)
            {
                GameEntry.Event.Unsubscribe(MovePieceCompleteEventArgs.EventId, OnMovePieceComplete);
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_ShouldChange)
            {
                m_ShouldChange = false;
                //先判断当前桌子上是否还有棋子，如果没有则将双方填满的手动区移动到颜色区
                if (m_BoardGameComponent.MidFactoryAreaEmpty())
                {
                    ChangeState<ProcedureGameStepSettle>(procedureOwner);
                    return;
                }
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

        /// <summary>
        /// 棋子移动完成事件回调
        /// </summary>
        private void OnMovePieceComplete(object sender, GameEventArgs e)
        {
            MovePieceCompleteEventArgs ne = (MovePieceCompleteEventArgs)e;

            if (ne.Camp == PlaceAreaCamp.Other)
            {
                m_ShouldChange = true;
                m_BoardGameComponent.CanInteractive = false; // 禁止交互，等待流程切换完成
                return; // 只处理自己回合的棋子移动完成事件
            }
            else
            {
                Log.Error("当前流程存在错误");
                return;
            }
        }
    }
}
