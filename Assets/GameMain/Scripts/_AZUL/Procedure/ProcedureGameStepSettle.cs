using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameStepSettle : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_SettleStart;
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

            m_SettleStart = false;
            m_ResetGame=false;

            GameEntry.Referee.ShowTip("桌上没有棋子了, 即将结算...");
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

            if (!m_SettleStart)
            {
                m_BoardGameComponent.StepSettle();
                //判断是否满足结束条件：存在某一玩家的颜色区的横排全部填满
                bool matchGameOverCondition = m_BoardGameComponent.ExistColoredAreaRowFullFilled();

                m_SettleStart = true;

                if (matchGameOverCondition)
                {
                    ChangeState<ProcedureGameFinalSettle>(procedureOwner);
                }
                else
                {
                    //重新发牌
                    ChangeState<ProcedureGameDealCards>(procedureOwner);
                }
            }
        }
    }
}
