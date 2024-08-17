using System;

namespace TileType {

    public static class Tile {
        public static Wall wall = new Wall();
        public static NoWallConnection noWallConnection = new NoWallConnection();
        public static VerticalWallConnection verticalWallConnection = new VerticalWallConnection();
        public static DoubleVerticalWallConnection doubleVerticalWallConnection = new DoubleVerticalWallConnection();
        public static HorizontalWallConnection horizontalWallConnection = new HorizontalWallConnection();
        public static BothWallConnection bothWallConnection = new BothWallConnection();
        public static HiveEntrance hiveEntrance = new HiveEntrance();
        public static HiveTile hiveTile = new HiveTile();
    }
    
    public interface ITileBase {

        bool Decision(Room.TileGenerationData[,] generationData, int tileWeight, int x, int y, int lastRow, int lastColumn, out ITileBase currentContinuationType, out int tileContinuationCount);
        ITileBase Finalize(int weight);
    }
}