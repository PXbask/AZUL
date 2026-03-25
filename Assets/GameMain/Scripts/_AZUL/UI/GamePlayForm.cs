using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class GamePlayForm : UGuiForm
    {
        private ProcedureMain m_ProcedureMain = null;

        public void OnStartButtonClick()
        {
            m_ProcedureMain.StartGame();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_ProcedureMain = (ProcedureMain)userData;
            if (m_ProcedureMain == null)
            {
                Log.Warning("ProcedureMain is invalid when open GamePlayForm.");
                return;
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            m_ProcedureMain = null;

            base.OnClose(isShutdown, userData);
        }
    }
}
