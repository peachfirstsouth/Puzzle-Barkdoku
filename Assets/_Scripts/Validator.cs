using UnityEngine;

public static class MeowdokuRuleValidator
{
    /// <summary>
    /// Checks if placing a dog at (targetRow, targetCol) adheres to all game rules.
    /// </summary>
    public static bool IsValidMove(int targetRow, int targetCol, int targetRegionId)
    {
        BoxController[,] allCells = GameManager.Instance.boxes; // Assume GameManager stores the 2D array of boxes
        int gridSize = allCells.GetLength(0);

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                // Skip checking the cell against itself
                if (r == targetRow && c == targetCol) continue;

                BoxController otherCell = allCells[r, c];
                if (otherCell.CurrentState != CellState.Dog) continue;

                // Rule 1: Unique Row Check
                if (r == targetRow) return false;

                // Rule 2: Unique Column Check
                if (c == targetCol) return false;

                // Rule 3: Unique Region Check
                if (otherCell.RegionId == targetRegionId) return false;

                // Rule 4: "Aloof" Adjacent & Diagonal Space Check (No two dogs can touch anywhere)
                int rowDiff = Mathf.Abs(r - targetRow);
                int colDiff = Mathf.Abs(c - targetCol);
                if (rowDiff <= 1 && colDiff <= 1)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
