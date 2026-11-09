using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class Snakes
    {
        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
            public Point() { }
        }
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down,
            Start
        }
        public List<Point> points;
        public Direction direction = Direction.Start;
        public bool GameOver = false;
    }
}
