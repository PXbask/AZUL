using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public struct AIAction
    {
        /// <summary>
        /// -1是中间区域，否则是工厂圆盘编号
        /// </summary>
        public int sourceId;

        /// <summary>
        /// 需要移动的棋子的颜色
        /// </summary>
        public PieceColorType color;

        /// <summary>
        /// -1是弃牌区，否则是花砖区行编号
        /// </summary>
        public int destinationId;
    }
}
