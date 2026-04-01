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
    }
}