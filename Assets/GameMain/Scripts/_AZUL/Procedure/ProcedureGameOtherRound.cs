using DG.Tweening;
using GameFramework.Event;
using System;
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
        private bool m_ResetGame = false;
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.CurrentPlayer = PlaceAreaCamp.Other;

            m_BoardGameComponent.m_Interactive = !m_BoardGameComponent.FightwithAI;
            m_ShouldChange = false;
            m_ResetGame = false;

            // 订阅棋子移动完成事件
            GameEntry.Event.Subscribe(MovePieceCompleteEventArgs.EventId, OnMovePieceComplete);
            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);
            GameEntry.Event.Subscribe(ReceiveAIServerMsgEventArgs.EventId, OnReceiveAIServerMsg);

            GameEntry.Referee.ShowTip("现在是对手的回合");

            //如果是和机器人打牌，则向AI服务器发送消息
            if (m_BoardGameComponent.FightwithAI)
            {
                m_BoardGameComponent.SendCurrentBoardInfoToAIServer();
            }
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
                GameEntry.Event.Unsubscribe(ReceiveAIServerMsgEventArgs.EventId, OnReceiveAIServerMsg);
            }
        }

        private void OnReceiveAIServerMsg(object sender, GameEventArgs e)
        {
            if (GameEntry.BoardGame.FightwithAI)
            {
                ReceiveAIServerMsgEventArgs ne = (ReceiveAIServerMsgEventArgs)e;
                AIAction aiAction = ne.AIAction;
                //延时2秒执行AI动作，模拟AI思考时间
                DOVirtual.DelayedCall(2f, () =>
                {
                    GameEntry.BoardGame.ExecuteAIAction(aiAction);
                });
            }
            else
            {
                Log.Error("当前没有与AI打牌但收到了AI服务器消息");
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

            if (ne.Camp == PlaceAreaCamp.Other)
            {
                m_ShouldChange = true;
                m_BoardGameComponent.m_Interactive = false; // 禁止交互，等待流程切换完成
            }
            else
            {
                Log.Error("当前流程存在错误");
            }
        }
    }
}
