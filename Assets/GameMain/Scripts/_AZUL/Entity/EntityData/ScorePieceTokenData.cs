using GameFramework.DataTable;
using StarForce;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    [Serializable]
    public class ScorePieceTokenData : EntityData
    {
        /// <summary>
        /// 目标放置区域（可选）
        /// </summary>
        public ScorePlaceTokenArea TargetArea { get; set; }

        public ScorePieceTokenData(int entityId) : base(entityId, 10001)
        {
            
        }
    }
}
