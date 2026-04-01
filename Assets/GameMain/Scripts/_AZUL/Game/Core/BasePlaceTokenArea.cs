using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public interface IPlaceTokenArea
    {
        /// <summary>
        /// 区域的阵营信息,表明该区域属于哪个玩家
        /// </summary>
        PlaceAreaCamp Camp { get; }

        /// <summary>
        /// 上面的棋子需要放置的位置
        /// </summary>
        Vector3 PlaceDestination { get; }

        /// <summary>
        /// 获取上面的棋子
        /// </summary>
        IPieceToken Token { get; }

        /// <summary>
        /// 判断该区域是否为空
        /// </summary>
        /// <returns></returns>
        bool IsEmpty();

        /// <summary>
        /// 放置棋子
        /// </summary>
        void PlaceToken(IPieceToken pieceToken);

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

    public class BasePlaceTokenArea : MonoBehaviour, IPlaceTokenArea
    {
        [SerializeField]
        protected int m_Row;

        [SerializeField] 
        protected int m_Column;

        [SerializeField]
        protected PlaceTokenPositionGroup m_PositionGroup;
        public PlaceTokenPositionGroup PositionGroup
        {
            set { m_PositionGroup = value; }
        }

        [SerializeField]
        protected PlaceAreaCamp m_Camp;
        public PlaceAreaCamp Camp
        {
            get { return m_Camp; }
            set { m_Camp = value; }
        }

        public virtual Vector3 PlaceDestination => transform.position;

        public virtual IPieceToken Token { get; protected set; }

        private Tween m_PlaceTokenTween = null;

        protected virtual void Awake()
        {
            Token = null;
            m_PlaceTokenTween = null;
        }

        public bool IsEmpty()
        {
            return Token == null;
        }

        public virtual void PlaceToken(IPieceToken pieceToken)
        {
            if (m_PlaceTokenTween != null)
            {
                m_PlaceTokenTween.Kill();
                m_PlaceTokenTween = null;
            }

            if (pieceToken.OwnerPlaceTokenArea != null)
            {
                pieceToken.OwnerPlaceTokenArea.RemoveToken();
            }
            Token = pieceToken;
            pieceToken.OwnerPlaceTokenArea = this;

            var curPos = pieceToken.Transform.position;
            if (Vector3.Distance(curPos, PlaceDestination) < 0.01f)
            {
                return;
            }

            pieceToken.Interactable = false;
            m_PlaceTokenTween = pieceToken.Transform.DOMove(PlaceDestination, 0.5f).SetEase(Ease.InOutSine);
            m_PlaceTokenTween.onKill += () =>
            {
                pieceToken.Interactable = true;
                pieceToken.Transform.position = PlaceDestination;
            };
        }

        public PlaceTokenAreaPosition GetPositionData()
        {
            return new PlaceTokenAreaPosition()
            {
                PositionGroup = m_PositionGroup,
                Row = m_Row,
                Column = m_Column
            };
        }

        public void RemoveToken()
        {
            if (Token != null)
            {
                Token.OwnerPlaceTokenArea = null;
                Token = null;
            }
        }
    }
}
