using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AZUL
{
    public class PlayerBoard : MonoBehaviour
    {
        public bool isSelfPlayer;

        public PlaceAreaCamp camp;

        public ScorePieceToken ScorePieceToken;

        public List<ScorePlaceTokenArea> ScorePlaceTokenAreas = new List<ScorePlaceTokenArea>();

        /// <summary>
        /// 左侧放置区域（使用包装类支持 Inspector 序列化）
        /// </summary>
        public List<PlaceTokenAreaRow> LeftPlaceTokenAreas = new List<PlaceTokenAreaRow>();

        /// <summary>
        /// 右侧彩色放置区域（使用包装类支持 Inspector 序列化）
        /// </summary>
        public List<ColoredPlaceTokenAreaRow> RightPlaceTokenAreas = new List<ColoredPlaceTokenAreaRow>();

        public List<LosePlaceTokenArea> LosePlaceTokenAreas = new List<LosePlaceTokenArea>();

        private int m_Score = 0;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        public int Score
        {
            get => m_Score;
            set
            {
                m_Score = value;
                if (scoreText != null)
                {
                    int index = Mathf.Clamp((m_Score % ScorePlaceTokenAreas.Count) + 1, 0, ScorePlaceTokenAreas.Count - 1);
                    if(m_Score < ScorePlaceTokenAreas.Count)
                    {
                        index = Mathf.Clamp(m_Score, 0, ScorePlaceTokenAreas.Count);
                    }
                    else
                    {
                        index = Mathf.Clamp((m_Score - 1) % (ScorePlaceTokenAreas.Count - 1) + 1, 0, ScorePlaceTokenAreas.Count - 1);
                    }
                    GameEntry.BoardGame.MoveScoreTokenToArea(ScorePieceToken, ScorePlaceTokenAreas[index]);
                    scoreText.text = $"当前分数:{m_Score}";
                }
            }
        }

        private void Start()
        {
            ScorePieceToken = null;

            if (isSelfPlayer)
            {
                camp = PlaceAreaCamp.Self;
            }
            else
            {
                camp = PlaceAreaCamp.Other;
            }

            foreach (var row in LeftPlaceTokenAreas)
            {
                foreach (var area in row.Areas)
                {
                    area.Camp = camp;
                    area.PositionGroup = PlaceTokenPosition.Manual;
                }
            }

            foreach (var row in RightPlaceTokenAreas)
            {
                foreach (var area in row.Areas)
                {
                    area.Camp = camp;
                    area.PositionGroup = PlaceTokenPosition.Colored;
                }
            }

            foreach (var area in LosePlaceTokenAreas)
            {
                area.Camp = camp;
                area.PositionGroup = PlaceTokenPosition.Lose;
            }

            foreach (var area in ScorePlaceTokenAreas)
            {
                area.Camp = camp;
                area.PositionGroup = PlaceTokenPosition.Score;
            }
        }

        public void GameReset()
        {
            Score = 0;
            ScorePieceToken = null;
        }

        /// <summary>
        /// 获取左侧指定行列的区域
        /// </summary>
        public PlaceTokenArea GetLeftArea(int row, int col)
        {
            if (row < 0 || row >= LeftPlaceTokenAreas.Count) return null;
            if (col < 0 || col >= LeftPlaceTokenAreas[row].Areas.Count) return null;
            return LeftPlaceTokenAreas[row].Areas[col];
        }

        /// <summary>
        /// 获取右侧指定行列的区域
        /// </summary>
        public ColoredPlaceTokenArea GetRightArea(int row, int col)
        {
            if (row < 0 || row >= RightPlaceTokenAreas.Count) return null;
            if (col < 0 || col >= RightPlaceTokenAreas[row].Areas.Count) return null;
            return RightPlaceTokenAreas[row].Areas[col];
        }

        public PlayerBoardData GetPlayerBoardData()
        {
            PlayerBoardData data = new PlayerBoardData
            {
                score = Score,
                manualAreas = new List<List<PlaceTokenAreaData>>(),
                coloredAreas = new List<List<PlaceTokenAreaData>>(),
                loseAreas = new List<PlaceTokenAreaData>()
            };
            foreach (var row in LeftPlaceTokenAreas)
            {
                List<PlaceTokenAreaData> rowData = new List<PlaceTokenAreaData>();
                foreach (var area in row.Areas)
                {
                    rowData.Add(BoardGameUtility.GetPlaceTokenAreaData(area));
                }
                data.manualAreas.Add(rowData);
            }
            foreach (var row in RightPlaceTokenAreas)
            {
                List<PlaceTokenAreaData> rowData = new List<PlaceTokenAreaData>();
                foreach (var area in row.Areas)
                {
                    rowData.Add(BoardGameUtility.GetPlaceTokenAreaData(area));
                }
                data.coloredAreas.Add(rowData);
            }
            foreach (var area in LosePlaceTokenAreas)
            {
                data.loseAreas.Add(BoardGameUtility.GetPlaceTokenAreaData(area));
            }
            return data;
        }
    }
}
