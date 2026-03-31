using DG.Tweening;
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

        private Tween m_SelectTween = null;
        private Tween m_DeselectTween = null;
        private Tween m_ResetMoveTween = null;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_SelectTween = null;
            m_DeselectTween = null;
            m_ResetMoveTween = null;
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_PieceTokenData = userData as PieceTokenData;
            if (m_PieceTokenData == null)
            {
                //Log.Error("PieceToken data is invalid.");
                return;
            }

            var dataBinding = GetComponent<PieceTokenDataBinding>();
            var mat = dataBinding.GetMaterial(m_PieceTokenData.ColorType);
            var renderer = GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);

            if (m_SelectTween != null)
            {
                m_SelectTween.Kill();
                m_SelectTween=null;
            }
            if(m_DeselectTween != null)
            {
                m_DeselectTween.Kill();
                m_DeselectTween=null;
            }
        }

        public virtual void OnSelected()
        {
            //表现：向上移动一定距离
            var area = OwnerPlaceTokenArea;
            if (area != null)
            {
                if(m_DeselectTween != null)
                {
                    m_DeselectTween.Kill();
                    m_DeselectTween = null;
                }
                var endPos = area.PlaceDestination + Vector3.up * 0.2f;
                
                m_SelectTween = CachedTransform.DOMove(endPos, 0.2f);
            }
        }

        public virtual void OnDeselected()
        {
            var area = OwnerPlaceTokenArea;
            if (area != null)
            {
                if (m_SelectTween != null)
                {
                    m_SelectTween.Kill();
                    m_DeselectTween = null;
                }
                var endPos = area.PlaceDestination;

                m_SelectTween = CachedTransform.DOMove(endPos, 0.2f);
            }
        }
    }
}