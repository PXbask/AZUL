using GameFramework.DataTable;
using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class BoardGameComponent : GameFrameworkComponent
    {
        public List<int> RemainPieceIds = new List<int>();

        [SerializeField]
        private bool m_Active = false;

        [SerializeField]
        private bool m_HasSubscribe = false;

        /// <summary>
        /// 当前选中的棋子
        /// </summary>
        private PieceToken m_SelectedPieceToken = null;

        /// <summary>
        /// 主相机（用于射线检测）
        /// </summary>
        private Camera m_MainCamera;

        private Transform m_PieceBag;

        private PlayerBoard m_SelfBoard;
        private PlayerBoard m_OtherBoard;
        private MidBoard m_MidBoard;

        public readonly int PlayerNum = 2;
        public readonly int FactoryDiskNum = 5;

        protected override void Awake()
        {
            base.Awake();
            m_Active = false;
            m_HasSubscribe = false;
        }

        public void SubscribeEvents()
        {
            if (m_HasSubscribe) return;
            // 订阅实体显示成功事件（在 Start 中确保 GameEntry 已初始化）
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (GameEntry.Event != null)
            {
                //GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            }
        }

        private void Update()
        {
            if (!m_Active) return;

            // 检测鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
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

            if(m_SelectedPieceToken == null)
            {
                // 从鼠标位置发射射线
                Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 进行射线检测
                if (Physics.Raycast(ray, out hit))
                {
                    // 尝试获取点击物体上的 PieceToken 组件
                    PieceToken pieceToken = hit.collider.GetComponent<PieceToken>();

                    if (pieceToken != null && pieceToken.CanInteractive)
                    {
                        // 找到了 PieceToken，保存到缓存中
                        m_SelectedPieceToken = pieceToken;

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
                        ClearSelectedPieceToken();
                    }
                }
                else
                {
                    // 没有点击到任何物体，取消选中
                    ClearSelectedPieceToken();
                }
            }
        }


        /// <summary>
        /// 获取当前选中的棋子
        /// </summary>
        public PieceToken GetSelectedPieceToken()
        {
            return m_SelectedPieceToken;
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
            //如果对应的颜色区有这个颜色的棋子了，就不能放了；
            //除非颜色盘中该颜色均填满并且可选棋子只剩下这个颜色了
            //这样的话点击任意Manaul区均可把棋子放入失分区
            

            // 检查放置区域是否为空
            if (!targetArea.IsEmpty())
            {
                Log.Warning("PlaceTokenArea is not empty. Cannot place piece.");
                ClearSelectedPieceToken();
                return;
            }

            targetArea.PlaceToken(pieceToken);
            ClearSelectedPieceToken();
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void GameReset()
        {
            m_Active = true;

            RemainPieceIds.Clear();
            IDataTable<DRPiece> dtPiece = GameEntry.DataTable.GetDataTable<DRPiece>();
            foreach (DRPiece piece in dtPiece.GetAllDataRows())
            {
                RemainPieceIds.Add(piece.Id);
            }

            if(m_PieceBag == null)
            {
                m_PieceBag = GameObject.Find("PieceBag").transform;
            }
            if(m_SelfBoard == null)
            {
                m_SelfBoard = GameObject.Find("Board_self").GetComponent<PlayerBoard>();
            }
            if(m_OtherBoard == null)
            {
                m_OtherBoard = GameObject.Find("Board_other").GetComponent<PlayerBoard>();
            }
            if(m_MidBoard == null)
            {
                m_MidBoard = GameObject.Find("MidBoard").GetComponent<MidBoard>();
            }
            // 获取主相机
            m_MainCamera = Camera.main;
            if (m_MainCamera == null)
            {
                Log.Warning("Main Camera not found. Raycast detection will not work.");
            }

            SubscribeEvents();
        }

        /// <summary>
        /// 游戏开始时发牌
        /// </summary>
        public void DealPiece()
        {
            if (!m_Active)
            {
                Log.Error("BoardGameComponent is not active. Please call GameReset() before dealing pieces.");
                return;
            }

            RemainPieceIds.Remove(0);  // 假设0是一个特殊的棋子ID，代表空棋子或占位符，不需要发牌
            SpawnPieceAndPlace(0, GetRemainSlotFromMidDisk());

            var midDiskSlots = m_MidBoard.FactoryDisks.SelectMany(disk => disk.TokenAreas).ToList();
            var randomTokens = TakeRandomPieces(midDiskSlots.Count);
            for (int i = 0; i < randomTokens.Count; i++)
            {
                SpawnPieceAndPlace(randomTokens[i], midDiskSlots[i]);
            }
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
                int randomIndex = Random.Range(0, RemainPieceIds.Count);
                result.Add(RemainPieceIds[randomIndex]);
                RemainPieceIds.RemoveAt(randomIndex);
            }

            return result;
        }
    }
}
