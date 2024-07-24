/***
 * 
 *   MazeGenerator - It generates mazes for OSHWDEM's robot contest.
 * 
 *   Copyright (C) 2015 Bricolabs (bricolabs.cc) & Rafa Couto (aka caligari)
 * 
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Reflection.Emit;
using Mono.Options;

namespace Treboada.Net.Ia
{
	class Oshwdem
	{
		public bool ShouldShowHelp = false;

		public float Straightforward = 0.50f;


		public static int Cols = 16;
		public static int Rows = 16;
		public static bool CornerGoal = false;

        public static void Main (string[] args)
		{
			// show the version
			Console.WriteLine ("\nOSHWDEM Maze Generator v{0}.{1} R{2}", Version.Major, Version.Minor, Version.Revision);

			Oshwdem oshwdem = new Oshwdem ();

			oshwdem.CommandLineArgs (args);
			if (oshwdem.ShouldShowHelp) 
			{
				oshwdem.ShowHelp ();
			}
			else 
			{
				oshwdem.Run ();
			}
		}

		private void ShowHelp()
		{
			Console.WriteLine ("\n-h --help");
			Console.WriteLine ("    Shows this help");
            Console.WriteLine("\n-s --straightforward");
            Console.WriteLine("    Generates more straightness paths; float value (0.00 - 1.00), default is {0}", Straightforward);
            Console.WriteLine("\n-z --size");
            Console.WriteLine("    Defines the size of the maze; two int values separated by a comma, default is ({0},{1})", Cols, Rows);
            Console.WriteLine("\n-c --cornergoal");
            Console.WriteLine("    Indicate if the goal is in the upper right corner. default is false (goal in the center)\n");
        }

		public void CommandLineArgs(string[] args)
		{
			try 
			{
				var options = new OptionSet { 
					{ "s|straightforward=", "Probability to generate straightforward paths (0.0 - 1.0).", s => 	float.TryParse(s, out Straightforward) }, 
					{ "h|help", "Show this message and exit", h => ShouldShowHelp = (h != null) },
                    { "z|size=", "Two integers separated by a comma.",
					  n => {
							var nums = n.Split(',');
							if (nums.Length == 2 && int.TryParse(nums[0], out int num1) && int.TryParse(nums[1], out int num2)) {
                                Cols = num1;
								Rows = num2;
							} else {
								throw new OptionException("The argument should contain two integers separated by a comma.", "numbers");
							}
						} 
					},
                    { "c|cornergoal", "Indicate if the goal is in the corner.", c => CornerGoal = c != null }
				};

				//System.Collections.Generic.List<string> extra = 
				options.Parse (args);
			} 
			catch (OptionException e) 
			{
				Console.WriteLine ("Command line arguments error: {0}", e.Message);
				Console.WriteLine ("Try `--help' for more information.");
				ShouldShowHelp = true;
			}
		}

		private void Run()
		{
			while (true)
			{
				// create a square maze with 13 cells on every side
				Maze maze = CreateMaze(Cols, Rows);

				// the finish door
				//maze.UnsetWall (3, 0, Maze.Direction.W);

				// prepare de generator
				MazeGenerator generator = SetupGenerator(maze);

				// DepthFirst options
				DepthFirst df = generator as DepthFirst;
				if (df != null)
				{
					df.Straightforward = Straightforward;
					Console.WriteLine("Algorithm: depth-first [straightforward probability {0:P0}]", Straightforward);
				}

				// generate from top-left corner, next to the starting cell
				generator.Generate(maze.Cols - 2, 0);

				// output to the console
				Console.Write(maze);

				// wait for <enter>
				Console.ReadLine();
			}
		}


		private Maze CreateMaze(int X, int Y)
		{
			// square and fully walled
			Maze maze = new Maze (X, Y, Maze.WallInit.Full, CornerGoal);

			// set the starting cell
			maze.UnsetWall (0, maze.Rows - 1, Maze.Direction.N);

			if (!CornerGoal) {
                if (Cols % 2 == 0) {
                    //clear the walls inside 2x2 center cells
                    maze.UnsetWall(maze.Cols / 2 - 1, maze.Rows / 2 - 1, Maze.Direction.S);
                    maze.UnsetWall(maze.Cols / 2 - 1, maze.Rows / 2 - 1, Maze.Direction.E);
                    maze.UnsetWall(maze.Cols / 2, maze.Rows / 2 - 1, Maze.Direction.S);
                    maze.UnsetWall(maze.Cols / 2 - 1, maze.Rows / 2, Maze.Direction.E);
                }
                else {
                    // // clear the walls inside 2x2 upper right center cells
                    maze.UnsetWall(maze.Cols / 2, maze.Rows / 2 - 1, Maze.Direction.S);
                    maze.UnsetWall(maze.Cols / 2, maze.Rows / 2 - 1, Maze.Direction.E);
                    maze.UnsetWall(maze.Cols / 2 + 1, maze.Rows / 2 - 1, Maze.Direction.S);
                    maze.UnsetWall(maze.Cols / 2, maze.Rows / 2, Maze.Direction.E);

                }
            } else {
				// clear the walls inside 2x2 upper right corner cells
				maze.UnsetWall(maze.Cols - 2, 0, Maze.Direction.S);
				maze.UnsetWall(maze.Cols - 2, 0, Maze.Direction.E);
				maze.UnsetWall(maze.Cols - 1, 0, Maze.Direction.S);
				maze.UnsetWall(maze.Cols - 2, 1, Maze.Direction.E);
			}
            return maze;
		}


		private MazeGenerator SetupGenerator (Maze maze)
		{
			// pretty algorithm to generate mazes
			DepthFirst generator = new DepthFirst (maze);

			// starting cell is set
			generator.SetVisited (0, maze.Rows - 1, true);

            if (!CornerGoal)
            {
				if (Cols%2 == 0) {
                    // dont enter into the 2x2 center
                    generator.SetVisited(maze.Cols / 2 - 1, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2 - 1, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2 - 1, maze.Rows / 2, true);
                }
				else {
                    // dont enter into the 2x2 upper right center
                    generator.SetVisited(maze.Cols / 2, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2 + 1, maze.Rows / 2 - 1, true);
                    generator.SetVisited(maze.Cols / 2, maze.Rows / 2, true);

                }


            }
            else {
                // clear the walls inside 2x2 upper right corner cells
                generator.SetVisited(maze.Cols - 2, 0, true);
                //generator.SetVisited(maze.Cols - 2, 1, true);
                generator.SetVisited(maze.Cols - 1, 0, true);
                //generator.SetVisited(maze.Cols - 1, 1, true);
            }
            
            return generator;
		}


		public static Version Version { 
			get { return System.Reflection.Assembly.GetEntryAssembly().GetName().Version; } 
		}
	}
}
