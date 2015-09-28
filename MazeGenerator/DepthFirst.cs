using System;
using System.Collections.Generic;

namespace Treboada.Net.Ia
{
	/// <summary>
	/// Depth first algorithm <see cref="https://en.wikipedia.org/wiki/Maze_generation_algorithm#Depth-first_search"/>
	/// </summary>
	public class DepthFirst : MazeGenerator
	{
		private Maze Maze; 

		private bool[,] Visited;


		public DepthFirst (Maze maze)
		{
			this.Maze = maze;
			this.Visited = new bool[maze.Cols, maze.Rows];
		}


		public bool IsVisited(int col, int row)
		{
			return Visited[col, row];
		}


		public void SetVisited(int col, int row, bool visited)
		{
			Visited[col, row] = visited;
		}


		public void Generate(int col, int row)
		{
			// explore recursively
			VisitRec (col, row);
		}

		private static Maze.Direction[] FourRoses = new Maze.Direction[] {
			Maze.Direction.N,
			Maze.Direction.E,
			Maze.Direction.S,
			Maze.Direction.W,
		};

		private void VisitRec(int col, int row)
		{
			// mark visited
			Visited [col, row] = true;

			// list of possible destinations
			List<Maze.Direction> pending = new List<Maze.Direction> (FourRoses);

			// while possible destinations
			while (pending.Count > 0) {

				// extract one from the list
				int index = RndFactory.Next () % pending.Count;
				Maze.Direction direction = pending[index];
				pending.RemoveAt (index);

				// relative to this cell
				int c = col, r = row;

				// calculate the next one
				switch (direction) {

				case Maze.Direction.N:
					r--;
					break;

				case Maze.Direction.E: 
					c++;
					break;

				case Maze.Direction.S: 
					r++;
					break;

				case Maze.Direction.W: 
					c--;
					break;
				}

				// if destination is valid
				if (CellIsValid (c, r) && !Visited [c, r]) {

					// open the wall and explore recursively
					Maze.UnsetWall (col, row, direction);
					VisitRec (c, r);
				}
			}

		}


		private bool CellIsValid(int col, int row)
		{
			return (col >= 0 && col < Maze.Cols && row >= 0 && row < Maze.Rows);
		}
	}
}
