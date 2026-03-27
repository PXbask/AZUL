using GameFramework.DataTable;
using StarForce;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    [Serializable]
    public class PieceTokenData : EntityData
    {
        [SerializeField]
        private PieceColorType colorType;

        public PieceColorType ColorType
        {
            get
            {
                return colorType;
            }
        }

        [SerializeField]
        private int pieceId;
        public int PieceId => pieceId;

        /// <summary>
        /// 目标放置区域（可选）
        /// </summary>
        public PlaceTokenArea TargetArea { get; set; }

        public PieceTokenData(int entityId, int pieceId) : base(entityId, 10000)
        {
            this.pieceId = pieceId;
            IDataTable<DRPiece> dtPiece = GameEntry.DataTable.GetDataTable<DRPiece>();
            DRPiece drPiece = dtPiece.GetDataRow(pieceId);
            if (drPiece == null)
            {
                return;
            }

            colorType = drPiece.Color;
        }
    }
}
