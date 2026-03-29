using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AZUL
{
    public interface IPlaceTokenArea
    {
        /// <summary>
        /// 表明区域的位置信息,位于哪块区域
        /// </summary>
        PlaceTokenPosition PositionGroup { get; set; }

        /// <summary>
        /// 区域的阵营信息,表明该区域属于哪个玩家
        /// </summary>
        PlaceAreaCamp Camp { get; set; }

        /// <summary>
        /// 上面的棋子需要放置的位置
        /// </summary>
        Vector3 PlaceDestination { get; }
        
        /// <summary>
        /// 获取上面的棋子
        /// </summary>
        PieceToken Token { get; }

        /// <summary>
        /// 判断该区域是否为空
        /// </summary>
        /// <returns></returns>
        bool IsEmpty();

        /// <summary>
        /// 获取详细的位置信息,包含所在区域,行列信息
        /// </summary>
        /// <returns></returns>
        PlaceTokenAreaPosition GetPositionData();

        /// <summary>
        /// 移除上面的棋子
        /// </summary>
        void RemoveToken();
    }
    public class PlaceTokenArea : MonoBehaviour, IPlaceTokenArea
    {
        [SerializeField]
        protected PieceToken token;

        public PieceToken Token => token;

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

        public virtual void RemoveToken()
        {
            token = null;
        }
    }
}
