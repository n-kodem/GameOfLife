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
        /// <summary> Standardowy </summary>
        Standard,
        /// <summary>  Immigration (2 kolory). </summary>
        Immigration,
        /// <summary> QuadLife (4 kolory). </summary>
        QuadLife
    }

    public record Statistics
    {
        /// <summary> Całkowita liczba pokoleń </summary>
        public long Generations { get; set; }
        /// <summary> Liczba komórek, które się narodziły </summary>
        public long CellsBorn { get; set; }
        /// <summary> Liczba komórek, które umarły </summary>
        public long CellsDied { get; set; }
        /// <summary> Liczba żywych komórek na planszy </summary>
        public int AliveCount { get; set; }
    }
}
