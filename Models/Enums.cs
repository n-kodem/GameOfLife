namespace GameOfLife.Models
{
    /// <summary>
    /// Definiuje dostępne topologie siatki automatu komórkowego.
    /// </summary>
    public enum Topology
    {
        /// <summary> Standardowa siatka kwadratowa (8 sąsiadów). </summary>
        Square,
        /// <summary> Siatka sześciokątna (6 sąsiadów). </summary>
        Hexagonal,
        /// <summary> Siatka trójkątna (12 sąsiadów). </summary>
        Triangular
    }

    /// <summary>
    /// Modele dziedziczenia kolorów przez nowo narodzone komórki.
    /// </summary>
    public enum ColoringModel
    {
        /// <summary> Standardowy (jeden kolor dla wszystkich). </summary>
        Standard,
        /// <summary> Model Immigration (2 kolory, kolor większości rodziców). </summary>
        Immigration,
        /// <summary> Model QuadLife (4 kolory, zasady mieszania barw). </summary>
        QuadLife
    }

    /// <summary>
    /// Przechowuje dane statystyczne dotyczące bieżącej symulacji.
    /// </summary>
    public record Statistics
    {
        /// <summary> Całkowita liczba pokoleń od początku symulacji. </summary>
        public long Generations { get; set; }
        /// <summary> Łączna liczba komórek, które narodziły się w trakcie symulacji. </summary>
        public long CellsBorn { get; set; }
        /// <summary> Łączna liczba komórek, które obumarły w trakcie symulacji. </summary>
        public long CellsDied { get; set; }
        /// <summary> Aktualna liczba żywych komórek na planszy. </summary>
        public int AliveCount { get; set; }
    }
}
