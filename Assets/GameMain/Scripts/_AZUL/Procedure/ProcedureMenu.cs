using GameFramework.Event;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace AZUL
{
    public class ProcedureMenu : ProcedureBase
    {
        private BoardGameComponent m_BoardGameComponent = null;

        private bool m_StartGame = false;

        private MenuForm m_MenuForm = null;

        public void StartGame()
        {
            m_StartGame = true;
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_BoardGameComponent = GameEntry.BoardGame;

            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            m_StartGame = false;
            GameEntry.UI.OpenUIForm((int)UIFormId.MenuForm, this);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_StartGame)
            {
                //procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Main"));
                //procedureOwner.SetData<VarByte>("GameMode", (byte)GameMode.Survival);
                //ChangeState<ProcedureChangeScene>(procedureOwner);
                m_StartGame = false;
                m_BoardGameComponent.GameInit();
                m_BoardGameComponent.MoveCameraAnimWhenStartGame(() =>
                {
                    ChangeState<ProcedureMain>(procedureOwner);
                });

                if (m_MenuForm != null)
                {
                    m_MenuForm.Close();  // 传入 true，立即关闭，跳过淡出动画
                    m_MenuForm = null;
                }
            }
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_MenuForm = (MenuForm)ne.UIForm.Logic;
        }
    }
}
