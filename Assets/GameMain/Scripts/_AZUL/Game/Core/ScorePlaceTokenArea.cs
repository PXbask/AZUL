using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public class ScorePlaceTokenArea : PlaceTokenArea
    {
        [SerializeField]
        private int score;

        [SerializeField]
        protected ScorePieceToken scoreToken;

        public ScorePieceToken ScoreToken => scoreToken;

        public void PlaceToken(ScorePieceToken pieceToken)
        {
            if (pieceToken.OwnerPlaceTokenArea != null)
            {
                pieceToken.OwnerPlaceTokenArea.scoreToken = null;
            }
            scoreToken = pieceToken;
            pieceToken.OwnerPlaceTokenArea = this;

            var curPos = pieceToken.CachedTransform.position;
            if (Vector3.Distance(curPos, PlaceDestination) < 0.01f)
            {
                return;
            }

            pieceToken.CachedTransform.DOMove(PlaceDestination, 0.5f).SetEase(Ease.InOutSine);
        }

        public override void RemoveToken()
        {
            scoreToken = null;
        }
    }
}
