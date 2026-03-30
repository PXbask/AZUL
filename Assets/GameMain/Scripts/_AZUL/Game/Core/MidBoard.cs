using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    /// <summary>
    /// 中央棋盘 - 管理工厂圆盘和中央区域
    /// </summary>
    public class MidBoard : MonoBehaviour
    {
        [SerializeField]
        public PlaceAreaCamp camp = PlaceAreaCamp.Neutral;

        /// <summary>
        /// 工厂圆盘列表（可在 Inspector 中绑定）
        /// </summary>
        public List<FactoryDisk> FactoryDisks = new List<FactoryDisk>();

        /// <summary>
        /// 中央区域放置棋子的位置
        /// </summary>
        public List<PlaceTokenArea> CenterTokenAreas = new List<PlaceTokenArea>();

        private void Start()
        {
            foreach (var factoryDisk in FactoryDisks)
            {
                foreach (var area in factoryDisk.TokenAreas)
                {
                    area.Camp = camp;
                    area.PositionGroup = PlaceTokenPosition.Factory;
                }
            }

            foreach(var centerTokenArea in CenterTokenAreas)
            {
                centerTokenArea.Camp = camp;
                centerTokenArea.PositionGroup = PlaceTokenPosition.MidTable;
            }
        }

        public List<List<PlaceTokenAreaData>> GetFactoriesData()
        {
            List<List<PlaceTokenAreaData>> factoriesData = new List<List<PlaceTokenAreaData>>();
            foreach (var factoryDisk in FactoryDisks)
            {
                List<PlaceTokenAreaData> factoryData = new List<PlaceTokenAreaData>();
                foreach (var area in factoryDisk.TokenAreas)
                {
                    PlaceTokenAreaData data = BoardGameUtility.GetPlaceTokenAreaData(area);
                    factoryData.Add(data);
                }
                factoriesData.Add(factoryData);
            }
            return factoriesData;
        }

        public List<PlaceTokenAreaData> GetCenterData()
        {
            List<PlaceTokenAreaData> centerData = new List<PlaceTokenAreaData>();
            foreach (var area in CenterTokenAreas)
            {
                PlaceTokenAreaData data = BoardGameUtility.GetPlaceTokenAreaData(area);
                centerData.Add(data);
            }
            return centerData;
        }
    }
}
