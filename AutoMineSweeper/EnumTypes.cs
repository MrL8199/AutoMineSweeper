using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMineSweeper
{
    public enum GameState
    {
        Ready,
        Success,
        Fail
    }


    public enum MineState
    {
        Unknow = -1,
        None = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Mine = 9   //Flag
    }



    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    public class BoxLocation
    {
        public BoxLocation(int x, int y, MineState state)
        {
            LocationX = x;
            LocationY = y;
            MineState = state;
        }

        public int LocationX { get; set; }
        public int LocationY { get; set; }
        public MineState MineState { get; set; }
    }

    public class UnknowBoxSum
    {
        public List<BoxLocation> Boxes { get; set; }
        public int Sum { get; set; }



        public BoxLocation GetNotBelongBox(List<BoxLocation> TargetBoxes)
        {
            foreach (var targetBox in TargetBoxes)
            {
                bool found = false;

                foreach(var thisbox in Boxes)
                {
                    if(targetBox.LocationX==thisbox.LocationX && targetBox.LocationY== thisbox.LocationY)
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    return targetBox;
                }
            }

            return null; 
        }



        public bool MatchBox(List<BoxLocation> TargetBoxes)
        {
            bool match = true;

            foreach(var box in Boxes)
            {
                bool find = false;
                foreach(var targetBox in TargetBoxes)
                {
                    if(box.LocationX == targetBox.LocationX && box.LocationY== targetBox.LocationY)
                    {
                        find = true;
                        break;
                    }
                }

                if(!find)
                {
                    match = false;
                    break;
                }
            }

            return match;
        }
    }

    public class ClickEvent
    {
        public ClickEvent(int x, int y, ClickType clicktype)
        {
            LocalX = x;
            LocalY = y;
            ClickType = clicktype;
        }

        public int LocalX { get; set; }
        public int LocalY { get; set; }
        public ClickType ClickType { get; set; }

        public override string ToString()
        {
            string clicktypestr = "";
            if (ClickType == ClickType.LeftClick)
            {
                clicktypestr = "LeftClick";
            }
            else
            {
                clicktypestr = "RightClick";
            }
            return $"({LocalX},{LocalY}):{clicktypestr}";
        }
    }

    public enum ClickType
    {
        LeftClick,
        RightClick
    }
}
