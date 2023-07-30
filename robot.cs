using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    internal class Robot
    {
        const double HIT_GIVEN_EXIST = 0.9;
        const double HIT_GIVEN_EMPTY = 0.05;
        const double MIS_GIVEN_EXIST = 0.1;
        const double MIS_GIVEN_EMPTY = 0.95;
        const double FWD = 0.75;
        const double LFT = 0.15;
        const double RGT = 0.10;
        readonly double[] DIR_PROB = new double[3] { RGT, LFT, FWD }; //R,L,F
        enum direction { west, north, east, south }; //0 = west 1 = north 2 = east 3 = south
        Maze internalMaze;
        bool[] measurements = new bool[4];

        (byte, byte)[] allTiles;
        Dictionary<(byte, byte), bool[]> theBigOne = new Dictionary<(byte, byte), bool[]>();    //mapping of all coords to a 4-bit state array (walls)
        Dictionary<(byte, byte), double> myGuess = new Dictionary<(byte, byte), double>();      //similar but now for estimates.

        public Robot(Maze x)
        {
            this.internalMaze = x;  //please fully define maze before setting up robot. thank you.
            allTiles = internalMaze.GetPathFull();
            Tile tempTile;
            foreach ((byte,byte)coord in allTiles)
            {
                theBigOne.Add(coord, new bool[4] { false, false, false, false });
                tempTile = new Tile(coord.Item1, coord.Item2);
                for (int i = 0; i < 4; i++)
                {
                    theBigOne[coord][i] = !internalMaze.IsLegalMove(tempTile, internalMaze.GetTile(tileCoords: GetNeighbor(tempTile,(direction)(i)).GetCoords()));
                }
            }
        }

        private bool[] Sense(Tile tgtTile)
        {
            //90% chance to correctly detect obstacle - 10% to erroneously ignore
            //5% chance to erroneously detect obstacle - 95% to correctly ignore
            Random q = new Random();
            double roll;
            bool[] temp = new bool[4];
            Tile nextTile;
            bool actual;

            for(int i = 0; i < 4; i++)
            {
                nextTile = GetNeighbor(tgtTile, (direction)i);
                actual = internalMaze.IsLegalMove(tgtTile, nextTile);
                roll = q.NextDouble();
                if (!actual) //the move in question is illegal; there is a wall
                {
                    if(roll <= HIT_GIVEN_EXIST) { actual = true; }
                    else { actual = false; }
                }
                else        //legal move; no wall
                {
                    if(roll <= MIS_GIVEN_EMPTY) { actual = false; }
                    else { actual = true;  }
                }
                temp[i] = actual;
            }
            return temp;
        }

        private void Filter()
        {
            double guess = 0.0;
            foreach (var x in theBigOne)
            {
                for (int i = 0; i < 4; i++)
                {
                    //product of all probs
                }
                //assign myGuess @ this index
            }

        }

        //Prob. function based on algorithm by Tâm Carbon https://tamcarbonart.wordpress.com/2018/10/09/c-pick-random-elements-based-on-probability/
        private void GetMove(Tile tgtTile)
        {
            //determine target direction somehow (dont pass it, i already pass so much LOL)
            //get 3 direction values based on this - one to 'right' and one to 'left' (+ original)  R,L,F
            //populate array of neighbors based on 3 directions
            //direction directions[] = new directions[3] {directions.west, directions.north, directions.east}
            //for(int i = 0; i < 3; i++) { neigborTiles[i] = GetNeighbor(curentTile, directions[i]) }
            Random q = new Random();
            direction toss = (direction)(q.Next(0, 3)); //picking random direction to move for now???????
            double roll = q.NextDouble();
            double cumulativeProb = 0.0;
            int tgtIndex = 0;
            direction[] directions;
            Tile[] neighborTiles = new Tile[3];

            switch (toss)       //MOST neighbor math can be determined by index +/- 1, but not west and south (ends of the list/no wraparound)
            {                   //unless .net does automatic wrap around stuff. LOL. not worried about ~10 lines of code
                case direction.west:
                    directions = new direction[3] { direction.north, direction.south, toss};
                    break;
                case direction.south:
                    directions = new direction[3] { direction.west, direction.east, toss };
                    break;
                default:
                    directions = new direction[3] { toss + 1, toss - 1, toss };
                    break;
            }

            for (int i = 0; i < 3; i++)                                     //NEED TO CHANGE THIS!
            { neighborTiles[i] = GetNeighbor(tgtTile, directions[i]); }     //unless I use tgtTile as the current tile. lol.


            for (int i = 0; i < 3; i++)
            {
                cumulativeProb += DIR_PROB[i];
                if(roll <= cumulativeProb)
                {
                    tgtIndex = i;
                    break; //hit! The random value is beneath the threshold established for the direction (but above the previous threshold)
                }
            }

            //moveTo(neighborTiles[tgtIndex]);
        }

        private Tile GetNeighbor(Tile current, direction x)
        {
            Tile testTgt;

            switch (x)
            {
                case direction.west:
                    testTgt = new Tile((byte)(current.x - 1), (byte)(current.y));
                    if(internalMaze.IsLegalMove(current, testTgt)) { return testTgt; }
                    break;
                case direction.north:
                    testTgt = new Tile((byte)(current.x), (byte)(current.y - 1));
                    if (internalMaze.IsLegalMove(current, testTgt)) { return testTgt; }
                    break;
                case direction.east:
                    testTgt = new Tile((byte)(current.x + 1), (byte)(current.y));
                    if (internalMaze.IsLegalMove(current, testTgt)) { return testTgt; }
                    break;
                case direction.south:
                    testTgt = new Tile((byte)(current.x), (byte)(current.y + 1));
                    if (internalMaze.IsLegalMove(current, testTgt)) { return testTgt; }
                    break;
                default:
                    break;
            }
            return current; //return self if no neighbor in given direction- automatic bounce! (move into wall -> not legal -> ends up on self)
        }





    }
}
