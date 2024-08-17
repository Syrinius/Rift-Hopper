using UnityEngine;
using Random = System.Random;

namespace TileType {
    public class VerticalWallConnection : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData,int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            
            //implement proper random seed?

            double seed = Utils.RNGDouble();
            if (y >= lastRow - 4 && tileWeight != 0) {
                //Debug.Log($"x: {x}, y: {y}, type: 17, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.verticalWallConnection;
                tileContinuationCount = 0;
            }
            else if ((generationData[x, y - 1].weight < 6 && generationData[x, y + 1].weight < 6 &&generationData[x, y - 1].weight != 1 && generationData[x, y + 1].weight != 1 && tileWeight == 2 && seed < 0.5) || (tileWeight == 5 && seed < 0.25)) {
                //Debug.Log($"x: {x}, y: {y}, type: 18, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 2;
            }    
            else if (tileWeight == 0) {
                //Debug.Log($"x: {x}, y: {y}, type: 19, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.noWallConnection;
                tileContinuationCount = 0;
            }
            else {
                //Debug.Log($"x: {x}, y: {y}, type: 20, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.verticalWallConnection;
                tileContinuationCount = 0;
            }
            
            //previous column is VERTICAL_WALL_CONNECTION && previous tile isWall -> checkPreviousTileWeight -> start triple wall || start single corridor - mark vertical
            //previous column is VERTICAL_WALL_CONNECTION && previous tile !isWall -> start single corridor -> mark none

            return true;
        }

        public ITileBase Finalize(int weight) {
            return (weight == 0)? (ITileBase)Tile.verticalWallConnection : Tile.doubleVerticalWallConnection;
        }
    }
}