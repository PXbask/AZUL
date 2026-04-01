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
    public sealed class DealPiecesDoneEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DealPiecesDoneEventArgs).GetHashCode();

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static DealPiecesDoneEventArgs Create()
        {
            DealPiecesDoneEventArgs e = ReferencePool.Acquire<DealPiecesDoneEventArgs>();
            return e;
        }

        public override void Clear()
        {

        }
    }
}
