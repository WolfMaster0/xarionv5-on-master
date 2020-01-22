// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core.ArrayExtensions;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OpenNos.PathFinder
{
    public static class BestFirstSearch
    {
        #region Methods

        public static List<Node> Backtrace(Node end)
        {
            List<Node> path = new List<Node>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }

        public static List<Node> FindPathJagged(GridPos start, GridPos end, GridPos[][] grid)
        {
            int gridX = grid.Length;
            int gridY = grid[0].Length;
            if (gridX <= start.X || gridY <= start.Y || start.X < 0 || start.Y < 0)
            {
                return new List<Node>();
            }

            Node[][] nodeGrid = JaggedArrayExtensions.CreateJaggedArray<Node>(gridX, gridY);
            if (nodeGrid[start.X][start.Y] == null)
            {
                nodeGrid[start.X][start.Y] = new Node(grid[start.X][start.Y]);
            }
            Node startingNode = nodeGrid[start.X][start.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(startingNode);
            startingNode.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                Node node = path.Pop();
                if (nodeGrid[node.X][node.Y] == null)
                {
                    nodeGrid[node.X][node.Y] = new Node(grid[node.X][node.Y]);
                }
                nodeGrid[node.X][node.Y].Closed = true;

                //if reached the end position, construct the path and return it
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }

                // get neigbours of the current node
                List<Node> neighbors = GetNeighborsJagged(nodeGrid, node, grid);
                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            neighbor.F = Heuristic.Octile(Math.Abs(neighbor.X - end.X), Math.Abs(neighbor.Y - end.Y));
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
            return new List<Node>();
        }

        public static List<Node> GetNeighborsJagged(Node[][] grid, Node node, GridPos[][] mapGrid)
        {
            int gridX = grid.Length;
            int gridY = grid[0].Length;
            short x = node.X, y = node.Y;
            List<Node> neighbors = new List<Node>();
            bool s0 = false, d0, s1 = false, d1, s2 = false, d2, s3 = false, d3;

            // ↑
            int indexX = x;
            int indexY = y - 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
                s0 = true;
            }

            // →
            indexX = x + 1;
            indexY = y;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
                s1 = true;
            }

            // ↓
            indexX = x;
            indexY = y + 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
                s2 = true;
            }

            // ←
            indexX = x - 1;
            indexY = y;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
                s3 = true;
            }

            d0 = s3 || s0;
            d1 = s0 || s1;
            d2 = s1 || s2;
            d3 = s2 || s3;

            // ↖
            indexX = x - 1;
            indexY = y - 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && d0 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
            }

            // ↗
            indexX = x + 1;
            indexY = y - 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && d1 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
            }

            // ↘
            indexX = x + 1;
            indexY = y + 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && d2 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
            }

            // ↙
            indexX = x - 1;
            indexY = y + 1;
            if (gridX > indexX && gridY > indexY && indexX >= 0 && indexY >= 0 && d3 && mapGrid[indexX][indexY].IsWalkable())
            {
                if (grid[indexX][indexY] == null)
                {
                    grid[indexX][indexY] = new Node(mapGrid[indexX][indexY]);
                }
                neighbors.Add(grid[indexX][indexY]);
            }

            return neighbors;
        }

        public static Node[][] LoadBrushFireJagged(GridPos user, GridPos[][] grid, short maxDistance = 22)
        {
            int gridX = grid.Length;
            int gridY = grid[0].Length;
            Node[][] nodeGrid = JaggedArrayExtensions.CreateJaggedArray<Node>(gridX, gridY);

            if (nodeGrid[user.X][user.Y] == null)
            {
                nodeGrid[user.X][user.Y] = new Node(grid[user.X][user.Y]);
            }
            Node start = nodeGrid[user.X][user.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(start);
            start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                Node node = path.Pop();
                if (nodeGrid[node.X][node.Y] == null)
                {
                    nodeGrid[node.X][node.Y] = new Node(grid[node.X][node.Y]);
                }

                nodeGrid[node.X][node.Y].Closed = true;

                // get neighbors of the current node
                List<Node> neighbors = GetNeighborsJagged(nodeGrid, node, grid);

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            double distance = Heuristic.Octile(Math.Abs(neighbor.X - node.X), Math.Abs(neighbor.Y - node.Y)) + node.F;
                            if (distance > maxDistance)
                            {
                                neighbor.Value = 1;
                                continue;
                            }
                            neighbor.F = distance;
                            nodeGrid[neighbor.X][neighbor.Y].F = neighbor.F;
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
            return nodeGrid;
        }

        public static List<Node> TracePathJagged(Node node, Node[][] grid, GridPos[][] mapGrid)
        {
            List<Node> list = new List<Node>();
            if (mapGrid == null || grid == null || node.X >= grid.Length || node.Y >= grid[0].Length || node.X < 0 || node.Y < 0 || grid[node.X][node.Y] == null)
            {
                node.F = 100;
                list.Add(node);
                return list;
            }
            Node currentnode = grid[node.X][node.Y];
            while (currentnode.F != 1 && currentnode.F != 0)
            {
                Node newnode = GetNeighborsJagged(grid, currentnode, mapGrid)?.OrderBy(s => s.F).FirstOrDefault();
                if (newnode != null)
                {
                    list.Add(newnode);
                    currentnode = newnode;
                }
            }
            return list;
        }

        #endregion
    }
}