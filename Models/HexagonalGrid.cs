using System.Collections.Generic;

namespace GameOfLife.Models
{
    public class HexagonalGrid : GridBase
    {
        public override string DefaultRules => "B2/S34";

        public HexagonalGrid(int width, int height) : base(width, height)
        {
        }

        public override void Step()
        {
            int born = 0;
            int died = 0;
            int alive = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int currentIdx = y * Width + x;
                    int currentState = Cells[currentIdx];

                    var neighborColors = GetNeighborColors(x, y);
                    int count = neighborColors.Count;

                    if (currentState > 0)
                    {
                        if (SurvivalRules.Contains(count))
                        {
                            NextCells[currentIdx] = currentState;
                            alive++;
                        }
                        else
                        {
                            NextCells[currentIdx] = 0;
                            died++;
                        }
                    }
                    else
                    {
                        if (BirthRules.Contains(count))
                        {
                            NextCells[currentIdx] = DetermineNewColor(neighborColors);
                            born++;
                            alive++;
                        }
                        else
                        {
                            NextCells[currentIdx] = 0;
                        }
                    }
                }
            }

            var temp = Cells;
            Cells = NextCells;
            NextCells = temp;

            Stats.Generations++;
            Stats.CellsBorn += born;
            Stats.CellsDied += died;
            Stats.AliveCount = alive;
        }

        private List<int> GetNeighborColors(int x, int y)
        {
            var colors = new List<int>(6);


            int[][] neighbors;
            if (y % 2 == 0)
            {
                neighbors = new int[][] {
                    new int[] { 1, 0 }, new int[] { 1, -1 }, new int[] { 0, -1 },
                    new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 1, 1 }
                };
            }
            else
            {
                neighbors = new int[][] {
                    new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, -1 },
                    new int[] { -1, 0 }, new int[] { -1, 1 }, new int[] { 0, 1 }
                };
            }

            foreach (var offset in neighbors)
            {
                int nx = x + offset[0];
                int ny = y + offset[1];

                if (nx < 0) nx = Width - 1;
                if (nx >= Width) nx = 0;
                if (ny < 0) ny = Height - 1;
                if (ny >= Height) ny = 0;

                int val = Cells[ny * Width + nx];
                if (val > 0) colors.Add(val);
            }

            return colors;
        }

        protected override IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
        {
            int[][] neighbors;
            if (y % 2 == 0)
            {
                neighbors = new int[][] {
                    new int[] { 1, 0 }, new int[] { 1, -1 }, new int[] { 0, -1 },
                    new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 1, 1 }
                };
            }
            else
            {
                neighbors = new int[][] {
                    new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, -1 },
                    new int[] { -1, 0 }, new int[] { -1, 1 }, new int[] { 0, 1 }
                };
            }
            foreach (var offset in neighbors) yield return (x + offset[0], y + offset[1]);
        }
    }
}
