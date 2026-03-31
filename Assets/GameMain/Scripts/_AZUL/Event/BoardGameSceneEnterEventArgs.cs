using GameFramework;
using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    /// <summary>
    /// 进入打牌场景事件
    /// </summary>
    public sealed class BoardGameSceneEnterEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(BoardGameSceneEnterEventArgs).GetHashCode();

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static BoardGameSceneEnterEventArgs Create()
        {
            BoardGameSceneEnterEventArgs e = ReferencePool.Acquire<BoardGameSceneEnterEventArgs>();
            return e;
        }

        public override void Clear()
        {
            
        }
    }
}
