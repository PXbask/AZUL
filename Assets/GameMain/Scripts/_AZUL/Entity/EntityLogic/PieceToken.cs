using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class PieceToken : Entity
    {
        [SerializeField]
        private PieceTokenData m_PieceTokenData = null;

        public PieceTokenData PieceTokenData
        {
            get
            {
                return m_PieceTokenData;
            }
        }

        [SerializeField]
        private bool m_CanInteractive = true;
        public bool CanInteractive
        {
            get => m_CanInteractive;
            set=> m_CanInteractive = value;
        }

        [SerializeField]
        private PlaceTokenArea m_PlaceTokenArea = null;
        public PlaceTokenArea OwnerPlaceTokenArea
        {
            get => m_PlaceTokenArea;
            set => m_PlaceTokenArea = value;
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_PieceTokenData = userData as PieceTokenData;
            if (m_PieceTokenData == null)
            {
                Log.Error("PieceToken data is invalid.");
                return;
            }

            var dataBinding = GetComponent<PieceTokenDataBinding>();
            var mat = dataBinding.GetMaterial(m_PieceTokenData.ColorType);
            var renderer = GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}