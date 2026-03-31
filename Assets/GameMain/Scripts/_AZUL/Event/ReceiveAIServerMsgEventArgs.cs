using GameFramework;
using GameFramework.Event;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public class ReceiveAIServerMsgEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ReceiveAIServerMsgEventArgs).GetHashCode();

        public override int Id => EventId;

        public AIAction AIAction { get; set; }

        public ReceiveAIServerMsgEventArgs()
        {
            AIAction = default;
        }

        public static ReceiveAIServerMsgEventArgs Create(string json)
        {
            ReceiveAIServerMsgEventArgs e = ReferencePool.Acquire<ReceiveAIServerMsgEventArgs>();
            e.AIAction = JsonMapper.ToObject<AIAction>(json);
            return e;
        }

        public override void Clear()
        {
            AIAction = default;
        }
    }
}
