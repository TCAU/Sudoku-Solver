using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_Solver
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Title = "Sudoku Solver";

            while (true)
            {
                Sudoku s;
                int read;
                Console.Write("Do you want to enter your own Sudoku values (y) or use one of the tests? (n)\n");
                do
                {
                    read = Console.Read();
                } while (read != 121 && read != 110); 

                if (read == 121)
                {
                    s = new Sudoku(GetUserInput());
                }
                else
                {
                    s = new Sudoku(GetTest());
                }

                Console.WriteLine("Intial: ");
                s.DisplayGrid();
                s.Solve();
                Console.WriteLine("Solved: ");
                s.DisplayGrid();
                System.Threading.Thread.Sleep(1000);
            }
        }

        internal class Sudoku
        {
            //main sudoku grid
            internal int[] grid = new int[81];

            //arrays of lists of all possible values per tile
            internal List<int>[] possibleValuesAtIndex = new List<int>[81];

            //string used to parse grid
            internal string sudokuString;

            //lists to contain values that were already entered, 
            //and thus can't be in also be somewhere else in the same row/column/block 
            internal List<int>[] AlreadyEntered_Rows = new List<int>[9];
            internal List<int>[] AlreadyEntered_Columns = new List<int>[9];
            internal List<int>[] AlreadyEntered_Blocks = new List<int>[9];

            //index values for backtracking
            int indexCurrent = -1;
            Stack<int> previousIndex = new Stack<int>();

            internal Sudoku(string str)
            {
                sudokuString = str;
                for (int x = 0; x < 81; x++)
                    grid[x] = 0;

                InitializeGrid();
                InitializeAlreadyEnteredValueLists();
                InitializePossibleValuesList();
            }

            private void ApplyValuesThatAreSingledOut()
            {                
                int i = 0;
                while (i < 81)
                {
                    if (possibleValuesAtIndex[i].Count() == 1 && grid[i] == 0)  //Only one value exists in possible values list for a blank cell
                    {
                        int value = possibleValuesAtIndex[i].First();

                        grid[i] = value; //enter that value to the grid

                        //enter that value for that row / column / block list
                        AlreadyEntered_Rows[i / 9].Add(value);
                        AlreadyEntered_Columns[i % 9].Add(value);
                        AlreadyEntered_Blocks[blockedSections[i]].Add(value);
                        ReInitializePossibleValuesList();
                        
                        i = -1; // start over again, since there might be another blank cell with a single possible value
                    }
                    i++;
                }

                i = 0;

                //set the backtracking index at the first cell with a blank value
                while (indexCurrent == -1)
                {
                    if (i == 80 //last cell 
                        || grid[i] == 0) // cell has a blank value
                    {
                        indexCurrent = i;
                    }
                    i++;
                }
            }

            internal void Solve()
            {
                ApplyValuesThatAreSingledOut();

                int lastBlankCell = 80; //start from last cell

                while (lastBlankCell > 0 //stop at first cell or  
                        && grid[lastBlankCell] > 0) //found blank cell  
                    lastBlankCell--;

                while (grid[lastBlankCell] == 0) //while last blank cell is still blank
                {
                    Console.WriteLine("index is now " + indexCurrent);
                    DisplayGrid();
                    if (CellsAllHavePossibleValues())
                    {
                        Guess();
                        getNextIndex();
                    }
                    else
                    {
                            getLastIndex();
                            Guess();
                            getNextIndex();
                    }
                }
            }

            private void getNextIndex()
            {
                //save previous index
                previousIndex.Push(indexCurrent);

                //get next index of empty cell
                while (grid[indexCurrent] != 0 && indexCurrent!= 80)
                    indexCurrent++;
            }

            private void getLastIndex()
            {
                indexCurrent = previousIndex.Pop();
            }

            private bool CellsAllHavePossibleValues()
            {
                //if a previous choice has made it so that one of the blank tiles
                //has no value to choose from, we made the wrong choice and need to backtrack
                for (int i = 0; i < 81; i++)
                {
                    if (possibleValuesAtIndex[i].Count() == 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            private void BackTrack()
            {
                getLastIndex();
                Guess();
            }

            private void Guess()
            {
                int previousValue;

                if (grid[indexCurrent] == 0) // a value was not guessed yet
                {
                    previousValue = grid[indexCurrent] = possibleValuesAtIndex[indexCurrent].First(); // set to first possible value

                    //update possible values lists
                    AddToValueEnteredLists(previousValue);
                }
                else //we already guessed at the value
                {
                    int listIndex = possibleValuesAtIndex[indexCurrent].IndexOf(grid[indexCurrent]); //index of the previous value in the list

                    previousValue = possibleValuesAtIndex[indexCurrent].ElementAt(listIndex); //get previous value

                    RemoveFromValueEnteredLists(previousValue); //remove previous value from rows/columns/blocks

                    Console.WriteLine("Backtrack. Previous value " + previousValue +" removed at index " + indexCurrent + ".");

                    if (grid[indexCurrent] != possibleValuesAtIndex[indexCurrent].Last()) // if the value is not the last possible value for that list
                    {
                        int newValue= possibleValuesAtIndex[indexCurrent].ElementAt(listIndex + 1);
                        grid[indexCurrent] = newValue; // add next value to cell
                        AddToValueEnteredLists(newValue); // add next value to all rows / column / block lists

                        Console.WriteLine("Next value added " + previousValue);
                    }
                }
            }

            private void RemoveFromValueEnteredLists(int value)
            {
                AlreadyEntered_Rows[indexCurrent / 9].Remove(value);
                AlreadyEntered_Columns[indexCurrent % 9].Remove(value);
                AlreadyEntered_Blocks[blockedSections[indexCurrent]].Remove(value);
                ReInitializePossibleValuesList();
            }

            private void AddToValueEnteredLists(int value)
            {
                AlreadyEntered_Rows[indexCurrent / 9].Add(value);
                AlreadyEntered_Columns[indexCurrent % 9].Add(value);
                AlreadyEntered_Blocks[blockedSections[indexCurrent]].Add(value);
                ReInitializePossibleValuesList();
            }

            private bool isSudokuSolved()
            {
                for (int i = 0; i < 9; i++)
                {
                    //sudoku is solved when all rows, columns and block lists have all 9 digits from 1-9
                    if (AlreadyEntered_Rows[i].Count() != 9 ||
                        AlreadyEntered_Columns[i].Count() != 9 ||
                        AlreadyEntered_Blocks[i].Count() != 9)
                            return false;
                }

                return true;
            }

            int[] blockedSections = new int[]             
            { 0, 0, 0, 1, 1, 1, 2, 2, 2, 
              0, 0, 0, 1, 1, 1, 2, 2, 2,  
              0, 0, 0, 1, 1, 1, 2, 2, 2,  
              3, 3, 3, 4, 4, 4, 5, 5, 5, 
              3, 3, 3, 4, 4, 4, 5, 5, 5, 
              3, 3, 3, 4, 4, 4, 5, 5, 5,  
              6, 6, 6, 7, 7, 7, 8, 8, 8, 
              6, 6, 6, 7, 7, 7, 8, 8, 8, 
              6, 6, 6, 7, 7, 7, 8, 8, 8};

            private void InitializePossibleValuesList()
            {
                for (int i = 0; i < 81; i++)
                {
                    possibleValuesAtIndex[i] = new List<int>();

                    if (grid[i]>0) //grid has a value entered already
                    {
                        possibleValuesAtIndex[i].Add(grid[i]); //so there's only one possible value
                    }
                    else if(grid[i] == 0)
                    {
                        for (int x = 1; x <= 9; x++)
                        {
                            if (NotInAlreadyEnteredLists(i, x))//if this number is not already taken for that row/column/block
                            {
                                possibleValuesAtIndex[i].Add(x); //add that number to the possible values for that cell
                            }
                        }
                    }
                }
            }

            private void ReInitializePossibleValuesList()
            {
                for (int i = 0; i < 81; i++)
                {
                    if (grid[i] == 0)
                    {
                        possibleValuesAtIndex[i].Clear();
                        for (int x = 1; x <= 9; x++)
                        {
                            if (NotInAlreadyEnteredLists(i,x))
                            {
                                possibleValuesAtIndex[i].Add(x); //add all possible values
                            }
                        }
                    }
                }
            }

            private bool NotInAlreadyEnteredLists(int index, int value)
            {
                return         (!AlreadyEntered_Rows[index / 9].Contains(value) &&
                                !AlreadyEntered_Columns[index % 9].Contains(value) &&
                                !AlreadyEntered_Blocks[blockedSections[index]].Contains(value));
            }

            private void InitializeAlreadyEnteredValueLists()
            {
                for (int i = 0; i < 9; i++)
                {
                    AlreadyEntered_Rows[i] = new List<int>();
                    AlreadyEntered_Columns[i] = new List<int>();
                    AlreadyEntered_Blocks[i] = new List<int>();
                }

                for (int x = 0; x < 81; x++)
                {
                    if (grid[x] > 0) { AlreadyEntered_Rows[x / 9].Add(grid[x]); }
                    if (grid[x] > 0) { AlreadyEntered_Columns[x % 9].Add(grid[x]); }
                    if (grid[x] > 0) { AlreadyEntered_Blocks[blockedSections[x]].Add(grid[x]); }
                }
            }

            public void DisplayGrid()
            {
                for (int x = 0; x < 81; x++)
                {
                    if (grid[x]==0)
                        Console.Write("[ ]");
                    else
                        Console.Write("[" + grid[x] + "]");
                    if ((x + 1) % 9 == 0) { Console.WriteLine(); }
                }
                Console.WriteLine();
            }

            private void InitializeGrid()
            {
                for (int x = 0; x < sudokuString.Length; x++)
                {
                    int v = (int)Char.GetNumericValue(sudokuString[x]);
                    grid[x] = v;
                }
            }
        }

        private static string GetUserInput()
        {
            StringBuilder userString = new StringBuilder();
            int row = 1;

            while (row < 10)
            {
                string input = "0";
                int dummy;
                Console.WriteLine("Please Enter row " + row);
                input = Console.ReadLine();

                while (input.Length != 9 || !int.TryParse(input, out dummy))
                {
                    Console.WriteLine(@"For each row, please enter eight numeric characters.
Blank cells should be entered as a zero");
                    input = Console.ReadLine();
                }

                userString.Append(input);
                row++;
            }

            return userString.ToString();
        }       

        private static string GetTest()
        {
            Random r = new Random();
            string testString;

            switch (r.Next(0,6))
            {
                case 1:
                    testString = "530070000" +
                                 "600195000" +
                                 "098000060" +
                                 "800060003" +
                                 "400803001" +
                                 "700020006" +
                                 "060000280" +
                                 "000419005" +
                                 "000080079";
                    break;
                case 2: //easy
                    testString = "007030800" +
                                 "000205000" +
                                 "400906001" +
                                 "043000210" +
                                 "100000005" +
                                 "058000670" +
                                 "500108009" +
                                 "000503000" +
                                 "002090500";
                    break;
                case 3:
                    testString = "003092000" +
                                 "400030010" +
                                 "270000000" +
                                 "010300008" +
                                 "050167030" +
                                 "300008060" +
                                 "000000053" +
                                 "030080009" +
                                 "000620100";
                    break;
                case 4: //empty
                    testString = "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000" +
                                 "000000000";
                    break;
                case 5:
                    testString = "503000700" +
                                 "000800006" +
                                 "070060040" +
                                 "040100000" +
                                 "708050309" +
                                 "000009060" +
                                 "050010070" +
                                 "600004000" +
                                 "002000503";
                    break;
                default: //"World's hardest sudoku" from The Telegraph, probably just clickbait
                    testString = "800000000" +
                                 "003600000" +
                                 "070090200" +
                                 "050007000" +
                                 "000045700" +
                                 "000100030" +
                                 "001000068" +
                                 "008500010" +
                                 "090000400";
                    break;
            }
            return testString;
        }
    }
}
