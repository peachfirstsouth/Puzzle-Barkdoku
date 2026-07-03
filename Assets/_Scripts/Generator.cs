using System.Collections.Generic;
using UnityEngine;

public class Generator
{
    private static readonly Vector2Int[] OrthogonalDirs = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>
    /// Generates a 2D grid where each value represents a Region ID (0 to gridSize - 1).
    /// </summary>
    /// <param name="gridSize">Size of the board (e.g., 8 or 9)</param>
    /// <param name="catSeeds">Optional: Pass your pre-calculated cat positions to ensure solvable regions.</param>
    public static int[,] GenerateRegions(int gridSize, List<Vector2Int> catSeeds = null)
    {
        int[,] grid = new int[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                grid[x, y] = -1; // -1 means unassigned

        List<Vector2Int> seeds = new List<Vector2Int>();

        // 1. Initialize Seeds
        if (catSeeds != null && catSeeds.Count == gridSize)
        {
            seeds.AddRange(catSeeds);
        }
        else
        {
            // Fallback: Pick random unique starting positions if no cat seeds provided
            while (seeds.Count < gridSize)
            {
                Vector2Int pos = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
                if (!seeds.Contains(pos)) seeds.Add(pos);
            }
        }

        int[] regionSizes = new int[gridSize];
        List<Vector2Int>[] frontiers = new List<Vector2Int>[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            frontiers[i] = new List<Vector2Int>();
            Vector2Int seed = seeds[i];
            grid[seed.x, seed.y] = i;
            regionSizes[i] = 1;
            AddNeighborsToFrontier(seed, grid, gridSize, frontiers[i]);
        }

        int totalCells = gridSize * gridSize;
        int assignedCells = gridSize;

        // 2. Grow Regions Simultaneously
        while (assignedCells < totalCells)
        {
            int bestRegion = -1;
            int minSize = int.MaxValue;

            // Find the smallest region that still has room to grow (keeps region sizes balanced)
            int startOffset = Random.Range(0, gridSize);
            for (int count = 0; count < gridSize; count++)
            {
                int i = (startOffset + count) % gridSize;
                CleanFrontier(frontiers[i], grid);

                if (frontiers[i].Count > 0 && regionSizes[i] < minSize)
                {
                    minSize = regionSizes[i];
                    bestRegion = i;
                }
            }

            // If some regions got boxed in, fallback to ANY region with an active frontier
            if (bestRegion == -1)
            {
                for (int i = 0; i < gridSize; i++)
                {
                    CleanFrontier(frontiers[i], grid);
                    if (frontiers[i].Count > 0)
                    {
                        bestRegion = i;
                        break;
                    }
                }
            }

            // If all frontiers are blocked but orphaned cells exist, break to fill them
            if (bestRegion == -1) break;

            // Pick a random tile from this region's expansion frontier
            int randIndex = Random.Range(0, frontiers[bestRegion].Count);
            Vector2Int target = frontiers[bestRegion][randIndex];
            frontiers[bestRegion].RemoveAt(randIndex);

            if (grid[target.x, target.y] == -1)
            {
                grid[target.x, target.y] = bestRegion;
                regionSizes[bestRegion]++;
                assignedCells++;
                AddNeighborsToFrontier(target, grid, gridSize, frontiers[bestRegion]);
            }
        }

        // 3. Cleanup: Fill any isolated trapped cells
        FillOrphanedCells(grid, gridSize);

        return grid;
    }

    private static void AddNeighborsToFrontier(Vector2Int pos, int[,] grid, int size, List<Vector2Int> frontier)
    {
        foreach (var dir in OrthogonalDirs)
        {
            Vector2Int next = pos + dir;
            if (next.x >= 0 && next.x < size && next.y >= 0 && next.y < size)
            {
                if (grid[next.x, next.y] == -1 && !frontier.Contains(next))
                {
                    frontier.Add(next);
                }
            }
        }
    }

    private static void CleanFrontier(List<Vector2Int> frontier, int[,] grid)
    {
        for (int i = frontier.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = frontier[i];
            if (grid[pos.x, pos.y] != -1)
            {
                frontier.RemoveAt(i);
            }
        }
    }

    private static void FillOrphanedCells(int[,] grid, int size)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (grid[x, y] == -1)
                    {
                        // Absorb into the first valid adjacent region found
                        foreach (var dir in OrthogonalDirs)
                        {
                            int nx = x + dir.x;
                            int ny = y + dir.y;
                            if (nx >= 0 && nx < size && ny >= 0 && ny < size && grid[nx, ny] != -1)
                            {
                                grid[x, y] = grid[nx, ny];
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}