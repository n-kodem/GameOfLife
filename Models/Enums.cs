namespace GameOfLife.Models
{
    public enum Topology
    {
        Square,
        Hexagonal,
        Triangular
    }


    public enum ColoringModel
    {
        Standard,
        Immigration,
        QuadLife
    }

    public record Statistics
    {
        public long Generations { get; set; }
        public long CellsBorn { get; set; }
        public long CellsDied { get; set; }
        public int AliveCount { get; set; }
    }
}
