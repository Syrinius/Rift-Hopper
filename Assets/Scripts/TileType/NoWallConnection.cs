using UnityEngine;
using Random = System.Random;

namespace TileType {
    public class NoWallConnection : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData,int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            
            //implement proper random seed?
            
            double seed = Utils.RNGDouble();

            if ((tileWeight == 2 && seed < 0.5) || (tileWeight == 5 && seed < 0.25)) {
                //Debug.Log($"x: {x}, y: {y}, type: 21, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 2;
            }
            else {
                //Debug.Log($"x: {x}, y: {y}, type: 22, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.verticalWallConnection;
                tileContinuationCount = 0;
            }
            
            //previous column is NO_WALL_CONNECTION -> checkPreviousTileWeight -> start triple wall || start single corridor
            
            return true;
        }
        
        public ITileBase Finalize(int weight) {
            return (weight == 0)? (ITileBase)Tile.noWallConnection : Tile.verticalWallConnection;
        }
    }
}