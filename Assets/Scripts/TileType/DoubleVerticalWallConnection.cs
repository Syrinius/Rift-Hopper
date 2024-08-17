using System;
using UnityEngine;
using Random = System.Random;

namespace TileType {
    public class DoubleVerticalWallConnection : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData,int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            
            if (tileWeight == 0) {
                //Debug.Log($"x: {x}, y: {y}, type: 15, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.noWallConnection;
                tileContinuationCount = 0;
            }
            else {
                //Debug.Log($"x: {x}, y: {y}, type: 16, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.verticalWallConnection;
                tileContinuationCount = 0;
            }
            
            //previous column is DOUBLE_VERTICAL_WALL_CONNECTION && previous tile isWall -> start single corridor - mark vertical
            //previous column is DOUBLE_VERTICAL_WALL_CONNECTION && previous tile !isWall -> start single corridor -> mark none

            return true;
        }

        public ITileBase Finalize(int weight) {
            throw new NotImplementedException();
        }
    }
}