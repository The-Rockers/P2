﻿using System;
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

        (byte, byte)[] allPathTiles;
        Dictionary<(byte, byte), bool[]> theBigOne = new Dictionary<(byte, byte), bool[]>();    //mapping of all coords to a 4-bit state array (walls)
        Dictionary<(byte, byte), double> myGuess = new Dictionary<(byte, byte), double>();      //similar but now for estimates.

        public Robot(Maze x)
        {
            this.internalMaze = x;  //please fully define maze before setting up robot. thank you.
            allPathTiles = internalMaze.GetPathFull();
            Tile tempTile;
            double intl = 1.0 / allPathTiles.Count();
            foreach ((byte,byte)coord in allPathTiles)
            {
                this.theBigOne.Add(coord, new bool[4] { false, false, false, false });
                this.myGuess.Add(coord, intl);
                tempTile = new Tile(coord.Item1, coord.Item2);
                Tile cursorTile;
                internalMaze.GetTile((coord)).SetFace(Convert.ToString(Math.Round(intl*100,2)));

                cursorTile = internalMaze.GetTile(((byte,byte))(tempTile.x-1, tempTile.y));
                this.theBigOne[coord][0] = !internalMaze.IsLegalMove(tempTile, cursorTile);

                cursorTile = internalMaze.GetTile(((byte, byte))(tempTile.x, tempTile.y-1));
                this.theBigOne[coord][1] = !internalMaze.IsLegalMove(tempTile, cursorTile);

                cursorTile = internalMaze.GetTile(((byte,byte))(tempTile.x+1, tempTile.y));
                this.theBigOne[coord][2] = !internalMaze.IsLegalMove(tempTile, cursorTile);

                cursorTile = internalMaze.GetTile(((byte, byte))(tempTile.x, tempTile.y+1));
                this.theBigOne[coord][3] = !internalMaze.IsLegalMove(tempTile, cursorTile);

                //doing this manually sucks but the other way had a zillion casts that sucked lol.
            }
            
        }

        public void PrintTruth(byte x, byte y) //debug only. crashes if you ask for something not on the path, haha
        { Console.WriteLine(printEvidence(theBigOne[(x,y)])); }


        private bool[] Sense(Tile tgtTile)  //returns flawed measurements
        {
            //90% chance to correctly detect obstacle - 10% to erroneously ignore
            //5% chance to erroneously detect obstacle - 95% to correctly ignore
            Random q = new Random();
            double roll;
            bool[] measurement = new bool[4];
            //Tile nextTile;
            bool actual;

            for(int i = 0; i < 4; i++)
            {
                //nextTile = GetNeighbor(tgtTile, (direction)i);
                //actual = internalMaze.IsLegalMove(tgtTile, nextTile);       //actual: is the ground truth. why did i do this...?
                

                roll = q.NextDouble(); //so long as the value is UNDER the threshold of RIGHT|TRUTH

                if (this.theBigOne[tgtTile.GetCoords()][i]) //there is a wall
                {
                    if(roll <= HIT_GIVEN_EXIST) { actual = true; }
                    else { actual = false; }    //wait. this is redundant. uh.
                }                                                   
                else        //legal move; no wall
                {
                    if(roll <= MIS_GIVEN_EMPTY) { actual = false; }
                    else { actual = true;  }
                }
                //this loop flips "actual" from meaning "this is a legal move" to "there is no wall here"
                //so I can just assign to "actual" at end of iteration
                measurement[i] = actual;
            }
            Console.WriteLine(String.Format("The Tile I'm at: {0},{1}",tgtTile.x, tgtTile.y));
            Console.WriteLine(String.Format("What I THINK I see...{0}", printEvidence(measurement)));
            Console.WriteLine(String.Format("What's REALLY there... {0}", printEvidence(this.theBigOne[tgtTile.GetCoords()])));
            return measurement;
        }

        public string printEvidence(bool[] targ)
        {
            string targAsStr = "[";
            foreach(bool x in targ)
            {
                if (x) { targAsStr += "1"; }
                else { targAsStr += "0"; }
            }
            targAsStr += "]";
            return(targAsStr);

        }

        public void Filter(Tile currentTile)   //gee i sure hope this is what filtering is!! edit wow i was close!
        {                       //Filter: foreach item in list of open tiles, multiply together all (Z|S), then take that and multiply it by the previously existing estimate.
                                //divide THAT number by the sum of ALL of those numbers for a given tile's posterior estimate.
            bool[] z = Sense(currentTile);
            double[] guesstimate = new double[4];
            double guess = 0.0;
            double runningSum = 0.0;
            foreach (var x in theBigOne)            //FIXME: wait idk. lol.
            {
                for (int i = 0; i < 4; i++)
                {

                    if (x.Value[i])     //wall
                    {
                        if (z[i]) { guesstimate[i] = HIT_GIVEN_EXIST; } //true given true
                        else { guesstimate [i] = MIS_GIVEN_EXIST; } //false given true
                    }
                    else                //no wall
                    {
                        if (z[i]) { guesstimate[i] = HIT_GIVEN_EMPTY; } //true given false
                        else { guesstimate[i] = MIS_GIVEN_EXIST; } //false given false
                    }
                    //should probably. change this such that it checks most common cases (true|true, false|false) first.
                }
                for(int j = 1; j < 4; j++) { guesstimate[0] *= guesstimate[j]; }
                guess = guesstimate[0];
                guess *= myGuess[(x.Key.Item1, x.Key.Item2)];
                myGuess[(x.Key.Item1, x.Key.Item2)] = guess;    //ok i did that . now we have to sum that with all tiles ever yay and divide.
                runningSum += guess;
                //now remember to change the tile's face to this value. also round it to 4(?) digits
            }
            foreach(var y in theBigOne)
            {
                myGuess[(y.Key.Item1, y.Key.Item2)] /= runningSum;
            }
            updateMaze();
        }
        
        private void updateMaze()
        {
            Tile y;
            double g;
            foreach(var x in myGuess)
            {
                y = internalMaze.GetTile(x.Key);
                g = Math.Round(x.Value * 100, 2);
                y.SetFace(Convert.ToString(g));
            }
        }


        //Prob. function based on algorithm by Tâm Carbon https://tamcarbonart.wordpress.com/2018/10/09/c-pick-random-elements-based-on-probability/
        private void GetMove(Tile tgtTile)
        {
            //determine target direction (somehow)
            //get 3 direction values based on this - one to 'right' and one to 'left' (+ original)  R,L,F
            //populate array of neighbors based on 3 directions, eg:
            //  direction directions[] = new directions[3] {directions.west, directions.north, directions.east }
            //  for(int i = 0; i < 3; i++) { neigborTiles[i] = GetNeighbor(curentTile, directions[i]) }
            Random q = new Random();
            direction toss = (direction)(q.Next(0, 3)); //picking random direction to move for now??? (TikTok TTS lady voice)
            double roll = q.NextDouble();
            double cumulativeProb = 0.0;
            int tgtIndex = 0;
            direction[] directions;
            Tile[] neighborTiles = new Tile[3];

            switch (toss)       //MOST neighbor math can be determined by index +/- 1, but not west and south (ends of the list/no wraparound)
            {                   //unless .Net does automatic wrap around stuff. LOL. not worried about ~10 lines of code
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

        private Tile GetNeighbor(Tile current, direction x) //sets neighbor in context of possible movement ONLY!!!!
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
