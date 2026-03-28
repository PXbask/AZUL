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

        private bool m_Settle;
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_BoardGameComponent = GameEntry.BoardGame;
            m_BoardGameComponent.CanInteractive = false;
            m_Settle = false;

            GameEntry.Referee.ShowTip("桌上没有棋子了, 即将结算...");
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_Settle)
            {
                m_BoardGameComponent.MoveFilledRowInManualAreaToColoredArea();
                //判断是否满足结束条件：存在某一玩家的颜色区的横排全部填满
                bool res = m_BoardGameComponent.ExistColoredAreaRowFullFilled();
                m_Settle = true;
                if (m_Settle)
                {
                    if (res)
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
}
