using GameFramework.Event;
using StarForce;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureMain : ProcedureBase
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

            //初始化裁判
            GameEntry.Referee.GameInit();
            GameEntry.Referee.ShowTip("欢迎来到AZUL! 准备好了吗?");

            //GameEntry.AI.Run();
            //GameEntry.AI.SendNetworkMessage("hello");
        }

        private void OnGameReset(object sender, GameEventArgs e)
        {
            m_ResetGame=true;
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
    }
}
