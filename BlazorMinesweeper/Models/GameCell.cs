namespace BlazorMinesweeper.Models
{
	public record CellLocation(int Row, int Column);

	public enum CellState
	{
		Unknown = 0,
		Empty,
		Flagged,
		Bomb,
	}

	public sealed class GameCell
	{
		public GameCell(CellLocation location) => Location = location;

		public CellLocation Location { get; }

		public CellState State { get; set; }
		public bool HasBomb { get; set; }
		public int NeighborCount { get; set; }
	}
}