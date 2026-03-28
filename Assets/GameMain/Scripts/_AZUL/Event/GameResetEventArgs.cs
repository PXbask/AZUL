using GameFramework;
using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public sealed class GameResetEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(GameResetEventArgs).GetHashCode();

        public GameResetEventArgs()
        {
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public static GameResetEventArgs Create()
        {
            GameResetEventArgs e = ReferencePool.Acquire<GameResetEventArgs>();
            return e;
        }

        public override void Clear()
        {
        }
    }
}
