using DG.Tweening;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class ScorePieceToken : Entity, IPieceToken
    {
        [SerializeField]
        private ScorePlaceTokenArea m_PlaceTokenArea = null;
        public IPlaceTokenArea OwnerPlaceTokenArea
        {
            get => m_PlaceTokenArea;
            set => m_PlaceTokenArea = value as ScorePlaceTokenArea;
        }

        private bool m_Interactable = false;
        public bool Interactable
        {
            get => false;
            set => m_Interactable = value;
        }

        public Transform Transform => CachedTransform;

        private Tween m_GotoAreaTween = null;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_GotoAreaTween = null;
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);

            if (m_GotoAreaTween != null)
            {
                m_GotoAreaTween.Kill();
                m_GotoAreaTween = null;
            }
        }

        public void GotoArea(IPlaceTokenArea area)
        {
            if (area == null)
            {
                Log.Warning("Target area is invalid.");
                return;
            }

            if (OwnerPlaceTokenArea != null)
            {
                OwnerPlaceTokenArea.RemoveToken();
                OwnerPlaceTokenArea = null;
            }
            OwnerPlaceTokenArea = area;

            var curPos = Transform.position;
            if (Vector3.Distance(curPos, area.PlaceDestination) < 0.01f)
                return;

            Interactable = false;
            m_GotoAreaTween = Transform.DOMove(area.PlaceDestination, 0.5f).SetEase(Ease.InOutSine);
            m_GotoAreaTween.onKill += () =>
            {
                Interactable = true;
                Transform.position = area.PlaceDestination;
            };
        }
    }
}