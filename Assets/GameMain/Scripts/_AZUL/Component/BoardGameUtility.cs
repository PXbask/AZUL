using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public static class BoardGameUtility
    {
        /// <summary>
        /// Determines whether the specified color exists in the colored area of a given row on the player's board.
        /// </summary>
        /// <param name="boardGame">The player's board to search for the specified color.</param>
        /// <param name="row">The zero-based index of the row to check within the player's board.</param>
        /// <param name="color">The color to search for in the specified row.</param>
        /// <returns>true if the specified color is present in the colored area of the given row; otherwise, false.</returns>
        public static bool PlayerBoardHasColorInColoredAreaInRow(PlayerBoard boardGame, int row, PieceColorType color)
        {
            var areaList = boardGame.RightPlaceTokenAreas[row].Areas;
            for (int column = 0; column < areaList.Count; column++)
            {
                if (areaList[column].ColorType == color)
                {
                    return !areaList[column].IsEmpty();
                }
            }
            Debug.LogError($"Color {color} not found in the colored area of row {row} on the player's board.");
            return false;
        }

        /// <summary>
        /// Determines whether the specified color exists in the manual area of a given row on the player's board.
        /// </summary>
        /// <param name="boardGame">The player board to inspect. Cannot be null.</param>
        /// <param name="row">The zero-based index of the row to check within the manual area.</param>
        /// <param name="color">The color to search for in the specified row's manual area.</param>
        /// <returns>true if the specified color is present in the manual area of the given row; otherwise, false.</returns>
        public static bool PlayerBoardDiffColorInManualAreaInRow(PlayerBoard boardGame, int row, PieceColorType color)
        {
            var areaList = boardGame.LeftPlaceTokenAreas[row].Areas;
            var firstArea = areaList[0];
            return !firstArea.IsEmpty() && firstArea.Token.PieceTokenData.ColorType != color;
        }

        /// <summary>
        /// 获取与指定PieceToken颜色相同的所有PieceToken，这些PieceToken必须位于工厂区域内。
        /// </summary>
        /// <param name="pieceToken"></param>
        /// <returns></returns>
        public static List<PieceToken> GetAllColorTypeTokenInFactory(PieceToken pieceToken, out List<PieceToken> remainTokens)
        {
            var result = new List<PieceToken>();
            remainTokens = new List<PieceToken>();
            if (pieceToken.OwnerPlaceTokenArea != null && pieceToken.OwnerPlaceTokenArea.PositionGroup == PlaceTokenPosition.Factory)
            {
                foreach (var factory in GameEntry.BoardGame.MidBoard.FactoryDisks)
                {
                    if (factory.TokenAreas.Contains(pieceToken.OwnerPlaceTokenArea))
                    {
                        foreach (var area in factory.TokenAreas)
                        {
                            if (!area.IsEmpty())
                            {
                                if (area.Token.PieceTokenData.ColorType == pieceToken.PieceTokenData.ColorType)
                                {
                                    result.Add(area.Token);
                                }
                                else
                                {
                                    remainTokens.Add(area.Token);
                                }
                            }
                        }
                        return result;
                    }
                }
            }
            Debug.LogError("棋子不在工厂区域内，无法获取相同颜色的棋子。");
            return null;
        }

        /// <summary>
        /// 获取与指定PieceToken颜色相同的所有PieceToken，这些PieceToken必须位于中部区域内。
        /// </summary>
        /// <param name="pieceToken"></param>
        /// <returns></returns>
        public static List<PieceToken> GetAllColorTypeTokenInMidTable(PieceToken pieceToken)
        {
            var result = new List<PieceToken>();
            if (pieceToken.OwnerPlaceTokenArea != null && pieceToken.OwnerPlaceTokenArea.PositionGroup == PlaceTokenPosition.MidTable)
            {
                foreach (var area in GameEntry.BoardGame.MidBoard.CenterTokenAreas)
                {
                    if (!area.IsEmpty() && area.Token.PieceTokenData.ColorType == pieceToken.PieceTokenData.ColorType)
                    {
                        result.Add(area.Token);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定行的手动区域空间的所有TokenArea
        /// </summary>
        /// <param name="boardGame"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static List<PlaceTokenArea> GetEmptyTokenAreaInManualAreaInRow(PlayerBoard boardGame, int row)
        {
            var result = new List<PlaceTokenArea>();
            var areaList = boardGame.LeftPlaceTokenAreas[row].Areas;
            for (int column = 0; column < areaList.Count; column++)
            {
                if (areaList[column].IsEmpty())
                {
                    result.Add(areaList[column]);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定行的减分区域空间的所有TokenArea
        /// </summary>
        /// <param name="boardGame"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static List<LosePlaceTokenArea> GetEmptyTokenAreaInLoseArea(PlayerBoard boardGame)
        {
            var result = new List<LosePlaceTokenArea>();
            foreach (var area in boardGame.LosePlaceTokenAreas)
            {
                if (area.IsEmpty())
                {
                    result.Add(area);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取中间区域count个空闲位置
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<PlaceTokenArea> GetEmptyTokenAreaInMidArea(int count)
        {
            var result = new List<PlaceTokenArea>(count);
            var areaList = GameEntry.BoardGame.MidBoard.CenterTokenAreas;
            int addedCount = 0;
            for (int column = 0; column < areaList.Count; column++)
            {
                if (areaList[column].IsEmpty())
                {
                    result.Add(areaList[column]);
                    addedCount++;
                }
                if (addedCount == count)
                {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 从中间区域获取首位token
        /// </summary>
        /// <returns></returns>
        public static PieceToken GetFirstTokenInMidArea()
        {
            var areaList = GameEntry.BoardGame.MidBoard.CenterTokenAreas;
            for (int i = 0; i < areaList.Count; i++)
            {
                if (!areaList[i].IsEmpty() && areaList[i].Token.PieceTokenData.ColorType == PieceColorType.SpecialToken)
                {
                    return areaList[i].Token;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断所有工厂圆盘是否为空
        /// </summary>
        /// <returns></returns>
        public static bool FactorysEmpty()
        {
            for (int i = 0; i < GameEntry.BoardGame.MidBoard.FactoryDisks.Count; i++)
            {
                var factory = GameEntry.BoardGame.MidBoard.FactoryDisks[i];
                for (int j = 0; j < factory.TokenAreas.Count; j++)
                {
                    if (!factory.TokenAreas[j].IsEmpty())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 判断中间区域是否为空
        /// </summary>
        /// <returns></returns>
        public static bool MidTableEmpty()
        {
            var areaList = GameEntry.BoardGame.MidBoard.CenterTokenAreas;
            for (int i = 0; i < areaList.Count; i++)
            {
                if (!areaList[i].IsEmpty())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取手动区域所有填满的行
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <returns></returns>
        public static List<PlaceTokenAreaRow> GetFilledRowInManualArea(PlayerBoard playerBoard)
        {
            var result = new List<PlaceTokenAreaRow>();
            foreach (var row in playerBoard.LeftPlaceTokenAreas)
            {
                bool isFilled = true;
                foreach (var area in row.Areas)
                {
                    if (area.IsEmpty())
                    {
                        isFilled = false;
                        break;
                    }
                }
                if (isFilled)
                {
                    result.Add(row);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取颜色区某一行的带颜色放置区
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <param name="row"></param>
        /// <param name="colorType"></param>
        /// <returns></returns>
        public static ColoredPlaceTokenArea GetColoredTileInColoredArea(PlayerBoard playerBoard, int row, PieceColorType colorType)
        {
            foreach (var area in playerBoard.RightPlaceTokenAreas[row].Areas)
            {
                if (area.ColorType == colorType)
                {
                    return area;
                }
            }
            Debug.LogError("找不到对应的颜色");
            return null;
        }

        /// <summary>
        /// 当砖块放入对应颜色区时，得到的总分
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <param name="coloredPlaceTokenArea"></param>
        /// <returns></returns>
        public static int CalculateScorePieceMoveToColoredArea(PlayerBoard playerBoard, ColoredPlaceTokenArea coloredPlaceTokenArea)
        {
            int continuousRow = 0;
            int continuousCol = 0;
            var data = coloredPlaceTokenArea.GetPositionData();
            //先计算竖排,计算上下组成的最大的连续棋子数
            for(int i = data.Row - 1; i >= 0; i--)
            {
                var area = GetColoredTokenAreaByPostion(playerBoard, i, data.Column);
                if (area.IsEmpty())
                {
                    break;
                }
                continuousRow++;
            }
            for (int i = data.Row; i < playerBoard.RightPlaceTokenAreas.Count; i++)
            {
                var area = GetColoredTokenAreaByPostion(playerBoard, i, data.Column);
                if (area.IsEmpty())
                {
                    break;
                }
                continuousRow++;
            }

            //再计算横排
            for(int i = data.Column - 1; i >= 0; i--)
            {
                var area = GetColoredTokenAreaByPostion(playerBoard, data.Row, i);
                if (area.IsEmpty())
                {
                    break;
                }
                continuousCol++;
            }
            for(int i = data.Column; i < playerBoard.RightPlaceTokenAreas[data.Row].Areas.Count; i++)
            {
                var area = GetColoredTokenAreaByPostion(playerBoard, data.Row, i);
                if (area.IsEmpty())
                {
                    break;
                }
                continuousCol++;
            }

            //如果是单独的一个棋子，则得1分
            if(continuousCol == 1)
            {
                return continuousRow;
            }
            if(continuousRow == 1)
            {
                return continuousCol;
            }

            return continuousRow + continuousCol;
        }

        /// <summary>
        /// 根据行列获取颜色区的放置区，如果行列越界则返回null
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static ColoredPlaceTokenArea GetColoredTokenAreaByPostion(PlayerBoard playerBoard, int row, int column)
        {
            if (row < 0 || row >= playerBoard.RightPlaceTokenAreas.Count) return null;
            if (column < 0 || column >= playerBoard.RightPlaceTokenAreas[row].Areas.Count) return null;
            return playerBoard.RightPlaceTokenAreas[row].Areas[column];
        }

        /// <summary>
        /// 对应玩家获取分数
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <param name="score"></param>
        public static void PlayerAddScore(PlayerBoard playerBoard, int score)
        {
            if (playerBoard == null) return;
            playerBoard.Score += score;
        }

        /// <summary>
        /// 获取某玩家的减分区所有已放置棋子的区域
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <returns></returns>
        public static List<LosePlaceTokenArea> GetAllFilledAreaInLoseArea(PlayerBoard playerBoard)
        {
            var res = new List<LosePlaceTokenArea>();
            foreach (var area in playerBoard.LosePlaceTokenAreas)
            {
                if (!area.IsEmpty())
                {
                    res.Add(area);
                }
            }
            return res;
        }

        /// <summary>
        /// 检查是否有玩家的颜色区某行被填满了
        /// </summary>
        /// <returns></returns>
        public static bool ExistColoredAreaRowFullFilled(PlayerBoard playerBoard)
        {
            for (int i = 0; i < playerBoard.RightPlaceTokenAreas.Count; i++)
            {
                bool isFullFilled = true;
                foreach (var area in playerBoard.RightPlaceTokenAreas[i].Areas)
                {
                    if (area.IsEmpty())
                    {
                        isFullFilled = false;
                        break;
                    }
                }
                if (isFullFilled)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算终局结算获得的分数
        /// </summary>
        /// <returns></returns>
        public static int CalcualteFinalScoreGened(PlayerBoard playerBoard)
        {
            //寻找颜色区是否有满行，如果有满行则加满行分数
            var filledRowNum = 0;
            var filledColNum = 0;
            var filledColorNum = 0;
            for (int i = 0; i < playerBoard.RightPlaceTokenAreas.Count; i++)
            {
                bool isFullFilled = true;
                foreach (var area in playerBoard.RightPlaceTokenAreas[i].Areas)
                {
                    if (area.IsEmpty())
                    {
                        isFullFilled = false;
                        break;
                    }
                }
                if (isFullFilled)
                {
                    filledRowNum++;
                }
            }
            //寻找颜色区是否有满列，如果有满列则加满列分数
            for (int i = 0; i < playerBoard.RightPlaceTokenAreas[0].Areas.Count; i++)
            {
                bool isFullFilled = true;
                for (int j = 0; j < playerBoard.RightPlaceTokenAreas.Count; j++)
                {
                    var area = GetColoredTokenAreaByPostion(playerBoard, j, i);
                    if (area.IsEmpty())
                    {
                        isFullFilled = false;
                        break;
                    }
                }
                if (isFullFilled)
                {
                    filledColNum++;
                }
            }
            //寻找全部颜色均填充的情况，如果全部颜色均填充则加满色分数
            var dic = new Dictionary<PieceColorType, int>();
            for(int i = 0; i < playerBoard.RightPlaceTokenAreas.Count; i++)
            {
                foreach(var area in playerBoard.RightPlaceTokenAreas[i].Areas)
                {
                    if (!area.IsEmpty())
                    {
                        var color = area.ColorType;
                        if (dic.ContainsKey(color))
                        {
                            dic[color]++;
                        }
                        else
                        {
                            dic[color] = 1;
                        }
                    }
                }
            }
            foreach(var kvp in dic)
            {
                if (kvp.Value == playerBoard.RightPlaceTokenAreas.Count)
                {
                    filledColorNum++;
                }
            }

            return filledRowNum * 2 + filledColNum * 7 + filledColorNum * 10;
        }
    }
}
