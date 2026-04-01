using DG.Tweening;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public interface IPieceToken
    {
        IPlaceTokenArea OwnerPlaceTokenArea { get; set; }

        bool Interactable { get; set; }

        Transform Transform { get; }
    }

    public class PieceToken : Entity, IPieceToken
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
        private IPlaceTokenArea m_PlaceTokenArea = null;
        public IPlaceTokenArea OwnerPlaceTokenArea
        {
            get => m_PlaceTokenArea;
            set => m_PlaceTokenArea = value;
        }

        [SerializeField]
        private bool m_Interactable = true;
        public bool Interactable
        {
            get => m_Interactable;
            set=> m_Interactable = value;
        }

        public Transform Transform => CachedTransform;

        private Tween m_SelectTween = null;
        private Tween m_DeselectTween = null;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_SelectTween = null;
            m_DeselectTween = null;
        }

        protected override void OnShow(object userData)
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

        public void PlaySelectAnim()
        {
            //表现：向上移动一定距离
            if (OwnerPlaceTokenArea != null)
            {
                if(m_DeselectTween != null)
                {
                    m_DeselectTween.Kill();
                    m_DeselectTween = null;
                }
                var endPos = OwnerPlaceTokenArea.PlaceDestination + Vector3.up * 0.2f;
                
                m_SelectTween = CachedTransform.DOMove(endPos, 0.2f);
            }
        }

        public void PlayDeselectAnim()
        {
            if (OwnerPlaceTokenArea != null)
            {
                if (m_SelectTween != null)
                {
                    m_SelectTween.Kill();
                    m_DeselectTween = null;
                }
                var endPos = OwnerPlaceTokenArea.PlaceDestination;

                m_SelectTween = CachedTransform.DOMove(endPos, 0.2f);
            }
        }
    }
}