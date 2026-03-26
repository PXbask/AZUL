using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    /// <summary>
    /// 工厂圆盘 - 用于放置和展示棋子
    /// </summary>
    [Serializable]
    public class FactoryDisk
    {
        /// <summary>
        /// 当前圆盘上的棋子放置区域
        /// </summary>
        public List<PlaceTokenArea> TokenAreas = new List<PlaceTokenArea>();
    }
}
