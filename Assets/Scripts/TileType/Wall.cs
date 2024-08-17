using System;
using UnityEngine;
using Random = System.Random;

namespace TileType {
    public class Wall : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData, int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            
            //implement proper random seed?
            
            double seed = Utils.RNGDouble();
            if (x == 3 && y == 5 && generationData[x, y].weight > 3) {
                currentContinuationType = Tile.horizontalWallConnection;
                tileContinuationCount = 2;
            }
            else if (generationData[x, y].weight > 1 && generationData[x, lastRow - 1].weight != 0 && y >= lastRow - 4) {
                if (tileWeight == 0) {
                    //Debug.Log($"x: {x}, y: {y}, type: 1, weight: {generationData[x, y].weight}");
                    currentContinuationType = Tile.horizontalWallConnection; 
                }
                else {
                    //Debug.Log($"x: {x}, y: {y}, type: 2, weight: {generationData[x, y].weight}");
                    currentContinuationType = Tile.bothWallConnection; 
                } 
                tileContinuationCount = 4;
            }
            else if (tileWeight == 0 && (x == lastColumn - 1 || generationData[x, y].weight == 1 || generationData[x, y].weight == 3 || (generationData[x, y].weight < 6 && seed < 0.5 ))) {
                //Debug.Log($"x: {x}, y: {y}, type: 3, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 1;
            }
            else if (tileWeight == 0) {
                //Debug.Log($"x: {x}, y: {y}, type: 4, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.horizontalWallConnection;
                tileContinuationCount = 2;
            }
            else if (x == lastColumn - 1 || (generationData[x, y + 1].weight < 6 && generationData[x, y - 1].weight < 6) && generationData[x, y].weight == 1 || generationData[x, y].weight == 3 || generationData[x, y + 1].weight == 1 || generationData[x, y + 1].weight == 3 || generationData[x, y - 1].weight == 1 || generationData[x, y - 1].weight == 3 || (generationData[x, y].weight == 2 && seed < 0.25 && x != 0)) {
                //Debug.Log($"x: {x}, y: {y}, type: 5, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.wall;
                tileContinuationCount = 2;
            }
            else {
                //Debug.Log($"x: {x}, y: {y}, type: 6, weight: {generationData[x, y].weight}");
                currentContinuationType = Tile.bothWallConnection;
                tileContinuationCount = 3;
            }

            //previous column is WALL && previous tile isWall -> checkColumnWeight && checkPreviousTileWeight  -> start quad corridor - mark both || start triple wall
            //previous column is WALL && previous tile !isWall -> checkColumnWeight -> start triple corridor - mark horizontal || start double wall
            //on first line's end force corridor on last decision

            return true;
        }

        public ITileBase Finalize(int weight) {
            return Tile.wall;
        }
    }
}