using DG.Tweening;
using GameFramework.DataTable;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGameFramework.Runtime;
using LitJson;

namespace AZUL
{
    public class BoardGameComponent : GameFrameworkComponent
    {
        [SerializeField]
        private bool m_Running = false;

        [SerializeField]
        private bool m_HasRegisterEvent = false;

        /// <summary>
        /// 当前选中的棋子
        /// </summary>
        private PieceToken m_SelectedPieceToken = null;

        /// <summary>
        /// 主相机（用于射线检测）
        /// </summary>
        private Camera m_MainCamera;

        [Header("相机动画目的地")]
        [SerializeField]
        private Transform m_CameraDestinationTrans;
        [SerializeField]
        private string m_CameraDestinationTransPath;

        [Header("棋子袋")]
        [SerializeField]
        private Transform m_PieceBag;
        [SerializeField]
        private string m_PieceBagPath;

        [Header("玩家棋盘")]
        [SerializeField]
        private PlayerBoard m_SelfBoard;
        [SerializeField]
        private string m_SelfBoardPath;

        [Header("对手棋盘")]
        [SerializeField]
        private PlayerBoard m_OtherBoard;
        [SerializeField]
        private string m_OtherBoardPath;

        [Header("中间公共棋盘")]
        [SerializeField]
        private MidBoard m_MidBoard;
        [SerializeField]
        private string m_MidBoardPath;

        /// <summary>
        /// 剩余的棋子Id
        /// </summary>
        private List<int> RemainPieceIds = new List<int>();

        /// <summary>
        /// 放入弃牌区的棋子Id
        /// </summary>
        private List<int> LostPieceIds = new List<int>();

        public MidBoard MidBoard => m_MidBoard;

        public readonly int PlayerNum = 2;
        public readonly int FactoryDiskNum = 5;

        private PlaceAreaCamp m_CurrentPlayer;
        public PlaceAreaCamp CurrentPlayer
        {
            get { return m_CurrentPlayer; }
            set
            { 
                m_CurrentPlayer = value; 
                if(value == PlaceAreaCamp.Other && FightwithAI)
                {
                    m_Interactive = false;
                }
                else
                {
                    m_Interactive = true;
                }
            }
        }

        public bool m_Interactive { get; set; }
        public bool FightwithAI => GameEntry.AI.IsRunning();

        private Sequence m_ClearTableSequence = null;

        protected override void Awake()
        {
            base.Awake();
            m_Running = false;
            m_HasRegisterEvent = false;

            m_Interactive = false;

            m_ClearTableSequence = null;
        }

        #region 事件订阅与取消
        private void SubscribeEvents()
        {
            // 订阅实体显示成功事件（在 Start 中确保 GameEntry 已初始化）
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);

            GameEntry.Event.Subscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
        }

        private void UnsubscribeEvents()
        {
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(BoardGameSceneEnterEventArgs.EventId, OnBoardGameSceneEnter);
        }

        /// <summary>
        /// 实体显示成功回调
        /// </summary>
        private void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;

            // 检查是否是我们创建的棋子实体
            if (ne.EntityLogicType == typeof(PieceToken))
            {
                PieceToken pieceToken = (PieceToken)ne.Entity.Logic;
                PieceTokenData data = (PieceTokenData)ne.UserData;

                // 在这里处理获取到的实体
                Log.Info("PieceToken created successfully. EntityId: {0}, TypeId: {1}", pieceToken.Id, data.TypeId);

                // 如果需要将棋子放置到特定区域
                if (data.TargetArea != null)
                {
                    data.TargetArea.PlaceToken(pieceToken);
                }
            }

