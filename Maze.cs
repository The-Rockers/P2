using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    internal class Maze
    {
        /*
         *  ## [] ## ## ## ## ##
         *  ## [] [] [] ## [] ##
         *  ## ## ## [] ## [] ##
         *  ## [] [] [] [] [] ##
         *  ## [] ## [] ## ## ##
         *  ## [] ## [] [] [] ##
         *  ## ## ## ## ## [] ##
         */
        //7 wide 7 "tall"
        //X increases rightward, Y increases downward
        //49 items!

        Dictionary<(byte, byte), Tile> tileSet = new Dictionary<(byte, byte), Tile>(); //dict of all tiles. maps coords to tile objects
        (byte,byte)[] path = new (byte,byte)[19];   //list of path spaces.
        public (byte, byte) start, end;             //x and y for start and end - not relevant in P2 ;P
        private int maxY, maxX;                     //equals NUMBER of items per axis; 1 more than max "index"
        public Maze(int mY, int mX) 
        {
            maxY = mY;
            maxX = mX;
            for(byte i = 0; i < maxY; i++)      //11
            {
                for(byte j = 0; j < maxX; j++)  //12
                { 
                    tileSet.Add((j,i), new Tile(j,i));
                }
            }
        }

        public void MakePath((byte,byte)[] pathSpaces)
        {
            this.path = pathSpaces;
            foreach((byte, byte) t in pathSpaces)
            {
                if(tileSet.ContainsKey(t))
                {
                    
                    tileSet[t].SetFace("[  ]");
                }
                
            }
            //tileSet[path[0]].SetFace("00");
            start = path[0];    //set start coords
            end = path[^1];     //set end coord
        }

        public Tile GetPathStart()
        {
            return tileSet[path[0]];
        }

        public (byte,byte)[] GetPathFull()
        {
            return path;
        }

        public Tile GetTile((byte,byte) tileCoords)
        {
            try
            {
                return tileSet[tileCoords];
            }
            catch(KeyNotFoundException)
            {
                return tileSet[(0, 0)];
            }
        }

        public bool IsLegalMove(Tile a, Tile b)
        {
            //a is current, b is target
            //Check: if obstacle, if OOB, *then* if valid.
            //Consider moving this to agent - create dict of discovered illegal moves to avoid repeat?
            if(b.face == "####") { return false; }
            if(b.x >= maxX || b.x < 0) { return false; }
            if(b.y >= maxY || b.y < 0) { return false; }
            if(b.x == (a.x + 1) || b.x == (a.x - 1))
            {
                if(b.y == a.y)
                {
                    return true;
                }
            }
            else if(b.x == a.x)
            {
                if (b.y == (a.y + 1) || b.y == (a.y - 1))
                {
                    return true;
                }

            }

            return false;
        }

        public void PrintMaze()
        {
            for (byte i = 0; i < maxY; i++)
            {
                for (byte j = 0; j < maxX; j++)
                {
                    try 
                    { 
                        tileSet.TryGetValue((j, i), out Tile ex);
                        //Console.Write(" ");
                        Console.Write(String.Format("{0,-6}",ex.face));
                    }
                    catch(NullReferenceException)
                    {
                        Console.WriteLine(String.Format("Tile not found. Targeted Coordinate: {0},{1}", j, i));
                        continue;
                    }
                }
                Console.Write(Environment.NewLine);
            }
        }




    }
}
