using GameFramework.DataTable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AZUL
{
    public class BoardGameComponent : GameFrameworkComponent
    {
        public List<int> RemainPieceIds = new List<int>();

        public Dictionary<int, List<int>> CurrentFactoryBoard = new Dictionary<int, List<int>>();

        [SerializeField]
        private bool m_Active = false;

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
        }
    }
}