            // 如果需要处理其他类型的实体，可以在这里添加更多的条件分支
            if (ne.EntityLogicType == typeof(ScorePieceToken))
            {
                ScorePieceToken pieceToken = (ScorePieceToken)ne.Entity.Logic;
                ScorePieceTokenData data = (ScorePieceTokenData)ne.UserData;

                Log.Info("ScorePieceToken created successfully. EntityId: {0}, TypeId: {1}", pieceToken.Id, data.TypeId);
                // 如果需要将棋子放置到特定区域
                if (data.TargetArea != null)
                {
                    data.TargetArea.PlaceToken(pieceToken);
                }
                //指定玩家的分数token
                if (data.TargetArea.Camp == PlaceAreaCamp.Self)
                {
                    m_SelfBoard.ScorePieceToken = pieceToken;
                }
                else if (data.TargetArea.Camp == PlaceAreaCamp.Other)
                {
                    m_OtherBoard.ScorePieceToken = pieceToken;
                }
            }
        }

        /// <summary>
        /// 当刚刚进入游戏场景时
        /// </summary>
        private void OnBoardGameSceneEnter(object sender, GameEventArgs e)
        {
            m_Running = true;
            RegisterSceneObjects();
        }
        #endregion

        private void OnDisable()
        {
            // 取消订阅事件
            if (GameEntry.Event != null)
            {
                UnsubscribeEvents();
            }
        }

        private void Update()
        {
            //注册事件
            if (!m_HasRegisterEvent)
            {
                if(GameEntry.Event != null)
                {
                    SubscribeEvents();
                    m_HasRegisterEvent= true;
                }
            }

            if (!m_Running) return;

            // 检测鼠标左键点击
            if (!m_Interactive) return;
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        public PlayerBoard GetPlayerBoard(PlaceAreaCamp camp)
        {
            if(camp == PlaceAreaCamp.Self)
            {
                return m_SelfBoard;
            }
            else if(camp == PlaceAreaCamp.Other)
            {
                return m_OtherBoard;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 处理鼠标点击事件
        /// </summary>
        private void HandleMouseClick()
        {
            if (m_MainCamera == null)
            {
                Log.Warning("Main Camera is null, cannot perform raycast.");
                return;
            }

            if (m_SelectedPieceToken == null)
            {
                // 从鼠标位置发射射线
                Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 进行射线检测
                if (Physics.Raycast(ray, out hit))
                {
                    // 尝试获取点击物体上的 PieceToken 组件
                    PieceToken pieceToken = hit.collider.GetComponent<PieceToken>();

                    if (pieceToken != null && pieceToken.Interactable && pieceToken.OwnerPlaceTokenArea != null
                        && (pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.MidTable || pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.Factory))
                    {
                        // 找到了 PieceToken，保存到缓存中
                        SelectPieceToken(pieceToken);

                        Log.Info("PieceToken selected. EntityId: {0}, Position: {1}",
                            pieceToken.Id,
                            pieceToken.transform.position);
                    }
                }
            }
            else
            {
                // 已经有选中的棋子了，检测点击的区域是否是合法的放置区域
                Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 进行射线检测
                if (Physics.Raycast(ray, out hit))
                {
                    // 尝试获取点击物体上的 PlaceTokenArea 组件（包括其子类）
                    PlaceTokenArea placeArea = hit.collider.GetComponent<PlaceTokenArea>();

                    if (placeArea != null)
                    {
                        // 找到了放置区域，进行后续处理
                        Log.Info("PlaceTokenArea detected. Type: {0}, Position: {1}",
                            placeArea.GetType().Name,
                            placeArea.transform.position);

                        // 处理放置逻辑
                        OnPlaceTokenToArea(m_SelectedPieceToken, placeArea);
                    }
                    else
                    {
                        // 点击的不是有效的放置区域，取消选中
                        Log.Info("Invalid placement area. Deselecting piece.");
                        ClearSelectedPieceTokenWithAnim();
                    }
                }
                else
                {
                    // 没有点击到任何物体，取消选中
                    ClearSelectedPieceTokenWithAnim();
                }
            }
        }

        /// <summary>
        /// 选中对应棋子
        /// </summary>
        /// <param name="pieceToken"></param>
        private void SelectPieceToken(PieceToken pieceToken)
        {
            m_SelectedPieceToken = pieceToken;
            pieceToken.PlaySelectAnim();
        }

        /// <summary>
        /// 获取当前选中的棋子
        /// </summary>
        public PieceToken GetSelectedPieceToken()
        {
            return m_SelectedPieceToken;
        }

        /// <summary>
        /// 清除当前选中的棋子,有动画
        /// </summary>
        public void ClearSelectedPieceTokenWithAnim()
        {
            if (m_SelectedPieceToken != null)
            {
                m_SelectedPieceToken.PlayDeselectAnim();
                Log.Info("Cleared selected piece. EntityId: {0}", m_SelectedPieceToken.Id);
                m_SelectedPieceToken = null;
            }
        }

        /// <summary>
        /// 清除当前选中的棋子
        /// </summary>
        public void ClearSelectedPieceToken()
        {
            if (m_SelectedPieceToken != null)
            {
                Log.Info("Cleared selected piece. EntityId: {0}", m_SelectedPieceToken.Id);
                m_SelectedPieceToken = null;
            }
        }

        /// <summary>
        /// 将选中的棋子放置到指定区域
        /// </summary>
        private void OnPlaceTokenToArea(PieceToken pieceToken, PlaceTokenArea targetArea)
        {
            //=========检查是否可以放置==========
            var posData = targetArea.GetPositionData();
            //如果目的地不是本轮玩家侧的区域，就不能放了；
            if (CurrentPlayer != targetArea.Camp)
            {
                Log.Info("Cannot place piece on opponent's area. Current player: {0}, Target area camp: {1}",
                    CurrentPlayer, targetArea.Camp);
                ClearSelectedPieceTokenWithAnim();
                return;
            }
            //如果目的地不是手动区域或地板区域，就不能放了；
            if (posData.PositionGroup != PlaceTokenPositionGroup.Manual && posData.PositionGroup != PlaceTokenPositionGroup.Lose)
            {
                Log.Info("Cannot place piece on this area. Position group: {0}", posData.PositionGroup);
                ClearSelectedPieceTokenWithAnim();
                return;
            }

            //目标位置是手动区域
            if (posData.PositionGroup == PlaceTokenPositionGroup.Manual)
            {
                //如果是手动区域并且对应的颜色区有这个颜色的棋子了，就不能放了；
                if (BoardGameUtility.PlayerBoardHasColorInColoredAreaInRow(GetBoardWithCurrentComp(), posData.Row, pieceToken.PieceTokenData.ColorType))
                {
                    Log.Info("Cannot place piece in manual area because the colored area in the same row already has a piece of the same color.");
                    ClearSelectedPieceTokenWithAnim();
                    return;
                }
                //如果是手动区域并且放置区不是这个颜色的棋子，就不能放了
                if (BoardGameUtility.PlayerBoardDiffColorInManualAreaInRow(GetBoardWithCurrentComp(), posData.Row, pieceToken.PieceTokenData.ColorType))
                {
                    Log.Info("Cannot place piece in manual area because the manual area in the same row has a piece of a different color.");
                    ClearSelectedPieceTokenWithAnim();
                    return;
                }
                //可以放置了,获取选择棋子所在的工厂圆盘所有的相同颜色棋子，从左到右放到对应行，多余的棋子放入减分区；工厂圆盘剩余棋子放入中间区域
                //如果棋子不在工厂圆盘，需要把首位token放入减分区
                if (pieceToken.OwnerPlaceTokenArea != null)
                {
                    //位于工厂区域
                    if (pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.Factory)
                    {
                        var remainTokens = new List<PieceToken>();
                        var allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInFactory(pieceToken, out remainTokens);
                        var leftAreas = BoardGameUtility.GetEmptyTokenAreaInManualAreaInRow(GetBoardWithCurrentComp(), posData.Row);
                        var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                        MovePieceListToManualSubLoseArea(allSameColorTokens, leftAreas, loseAreas);
                        //将工厂圆盘内剩余token放入中间区域
                        int remainCount = remainTokens.Count;
                        var midList = BoardGameUtility.GetEmptyTokenAreaInMidArea(remainCount);
                        for (int i = 0; i < remainCount; i++)
                        {
                            midList[i].PlaceToken(remainTokens[i]);
                        }
                    }
                    else if (pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.MidTable)
                    {
                        //位于中间区域
                        var firstToken = BoardGameUtility.GetFirstTokenInMidArea();
                        if (firstToken != null)
                        {
                            //需要把首位token放入减分区
                            var loseAreas_fiestToken = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                            //MovePieceToSubLoseArea(new List<PieceToken> { firstToken }, loseAreas_fiestToken);
                            MoveFirstTokenToSub(firstToken);
                        }
                        var allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInMidTable(pieceToken);
                        var leftAreas = BoardGameUtility.GetEmptyTokenAreaInManualAreaInRow(GetBoardWithCurrentComp(), posData.Row);
                        var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                        MovePieceListToManualSubLoseArea(allSameColorTokens, leftAreas, loseAreas);
                    }
                }
            }
            //目标位置是减分区域
            else
            {
                if (pieceToken.OwnerPlaceTokenArea != null)
                {
                    //位于工厂区域
                    if (pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.Factory)
                    {
                        var remainTokens = new List<PieceToken>();
                        var allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInFactory(pieceToken, out remainTokens);
                        var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                        MovePieceToSubLoseArea(allSameColorTokens, loseAreas);
                        //将工厂圆盘内剩余token放入中间区域
                        int remainCount = remainTokens.Count;
                        var midList = BoardGameUtility.GetEmptyTokenAreaInMidArea(remainCount);
                        for (int i = 0; i < remainCount; i++)
                        {
                            midList[i].PlaceToken(remainTokens[i]);
                        }
                    }
                    //位于中间区域
                    else if (pieceToken.OwnerPlaceTokenArea.GetPositionData().PositionGroup == PlaceTokenPositionGroup.MidTable)
                    {
                        var firstToken = BoardGameUtility.GetFirstTokenInMidArea();
                        if (firstToken != null)
                        {
                            //需要把首位token放入减分区
                            var loseAreas_fiestToken = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                            //MovePieceToSubLoseArea(new List<PieceToken> { firstToken }, loseAreas_fiestToken);
                            MoveFirstTokenToSub(firstToken);
                        }
                        var allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInMidTable(pieceToken);
                        var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                        MovePieceToSubLoseArea(allSameColorTokens, loseAreas);
                    }
                }
            }

            var tmpPlayer = CurrentPlayer;
            SwitchPlayer();
            GameEntry.Event.Fire(this, MovePieceCompleteEventArgs.Create(null, null, null, tmpPlayer));
            ClearSelectedPieceToken();
        }

        public void RegisterSceneObjects()
        {
            // 获取主相机
            m_MainCamera = Camera.main;
            if (m_MainCamera == null)
            {
                Log.Error("Main Camera not found. Raycast detection will not work.");
            }

            if (m_CameraDestinationTrans == null)
            {
                var obj = GameObject.Find(m_CameraDestinationTransPath);
                if(obj != null)
                {
                    m_CameraDestinationTrans = obj.transform;
                }
                else
                {
                    Log.Error("Cant find m_CameraDestinationTrans object");
                }
            }

            if (m_PieceBag == null)
            {
                var obj = GameObject.Find(m_PieceBagPath);
                if (obj != null)
                {
                    m_PieceBag = obj.transform;
                }
                else
                {
                    Log.Error("Cant find m_PieceBag object");
                }
            }

            if (m_SelfBoard == null)
            {
                var obj = GameObject.Find(m_SelfBoardPath);
                if (obj != null)
                {
                    m_SelfBoard = obj.GetComponent<PlayerBoard>();
                }
                else
                {
                    Log.Error("Cant find m_SelfBoard object");
                }
            }

            if (m_OtherBoard == null)
            {
                var obj = GameObject.Find(m_OtherBoardPath);
                if (obj != null)
                {
                    m_OtherBoard = obj.GetComponent<PlayerBoard>();
                }
                else
                {
                    Log.Error("Cant find m_OtherBoard object");
                }
            }

            if (m_MidBoard == null)
            {
                var obj = GameObject.Find(m_MidBoardPath);
                if (obj != null)
                {
                    m_MidBoard = obj.GetComponent<MidBoard>();
                }
                else
                {
                    Log.Error("Cant find m_MidBoard object");
                }
            }
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void GameReset()
        {
            m_Interactive = false;

            //动画表现：棋子飞回棋子袋并隐藏
            ClearAllPieceTokens();

            RemainPieceIds.Clear();
            LostPieceIds.Clear();
            IDataTable<DRPiece> dtPiece = GameEntry.DataTable.GetDataTable<DRPiece>();
            foreach (DRPiece piece in dtPiece.GetAllDataRows())
            {
                RemainPieceIds.Add(piece.Id);
            }

            //默认先手玩家为自己
            SetCurrentPlayer(PlaceAreaCamp.Self);

            m_SelfBoard.GameReset();
            m_OtherBoard.GameReset();
        }

        /// <summary>
        /// 游戏开始时发牌
        /// </summary>
        public void DealPiece()
        {
            m_Interactive = false;
            if (!m_Running)
            {
                Log.Error("BoardGameComponent is not active. Please call GameReset() before dealing pieces.");
                return;
            }

            //生成分数token
            if(m_SelfBoard.ScorePieceToken == null)
            {
                var zeroScoreArea_self = BoardGameUtility.GetScorePlaceTokenArea(m_SelfBoard, 0);
                SpawnScorePieceAndPlace(zeroScoreArea_self);
            }
            if (m_OtherBoard.ScorePieceToken == null)
            {
                var zeroScoreArea_other = BoardGameUtility.GetScorePlaceTokenArea(m_OtherBoard, 0);
                SpawnScorePieceAndPlace(zeroScoreArea_other);
            }

            if (RemainPieceIds.Remove(0))  // 假设0是一个特殊的棋子ID，代表空棋子或占位符，不需要发牌
            {
                SpawnPieceAndPlace(0, GetRemainSlotFromMidDisk());
            }

            var midDiskSlots = m_MidBoard.FactoryDisks.SelectMany(disk => disk.TokenAreas).ToList();
            if(RemainPieceIds.Count < midDiskSlots.Count)
            {
                //将弃牌区棋子全部放回棋子袋
                RemainPieceIds.AddRange(LostPieceIds);
                LostPieceIds.Clear();
            }
            var randomTokens = TakeRandomPieces(midDiskSlots.Count);
            for (int i = 0; i < randomTokens.Count; i++)
            {
                SpawnPieceAndPlace(randomTokens[i], midDiskSlots[i]);
            }
        }

        /// <summary>
        /// 生成棋子并放置到指定区域
        /// </summary>
        private void SpawnPieceAndPlace(int pieceId, PlaceTokenArea placeTokenArea)
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            GameEntry.Entity.ShowPieceToken(new PieceTokenData(entityId, pieceId)
            {
                Position = m_PieceBag.position,
                TargetArea = placeTokenArea,  // 保存目标区域，在事件回调中使用
            });
        }

        private void SpawnScorePieceAndPlace(ScorePlaceTokenArea placeTokenArea)
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            GameEntry.Entity.ShowScorePieceToken(new ScorePieceTokenData(entityId)
            {
                Position = m_PieceBag.position,
                TargetArea = placeTokenArea,  // 保存目标区域，在事件回调中使用
            });
        }

        private PlaceTokenArea GetRemainSlotFromMidDisk()
        {
            // 获取中央棋盘上的工厂圆盘列表
            foreach (var area in m_MidBoard.CenterTokenAreas)
            {
                // 如果该区域没有棋子，则返回这个区域
                if (area.IsEmpty())
                {
                    return area;
                }
            }

            // 如果所有工厂圆盘上的区域都已被占用，则返回 null
            return null;
        }

        /// <summary>
        /// 从 RemainPieceIds 中随机取出指定数量的元素，并从原列表中移除
        /// </summary>
        /// <param name="count">要取出的元素数量</param>
        /// <returns>随机抽取的子列表</returns>
        private List<int> TakeRandomPieces(int count)
        {
            // 如果请求数量超过剩余数量，则只取剩余的全部
            int actualCount = Mathf.Min(count, RemainPieceIds.Count);

            List<int> result = new List<int>(actualCount);

            // 随机抽取 n 次
            for (int i = 0; i < actualCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, RemainPieceIds.Count);
                result.Add(RemainPieceIds[randomIndex]);
                RemainPieceIds.RemoveAt(randomIndex);
            }

            return result;
        }

        public void SetCurrentPlayer(PlaceAreaCamp player)
        {
            CurrentPlayer = player;
            Log.Info("Current player set to: {0}", CurrentPlayer);
        }

        public PlayerBoard GetBoardWithCurrentComp()
        {
            if (CurrentPlayer == PlaceAreaCamp.Self)
            {
                return m_SelfBoard;
            }
            else
            {
                return m_OtherBoard;
            }
        }

        /// <summary>
        /// 将当前棋子放入弃牌区，并播放动画
        /// </summary>
        /// <param name="pieceToken"></param>
        public void LosePiece(PieceToken pieceToken)
        {
            pieceToken.Interactable = false;
            if (pieceToken.OwnerPlaceTokenArea != null)
            {
                pieceToken.OwnerPlaceTokenArea.RemoveToken();
                pieceToken.OwnerPlaceTokenArea = null;
            }

            pieceToken.CachedTransform.DOMove(m_PieceBag.position, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                GameEntry.Entity.HideEntity(pieceToken.Id);
            });

            LostPieceIds.Add(pieceToken.PieceTokenData.PieceId);
        }

        /// <summary>
        /// Distributes a list of same-color piece tokens into available manual and lose areas, placing any excess
        /// tokens into the discard area if necessary.
        /// </summary>
        /// <remarks>If the combined capacity of manual and lose areas is insufficient for all tokens, any
        /// remaining tokens are moved to the discard area. The order of placement preserves the order of tokens in the
        /// input list.</remarks>
        /// <param name="allSameColorTokens">The list of piece tokens of the same color to be distributed. Cannot be null.</param>
        /// <param name="remainManualAreas">The list of available manual placement areas for tokens. The method attempts to fill these areas first.
        /// Cannot be null.</param>
        /// <param name="remainLoseAreas">The list of available lose areas for tokens. Used if manual areas are insufficient. Cannot be null.</param>
        public void MovePieceListToManualSubLoseArea
            (List<PieceToken> allSameColorTokens, List<PlaceTokenArea> remainManualAreas, List<LosePlaceTokenArea> remainLoseAreas)
        {
            //如果手动区域可以容纳所有相同颜色棋子，就放到手动区域；
            if (allSameColorTokens.Count <= remainManualAreas.Count)
            {
                for (int i = 0; i < allSameColorTokens.Count; i++)
                {
                    remainManualAreas[i].PlaceToken(allSameColorTokens[i]);
                }
            }
            //如果不可以容纳但手动区域和减分区域加起来可以容纳，就先放满手动区域再放减分区域；
            else if (allSameColorTokens.Count > remainManualAreas.Count && allSameColorTokens.Count <= remainManualAreas.Count + remainLoseAreas.Count)
            {

                for (int i = 0; i < remainManualAreas.Count; i++)
                {
                    remainManualAreas[i].PlaceToken(allSameColorTokens[i]);
                }
                for (int i = remainManualAreas.Count; i < allSameColorTokens.Count; i++)
                {
                    remainLoseAreas[i - remainManualAreas.Count].PlaceToken(allSameColorTokens[i]);
                }
            }
            //如果连减分区域也放不下了，就先放满手动区域和减分区域，剩余的放入弃牌区
            else
            {
                for (int i = 0; i < remainManualAreas.Count; i++)
                {
                    remainManualAreas[i].PlaceToken(allSameColorTokens[i]);
                }
                for (int i = remainManualAreas.Count; i < remainManualAreas.Count + remainLoseAreas.Count; i++)
                {
                    remainLoseAreas[i - remainManualAreas.Count].PlaceToken(allSameColorTokens[i]);
                }
                //剩余的放入弃牌区
                for (int i = remainManualAreas.Count + remainLoseAreas.Count; i < allSameColorTokens.Count; i++)
                {
                    LosePiece(allSameColorTokens[i]);
                }
            }
        }

        /// <summary>
        /// 将一系列棋子放入减分区，如果减分区放不下了就放入弃牌区
        /// </summary>
        /// <param name="allSameColorTokens"></param>
        /// <param name="remainLoseAreas"></param>
        public void MovePieceToSubLoseArea(List<PieceToken> allSameColorTokens, List<LosePlaceTokenArea> remainLoseAreas)
        {
            //如果减分区域可以容纳所有相同颜色棋子，就放到减分区域；
            if (allSameColorTokens.Count <= remainLoseAreas.Count)
            {
                for (int i = 0; i < allSameColorTokens.Count; i++)
                {
                    remainLoseAreas[i].PlaceToken(allSameColorTokens[i]);
                }
            }
            //如果连减分区域放不下了，就先放满减分区域，剩余的放入弃牌区
            else
            {
                for (int i = 0; i < remainLoseAreas.Count; i++)
                {
                    remainLoseAreas[i].PlaceToken(allSameColorTokens[i]);
                }
                //剩余的放入弃牌区
                for (int i = remainLoseAreas.Count; i < allSameColorTokens.Count; i++)
                {
                    LosePiece(allSameColorTokens[i]);
                }
            }
        }

        /// <summary>
        /// 将首位token放入减分区，如果减分区没有位置则替换最后一个
        /// </summary>
        /// <param name="token"></param>
        public void MoveFirstTokenToSub(PieceToken token)
        {
            var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
            if(loseAreas.Count == 0)
            {
                var area = BoardGameUtility.GetLastAreaInLoseArea(GetBoardWithCurrentComp());
                if (area != null)
                {
                    if(area.Token != null)
                    {
                        if(area.Token is PieceToken pieceToken)
                        {
                            LosePiece(pieceToken);
                            area.PlaceToken(token);
                        }
                    }
                }
            }
            else
            {
                MovePieceToSubLoseArea(new List<PieceToken>() { token}, loseAreas);
            }
        }

        private void SwitchPlayer()
        {
            if (CurrentPlayer == PlaceAreaCamp.Self)
            {
                SetCurrentPlayer(PlaceAreaCamp.Other);
                return;
            }
            if (CurrentPlayer == PlaceAreaCamp.Other)
            {
                SetCurrentPlayer(PlaceAreaCamp.Self);
                return;
            }
        }

        public bool MidFactoryAreaEmpty()
        {
            return BoardGameUtility.FactorysEmpty() && BoardGameUtility.MidTableEmpty();
        }

        /// <summary>
        /// 如果没有则将双方填满的手动区移动到颜色区,并计算分数
        /// </summary>
        public void MoveFilledRowInManualAreaToColoredArea()
        {
            //先移动自己的
            MoveFilledRowInManualAreaToColoredAreaPerPlayer(PlaceAreaCamp.Self);

            //再移动对手的
            MoveFilledRowInManualAreaToColoredAreaPerPlayer(PlaceAreaCamp.Other);
        }

        public void MoveFilledRowInManualAreaToColoredAreaPerPlayer(PlaceAreaCamp camp)
        {
            PlayerBoard playerBoard = null;
            if(camp == PlaceAreaCamp.Self)
            {
                playerBoard = m_SelfBoard;
            }
            else if(camp == PlaceAreaCamp.Other)
            {
                playerBoard = m_OtherBoard;
            }
            int fromScore = playerBoard.Score;

            var pieceRows = BoardGameUtility.GetFilledRowInManualArea(playerBoard);
            foreach (var row in pieceRows)
            {
                var firstItem = row.Areas[0];
                var data = firstItem.GetPositionData();
                var targetArea = BoardGameUtility.GetColoredTileInColoredArea(playerBoard, data.Row, ((PieceToken)firstItem.Token).PieceTokenData.ColorType);

                //表现：将第一个token放入对应颜色区，其余进入弃牌区
                if (firstItem.Token != null)
                {
                    targetArea.PlaceToken(firstItem.Token);
                    for (int i = 1; i < row.Areas.Count; i++)
                    {
                        if(row.Areas[i].Token is PieceToken pieceToken)
                            LosePiece(pieceToken);
                    }
                }

                //计算分数
                int stepScore = BoardGameUtility.CalculateScorePieceMoveToColoredArea(playerBoard, targetArea);
                BoardGameUtility.PlayerAddScore(playerBoard, stepScore);
            }

            //然后计算减分区
            var loseAreas = BoardGameUtility.GetAllFilledAreaInLoseArea(playerBoard);
            for (int i = 0; i < loseAreas.Count; i++)
            {
                var loseArea = loseAreas[i];
                if (loseArea.Token != null)
                {
                    BoardGameUtility.PlayerAddScore(playerBoard, loseArea.LosePoint);

                    //如果是首位token，放回中间区域，否则放入弃牌区
                    if(((PieceToken)loseArea.Token).PieceTokenData.ColorType == PieceColorType.SpecialToken)
                    {
                        //谁拥有首位token，下一回合就是谁的先手
                        CurrentPlayer = camp;

                        var midArea = BoardGameUtility.GetEmptyTokenAreaInMidArea(1);
                        if(midArea.Count > 0)
                        {
                            midArea[0].PlaceToken(loseArea.Token);
                            loseArea.RemoveToken();
                        }
                    }
                    else
                    {
                        if (loseArea.Token is PieceToken pieceToken)
                            LosePiece(pieceToken);
                    }
                }
            }

            int toScore = playerBoard.Score;
            playerBoard.PlayAddScoreAnim(fromScore, toScore);
        }

        /// <summary>
        /// 检查是否有玩家的颜色区某行被填满了
        /// </summary>
        /// <returns></returns>
        public bool ExistColoredAreaRowFullFilled()
        {
            return BoardGameUtility.ExistColoredAreaRowFullFilled(m_SelfBoard)
                || BoardGameUtility.ExistColoredAreaRowFullFilled(m_OtherBoard);
        }

        /// <summary>
        /// 最终阶段结算，并计算分数
        /// </summary>
        public void FinalSettlement()
        {
            int fromScore, toScore;
            
            fromScore = m_SelfBoard.Score;
            var score = BoardGameUtility.CalcualteFinalScoreGened(m_SelfBoard);
            BoardGameUtility.PlayerAddScore(m_SelfBoard, score);
            toScore = m_SelfBoard.Score;
            m_SelfBoard.PlayAddScoreAnim(fromScore, toScore);

            fromScore = m_OtherBoard.Score;
            score = BoardGameUtility.CalcualteFinalScoreGened(m_OtherBoard);
            BoardGameUtility.PlayerAddScore(m_OtherBoard, score);
            toScore = m_OtherBoard.Score;
            m_OtherBoard.PlayAddScoreAnim(fromScore, toScore);
        }

        public PlayerBoard GetWinner()
        {
            //谁分数高谁获胜
            if (m_SelfBoard.Score > m_OtherBoard.Score)
            {
                return m_SelfBoard;
            }
            else if (m_SelfBoard.Score < m_OtherBoard.Score)
            {
                return m_OtherBoard;
            }
            else
            {
                //否则最多水平列获胜
                int selfFilledNum = BoardGameUtility.GetColoredAreaRowFullFilledNum(m_SelfBoard);
                int otherFilledNum = BoardGameUtility.GetColoredAreaRowFullFilledNum(m_OtherBoard);
                if (selfFilledNum > otherFilledNum)
                {
                    return m_SelfBoard;
                }
                else if(selfFilledNum < otherFilledNum)
                {
                    return m_OtherBoard;
                }
            }
            return null; // 平局
        }

        /// <summary>
        /// 清除所有在场的棋子实体
        /// </summary>
        private void ClearAllPieceTokens()
        {
            if(m_ClearTableSequence != null)
            {
                m_ClearTableSequence.Kill();
                m_ClearTableSequence = null;
            }
            m_ClearTableSequence = DOTween.Sequence();

            var allEntitys = BoardGameUtility.GetAllPiecesInBoard();
            for(int i = 0; i < allEntitys.Count; i++)
            {
                var entity = allEntitys[i];
                var pieceToken = entity as IPieceToken;
                if (pieceToken != null)
                {
                    // 清除棋子与放置区域的关联
                    if (pieceToken.OwnerPlaceTokenArea != null)
                    {
                        pieceToken.OwnerPlaceTokenArea.RemoveToken();
                        pieceToken.OwnerPlaceTokenArea = null;
                    }

                    var tween = pieceToken.Transform.DOMove(m_PieceBag.position, 0.5f).SetEase(Ease.InBack);
                    pieceToken.Interactable = false;
                    tween.onKill += () =>
                    {
                        pieceToken.Interactable = true;
                        GameEntry.Entity.HideEntity(entity);
                    };

                    m_ClearTableSequence.Insert(0.15f * i, tween);
                }
                else
                {
                    Log.Error("Entity with ID {0} does not have a PieceToken component.", entity.Id);
                }
            }

            m_ClearTableSequence.OnKill(() =>
            {
                GameEntry.Event.Fire(this, ClearTableDoneEventArgs.Create());
                Log.Info("All piece tokens have been cleared from the board.");
            });
            m_ClearTableSequence.Play();
            GameEntry.Referee.ShowTip("正在清理桌面...");

            Log.Info("Start Cleared all piece tokens. Total entities: {0}", allEntitys.Count);
        }

        /// <summary>
        /// 游戏开始时的过场动画
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayStartGameCameraAnim(Action onComplete)
        {
            if (m_CameraDestinationTrans != null && m_MainCamera != null)
            {
                m_MainCamera.transform.DOMove(m_CameraDestinationTrans.position, 4f).SetEase(Ease.InOutSine);
                m_MainCamera.transform.DORotateQuaternion(m_CameraDestinationTrans.rotation, 4f)
                    .SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        onComplete.Invoke();
                        GameEntry.PlayerView.SetMovementActive(true);
                    });
            }
        }

        public void SendCurrentBoardInfoToAIServer()
        {
            if(FightwithAI)
            {
                var tableData = BoardGameUtility.GetTableData();
                string jsonString = LitJson.JsonMapper.ToJson(tableData);
                Log.Info("Table Data JSON: {0}", jsonString);

                // 发送 JSON 字符串到 AI 服务器
                GameEntry.AI.SendNetworkMessage(jsonString);
            }
        }

        public void ExecuteAIAction(AIAction action)
        {
            PieceColorType colorType = (PieceColorType)action.color;
            var allSameColorTokens = new List<PieceToken>(); 
            if (action.sourceId == -1)
            {
                //说明来源是中间区域
                var firstToken = BoardGameUtility.GetFirstTokenInMidArea();
                if (firstToken != null)
                {
                    //需要把首位token放入减分区
                    var loseAreas_fiestToken = BoardGameUtility.GetEmptyTokenAreaInLoseArea(GetBoardWithCurrentComp());
                    //MovePieceToSubLoseArea(new List<PieceToken> { firstToken }, loseAreas_fiestToken);
                    MoveFirstTokenToSub(firstToken);
                }


                allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInMidTable(colorType);
                if (action.destinationId == -1)
                {
                    //说明目的地是弃牌区
                    var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(m_OtherBoard);
                    MovePieceToSubLoseArea(allSameColorTokens, loseAreas);
                }
                else
                {
                    //说明目的地是花砖区行
                    var leftAreas = BoardGameUtility.GetEmptyTokenAreaInManualAreaInRow(m_OtherBoard, action.destinationId);
                    var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(m_OtherBoard);
                    MovePieceListToManualSubLoseArea(allSameColorTokens, leftAreas, loseAreas);
                }
            }
            else
            {
                //说明来源是工厂圆盘
                allSameColorTokens = BoardGameUtility.GetAllColorTypeTokenInFactory(colorType, action.sourceId, out var remainTokens);
                if(action.destinationId == -1)
                {
                    //说明目的地是弃牌区
                    var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(m_OtherBoard);
                    MovePieceToSubLoseArea(allSameColorTokens, loseAreas);
                    //将工厂圆盘内剩余token放入中间区域
                    int remainCount = remainTokens.Count;
                    var midList = BoardGameUtility.GetEmptyTokenAreaInMidArea(remainCount);
                    for (int i = 0; i < remainCount; i++)
                    {
                        midList[i].PlaceToken(remainTokens[i]);
                    }
                }
                else
                {
                    //说明目的地是花砖区行
                     var leftAreas = BoardGameUtility.GetEmptyTokenAreaInManualAreaInRow(m_OtherBoard, action.destinationId);
                    var loseAreas = BoardGameUtility.GetEmptyTokenAreaInLoseArea(m_OtherBoard);
                    MovePieceListToManualSubLoseArea(allSameColorTokens, leftAreas, loseAreas);
                    //将工厂圆盘内剩余token放入中间区域
                    int remainCount = remainTokens.Count;
                    var midList = BoardGameUtility.GetEmptyTokenAreaInMidArea(remainCount);
                    for (int i = 0; i < remainCount; i++)
                    {
                        midList[i].PlaceToken(remainTokens[i]);
                    }
                }
            }

            var tmpPlayer = CurrentPlayer;
            SwitchPlayer();
            GameEntry.Event.Fire(this, MovePieceCompleteEventArgs.Create(null, null, null, tmpPlayer));
            ClearSelectedPieceToken();
        }
    }
}
