using UnityEngine;
using Random = System.Random;

namespace TileType {
    public class HorizontalWallConnection : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData,int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            
            //implement proper random seed?

            double seed = Utils.RNGDouble();

            if ((tileWeight == 2 && seed < 0.5) || (tileWeight == 5 && seed < 0.25)) {
                //Debug.Log($"x: {x}, y: {y}, type: 12, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 2;
            }
            else if (tileWeight != 0) {
                //Debug.Log($"x: {x}, y: {y}, type: 13, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.verticalWallConnection;
                tileContinuationCount = 0;
            }
            else {
                //Debug.Log($"x: {x}, y: {y}, type: 14, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 1;
            }
            
            //previous column is HORIZONTAL_WALL_CONNECTION && previous tile !isWall -> start double wall
            //previous column is HORIZONTAL_WALL_CONNECTION && previous tile isWall -> checkPreviousTileWeight -> start triple wall || start single corridor - mark vertical

            return true;
        }

        public ITileBase Finalize(int weight) {
            return (weight == 0)? (ITileBase)Tile.horizontalWallConnection : Tile.bothWallConnection;
        }
    }
}