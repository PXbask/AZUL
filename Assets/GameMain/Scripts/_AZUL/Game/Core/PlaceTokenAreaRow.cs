using System;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    /// <summary>
    /// 用于在 Inspector 中序列化 List&lt;PlaceTokenArea&gt; 的包装类
    /// </summary>
    [Serializable]
    public class PlaceTokenAreaRow
    {
        public List<PlaceTokenArea> Areas = new List<PlaceTokenArea>();
    }
}
