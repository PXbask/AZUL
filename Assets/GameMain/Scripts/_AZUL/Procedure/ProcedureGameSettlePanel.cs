using GameFramework;
using GameFramework.Event;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureGameSettlePanel : ProcedureBase
    {
        private bool m_ResetGame = false;
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(GameResetEventArgs.EventId, OnGameReset);
            m_ResetGame = false;

            ShowWinner();
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
        }

        public void ShowWinner()
        {
            var winner = GameEntry.BoardGame.GetWinner();
            if (winner == null)
            {
                GameEntry.Referee.ShowTip("双方平局");
            }
            else
            {
                if (winner.camp == PlaceAreaCamp.Self)
                {
                    GameEntry.Referee.ShowTip(Utility.Text.Format("你赢了!最终得分{0}分", winner.Score));
                }
                else
                {
                    GameEntry.Referee.ShowTip(Utility.Text.Format("对手赢了!最终得分{0}分", winner.Score));
                }
            }
        }
    }
}
