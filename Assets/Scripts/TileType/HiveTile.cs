using System;

namespace TileType {
    public class HiveTile : ITileBase {
        public bool Decision(Room.TileGenerationData[,] generationData, int tileWeight, int x, int y, int lastRow,
            int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount) {
            throw new NotImplementedException();
        }

        public ITileBase Finalize(int weight) {
            throw new NotImplementedException();
        }
    }
}