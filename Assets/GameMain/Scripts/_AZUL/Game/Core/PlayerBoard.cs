using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public class PlayerBoard : MonoBehaviour
    {
        public bool isSelfPlayer;

        public PlaceAreaCamp camp;

        /// <summary>
        /// 左侧放置区域（使用包装类支持 Inspector 序列化）
        /// </summary>
        public List<PlaceTokenAreaRow> LeftPlaceTokenAreas = new List<PlaceTokenAreaRow>();

        /// <summary>
        /// 右侧彩色放置区域（使用包装类支持 Inspector 序列化）
        /// </summary>
        public List<ColoredPlaceTokenAreaRow> RightPlaceTokenAreas = new List<ColoredPlaceTokenAreaRow>();

        public List<LosePlaceTokenArea> LosePlaceTokenAreas = new List<LosePlaceTokenArea>();

        private void Start()
        {
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
    }
}
