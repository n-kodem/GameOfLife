using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Models
{
    public abstract class GridBase
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public int[] Cells { get; protected set; }

        public int[] NextCells { get; protected set; }

        protected List<int> BirthRules = new();
        protected List<int> SurvivalRules = new();

        public ColoringModel Coloring { get; set; } = ColoringModel.Standard;

        public Statistics Stats { get; } = new();

        /// <summary> Domyślne reguły dla danej topologii (np. B3/S23). </summary>
        public abstract string DefaultRules { get; }

        protected GridBase(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new int[width * height];
            NextCells = new int[width * height];
            SetRules(DefaultRules);
        }

        /// <summary>
        /// Parsuje ciąg tekstowy reguł (np. "B3/S23") i aktualizuje logikę
        /// </summary>
        public void SetRules(string ruleString)
        {
            // Format: B3/S23
            BirthRules.Clear();
            SurvivalRules.Clear();

            var parts = ruleString.ToUpper().Split('/');
            foreach (var part in parts)
            {
                if (part.StartsWith("B"))
                {
                    foreach (char c in part.Substring(1))
                        if (char.IsDigit(c)) BirthRules.Add(c - '0');
                }
                else if (part.StartsWith("S"))
                {
                    foreach (char c in part.Substring(1))
                        if (char.IsDigit(c)) SurvivalRules.Add(c - '0');
                }
            }
        }

        public void Clear()
        {
            Array.Clear(Cells, 0, Cells.Length);
            Stats.Generations = 0;
            Stats.CellsBorn = 0;
            Stats.CellsDied = 0;
            Stats.AliveCount = 0;
        }

        public void Randomize(double density = 0.2, int maxColors = 1)
        {
            Random rand = new();
            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i] = rand.NextDouble() < density ? rand.Next(1, maxColors + 1) : 0;
            }
            UpdateAliveCount();
        }

        private void UpdateAliveCount()
        {
            Stats.AliveCount = Cells.Count(c => c > 0);
        }

        public abstract void Step();

        protected int GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
            return Cells[y * Width + x];
        }

        protected abstract IEnumerable<(int x, int y)> GetNeighbors(int x, int y);

        /// <summary>
        /// Wyznacza kolory komórek
        ///
        protected int DetermineNewColor(List<int> neighborColors)
        {
            if (neighborColors.Count == 0) return 1;
            if (Coloring == ColoringModel.Standard) return 1;

            if (Coloring == ColoringModel.Immigration)
            {
                // dziecko otrzymuje kolor większości rodziców
                return neighborColors.GroupBy(c => c)
                                     .OrderByDescending(g => g.Count())
                                     .First().Key;
            }

            if (Coloring == ColoringModel.QuadLife)
            {
                // zasady mieszania 4 kolorów
                var groups = neighborColors.GroupBy(c => c).ToList();
                if (groups.Count == 3) // 3 rodziców, każdy inny kolor
                {
                    var allColors = new HashSet<int> { 1, 2, 3, 4 };
                    foreach (var g in groups) allColors.Remove(g.Key);
                    return allColors.FirstOrDefault(); // 4. kolor
                }
                return groups.OrderByDescending(g => g.Count()).First().Key;
            }

            return 1;
        }

        /// <summary> Przełącza stan komórki </summary>
        public void ToggleCell(int x, int y, int color = 1)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            int idx = y * Width + x;
            Cells[idx] = Cells[idx] > 0 ? 0 : color;
            UpdateAliveCount();
        }

        /// <summary> Ustawia kolor komórki </summary>
        public void SetCell(int x, int y, int color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            Cells[y * Width + x] = color;
        }
    }}
