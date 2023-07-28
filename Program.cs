using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            (byte,byte)[] pathList = new (byte, byte)[]
            {    
                (1,0),
                (1,1),(2,1),(3,1),(5,1),
                (3,2),(5,2),
                (1,3),(2,3),(3,3),(4,3),(5,3),
                (1,4),(3,4),
                (1,5),(3,5),(4,5),(5,5),
                (5,6)
            };
            Maze myMaze = new Maze(7, 7);
            myMaze.makePath(pathList);
            myMaze.printMaze();
        }
        /*
         *  ## [] ## ## ## ## ##
         *  ## [] [] [] ## [] ##
         *  ## ## ## [] ## [] ##
         *  ## [] [] [] [] [] ##
         *  ## [] ## [] ## ## ##
         *  ## [] ## [] [] [] ##
         *  ## ## ## ## ## [] ##
         */
    }
}