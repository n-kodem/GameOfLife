using System;
using System.Collections.Generic;

namespace GameOfLife.Models
{
    public class TriangularGrid : GridBase
    {
        public TriangularGrid(int width, int height) : base(width, height)
        {
            SetRules("B2/S23"); // Example rule for triangular life
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
            var colors = new List<int>(12);
            // In a triangular grid, each triangle has 3 edge neighbors and 9 vertex neighbors (total 12)
            // Or sometimes only 3 edge neighbors are used. Let's use 12 for "fuller" life.

            bool isUpright = (x + y) % 2 == 0;

            // Simple 3 edge neighbors:
            int[][] neighbors;
            if (isUpright)
            {
                neighbors = new int[][] {
                    new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, 1 }
                };
            }
            else
            {
                neighbors = new int[][] {
                    new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }
                };
            }

            // Expanding to 12 neighbors (all triangles sharing at least one vertex)
            // This is complex to define by offsets alone because of the alternating orientation.
            // For now, let's stick to the 3 edge neighbors + 9 vertex neighbors model 
            // commonly used in triangular cellular automata.

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    // Simple filter for 12 neighbors
                    if (Math.Abs(dx) + Math.Abs(dy) > 3) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0) nx = (Width + nx % Width) % Width;
                    if (nx >= Width) nx = nx % Width;
                    if (ny < 0) ny = (Height + ny % Height) % Height;
                    if (ny >= Height) ny = ny % Height;

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
                for (int dx = -2; dx <= 2; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (Math.Abs(dx) + Math.Abs(dy) > 3) continue;
                    yield return (x + dx, y + dy);
                }
            }
        }
    }
}
