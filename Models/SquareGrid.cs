using System.Collections.Generic;

namespace GameOfLife.Models
{
    public class SquareGrid : GridBase
    {
        public override string DefaultRules => "B3/S23";

        public SquareGrid(int width, int height) : base(width, height) { }

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

            // Swap buffers
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
            var colors = new List<int>(8);
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    // wrapowanie bordera
                    if (nx < 0) nx = Width - 1;
                    if (nx >= Width) nx = 0;
                    if (ny < 0) ny = Height - 1;
                    if (ny >= Height) ny = 0;

                    int val = Cells[ny * Width + nx];
                    if (val > 0) colors.Add(val);
                }
            }
            return colors;
        }

        protected override IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    yield return (x + dx, y + dy);
                }
            }
        }
    }
}
