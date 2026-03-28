using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AZUL
{
    public class PlaceTokenArea : MonoBehaviour
    {
        [SerializeField]
        protected PieceToken token;

        public PieceToken Token { get { return token; } }

        [SerializeField]
        protected PlaceTokenPosition positionGroup;

        public PlaceTokenPosition PositionGroup { get { return positionGroup; } set { positionGroup = value; } }

        [SerializeField]
        protected PlaceAreaCamp camp;

        public PlaceAreaCamp Camp { get { return camp; } set { camp = value; } }

        [SerializeField]
        protected int Row;

        [SerializeField]
        protected int Column;

        public Vector3 PlaceDestination
        {
            get
            {
                return transform.position + Vector3.up * 0.02f;
            }
        }

        protected virtual void Awake()
        {
            token = null;
        }

        public void PlaceToken(PieceToken pieceToken)
        {
            if (pieceToken.OwnerPlaceTokenArea != null)
            {
                pieceToken.OwnerPlaceTokenArea.token = null;
            }
            token = pieceToken;
            pieceToken.OwnerPlaceTokenArea = this;

            var curPos = pieceToken.CachedTransform.position;
            if(Vector3.Distance(curPos, PlaceDestination) < 0.01f)
            {
                return;
            }

            pieceToken.CanInteractive = false;
            pieceToken.CachedTransform.DOMove(PlaceDestination, 0.5f).SetEase(Ease.InOutSine)
                .OnComplete(() => pieceToken.CanInteractive = true);
        }

        public bool IsEmpty()
        {
            return token == null;
        }

        public virtual PlaceTokenAreaPosition GetPositionData()
        {
            return new PlaceTokenAreaPosition
            {
                PositionGroup = positionGroup,
                Row = Row,
                Column = Column,
            };
        }

        public void RemoveToken()
        {
            token = null;
        }
    }
}
