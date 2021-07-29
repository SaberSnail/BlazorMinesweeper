using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorMinesweeper.Models
{
	public sealed class GameBoard
	{
		public GameBoard()
		{
			Cells = new List<IReadOnlyList<GameCell>>();
		}

		public IReadOnlyList<IReadOnlyList<GameCell>> Cells { get; set; }

		public void Reset(int rows, int columns, int bombs)
		{
			Cells = Enumerable.Range(0, rows)
				.Select(row =>
				{
					return Enumerable.Range(0, columns)
						.Select(column => new GameCell(new CellLocation(row, column)))
						.ToList()
						.AsReadOnly();
				})
				.ToList()
				.AsReadOnly();
			m_bombs = Math.Min(rows * columns - 1, bombs);
			m_isGenerated = false;
		}

		public GameCell TestCell(int row, int column)
		{
			GenerateBombsIfNeeded(new CellLocation(row, column));

			var cell = GetCell(row, column);
			if (cell?.State != CellState.Unknown)
				return cell;

			if (cell.HasBomb)
			{
				cell.State = CellState.Bomb;
			}
			else
			{
				cell.State = CellState.Empty;
				TryRevealNeighbors(cell);
			}
			return cell;
		}

		public GameCell FlagCell(int row, int column)
		{
			var cell = GetCell(row, column);
			if (cell?.State == CellState.Unknown)
				cell.State = CellState.Flagged;
			else if (cell?.State == CellState.Flagged)
				cell.State = CellState.Unknown;
			return cell;
		}

		public void RevealAll()
		{
			foreach (var cell in Cells.SelectMany(x => x))
			{
				if (cell.State == CellState.Unknown)
					cell.State = cell.HasBomb ? CellState.Bomb : CellState.Empty;
			}
		}

		private GameCell GetCell(int row, int column)
		{
			if (row < 0 || column < 0 || row >= Cells.Count)
				return null;
			var cellRow = Cells[row];
			if (column >= cellRow.Count)
				return null;
			return cellRow[column];
		}

		private void GenerateBombsIfNeeded(CellLocation ignore)
		{
			if (m_isGenerated)
				return;

			var ignoreSet = GetNeighbors(ignore).Select(x => x.Location).Append(ignore).ToHashSet();
			var rng = new Random();
			m_isGenerated = true;
			var bombCells = Cells
				.SelectMany(x => x)
				.Where(x => !ignoreSet.Contains(x.Location))
				.OrderBy(x => rng.Next())
				.Take(m_bombs);
			foreach (var cell in bombCells)
				cell.HasBomb = true;

			foreach (var cell in Cells.SelectMany(x => x))
				cell.NeighborCount = GetNeighbors(cell.Location).Count(x => x.HasBomb);
		}

		private void TryRevealNeighbors(GameCell cell)
		{
			if (cell.NeighborCount != 0)
				return;

			var neighbors = new Stack<GameCell>();
			var ignore = new HashSet<CellLocation> { cell.Location };
			neighbors.Push(cell);

			while (neighbors.Count != 0)
			{
				var check = neighbors.Pop();
				foreach (var candidate in GetNeighbors(check.Location))
				{
					if (ignore.Contains(candidate.Location) || candidate.State != CellState.Unknown)
						continue;
					ignore.Add(candidate.Location);
					candidate.State = CellState.Empty;
					if (candidate.NeighborCount == 0)
						neighbors.Push(candidate);
				}
			}
		}

		private IEnumerable<GameCell> GetNeighbors(CellLocation location)
		{
			var maxRow = Cells.Count - 1;
			var maxColumn = Cells[0].Count - 1;

			if (location.Row > 0)
			{
				if (location.Column > 0)
					yield return Cells[location.Row - 1][location.Column - 1];
				yield return Cells[location.Row - 1][location.Column];
				if (location.Column < maxColumn)
					yield return Cells[location.Row - 1][location.Column + 1];
			}

			if (location.Column > 0)
				yield return Cells[location.Row][location.Column - 1];
			yield return Cells[location.Row][location.Column];
			if (location.Column < maxColumn)
				yield return Cells[location.Row][location.Column + 1];

			if (location.Row < maxRow)
			{
				if (location.Column > 0)
					yield return Cells[location.Row + 1][location.Column - 1];
				yield return Cells[location.Row + 1][location.Column];
				if (location.Column < maxColumn)
					yield return Cells[location.Row + 1][location.Column + 1];
			}
		}

		bool m_isGenerated;
		int m_bombs;
	}
}
