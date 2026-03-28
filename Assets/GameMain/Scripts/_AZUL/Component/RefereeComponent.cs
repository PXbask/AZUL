using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class RefereeComponent : GameFrameworkComponent
    {
        private bool m_Active = false;

        [SerializeField]
        private TextMeshProUGUI tipText;

        protected override void Awake()
        {
            base.Awake();
            m_Active = false;
        }

        public void GameInit()
        {
            m_Active = true;
            tipText = GameObject.Find("Referee/Canvas/TipText").GetComponent<TextMeshProUGUI>();
        }

        public void ShowTip(string tipText)
        {
            if (!m_Active)
            {
                Log.Error("Referee is not active.");
                return;
            }
            this.tipText.text = tipText;
            Log.Info("Referee shows tip: {0}", tipText);
        }
    }
}
