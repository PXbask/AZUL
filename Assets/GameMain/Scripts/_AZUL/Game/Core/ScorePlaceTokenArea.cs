using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public sealed class ScorePlaceTokenArea : BasePlaceTokenArea
    {
        [SerializeField]
        private int m_Score;

        [SerializeField]
        private ScorePieceToken m_Token;

        public override IPieceToken Token
        {
            get { return m_Token; }
            protected set { m_Token = value as ScorePieceToken; }
        }

        public override Vector3 PlaceDestination => transform.position + Vector3.up * 0.02f;

        protected override void Awake()
        {
            base.Awake();
            Token = null;
        }

        public override void PlaceToken(IPieceToken pieceToken)
        {
            if (pieceToken is not ScorePieceToken)
            {
                Debug.LogError($"PlaceTokenArea can only place ScorePieceToken, but got {pieceToken.GetType()}");
                return;
            }

            base.PlaceToken(pieceToken);
        }
    }
}
