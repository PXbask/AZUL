using GameFramework;
using GameFramework.Event;
using StarForce;

namespace AZUL
{
    /// <summary>
    /// 棋子移动完成事件
    /// </summary>
    public sealed class MovePieceCompleteEventArgs : GameEventArgs
    {
        /// <summary>
        /// 棋子移动完成事件编号
        /// </summary>
        public static readonly int EventId = typeof(MovePieceCompleteEventArgs).GetHashCode();

        /// <summary>
        /// 初始化棋子移动完成事件的新实例
        /// </summary>
        public MovePieceCompleteEventArgs()
        {
            PieceToken = null;
            SourceArea = null;
            TargetArea = null;
            Camp = PlaceAreaCamp.Neutral;
        }

        /// <summary>
        /// 获取棋子移动完成事件编号
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取移动的棋子
        /// </summary>
        public PieceToken PieceToken
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取源放置区域
        /// </summary>
        public PlaceTokenArea SourceArea
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取目标放置区域
        /// </summary>
        public PlaceTokenArea TargetArea
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public PlaceAreaCamp Camp
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建棋子移动完成事件
        /// </summary>
        /// <param name="pieceToken">移动的棋子</param>
        /// <param name="sourceArea">源放置区域</param>
        /// <param name="targetArea">目标放置区域</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的棋子移动完成事件</returns>
        public static MovePieceCompleteEventArgs Create(PieceToken pieceToken, PlaceTokenArea sourceArea, PlaceTokenArea targetArea, PlaceAreaCamp camp)
        {
            MovePieceCompleteEventArgs e = ReferencePool.Acquire<MovePieceCompleteEventArgs>();
            e.PieceToken = pieceToken;
            e.SourceArea = sourceArea;
            e.TargetArea = targetArea;
            e.Camp = camp;
            return e;
        }

        /// <summary>
        /// 清理棋子移动完成事件
        /// </summary>
        public override void Clear()
        {
            PieceToken = null;
            SourceArea = null;
            TargetArea = null;
            Camp = PlaceAreaCamp.Neutral;
        }
    }
}
