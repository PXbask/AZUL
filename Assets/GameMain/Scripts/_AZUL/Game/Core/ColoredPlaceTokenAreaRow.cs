using System;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    /// <summary>
    /// 用于在 Inspector 中序列化 List&lt;ColoredPlaceTokenArea&gt; 的包装类
    /// </summary>
    [Serializable]
    public class ColoredPlaceTokenAreaRow
    {
        public List<ColoredPlaceTokenArea> Areas = new List<ColoredPlaceTokenArea>();
    }
}
