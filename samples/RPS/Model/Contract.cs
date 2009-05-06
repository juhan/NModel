using System;
using NModel.Attributes;
using NModel;
using NModel.Execution;

namespace RPSModel
{
    /// <summary>
    /// Types for each state variable
    /// </summary>    
    // Types of game phase
    public enum Phase { set, roundEnd, checkedResults }
    
    static class Contract
    {
        // Control state
        public static string Player1 = "";
        public static string Player2 = "";
        public static Phase phase = Phase.set;        

        // Data 
        // Domain for SetPlayerChoice        
        static Set<string> PlayerChoices()
        {
            return new Set<string>("rock", "scissors", "paper", "wrongInput");
        }

        /// <summary>
        /// Enabling conditions and actions
        /// </summary>
        [Requirement("SEC 0.1.2.3",
            "Added requirement to a guard method")]
        public static bool SetPlayer1Enabled()
        {
            return ( (phase != Phase.roundEnd) && (Player1.Length == 0) );
        }

        [Action]
        [Requirement("SEC 3.1.1.5",
            "An input item must be an item from the following list: " +
            "rock, paper, scissors")]
        [Requirement("SEC 3.1.2.7",
            "A player types-in his/her chosen item to one of the text-boxes and " +
            "clicks the relevant Submit button")]
        public static void SetPlayer1([Domain("PlayerChoices")] string choice)
        {
            Player1 = choice;
            if (Player2.Length > 0)
                phase = Phase.roundEnd;
            else
                phase = Phase.set;
        }

        public static bool SetPlayer2Enabled()
        {
            return ((phase != Phase.roundEnd) && (Player2.Length == 0));
        }

        [Action]        
        public static void SetPlayer2([Domain("PlayerChoices")] string choice)
        {
            Player2 = choice;
            if(Player1.Length > 0)
                phase = Phase.roundEnd;
            else
                phase = Phase.set;
        }
        
        public static bool ReadLastResultEnabled()
        {
            return (phase == Phase.roundEnd);                
        }
        [Action]
        [Requirement("SEC 3.2.1.1", "Items are resolved as follows: Rock blunts scissors; rock wins. Paper covers or captures rock; paper wins. Scissors cut paper; scissors wins. If both players choose the same item, the game is tied")]
        public static string ReadLastResult()
        {
            phase = Phase.checkedResults;
            return computeResults();            
        }

        // Check inputs and copute results 
        // In case of wrong-input returns an error message          
        private static string computeResults()
        {
            string result = "";

            if ((Player1 == "wrongInput") ||
                (Player2 == "wrongInput"))
                result = "Error: wrong input!";
            else if (Player1 == Player2)
                result = "Tie";
            else if (
                (Player1 == "paper" && Player2 == "rock") ||
                (Player1 == "rock" && Player2 == "scissors") ||
                (Player1 == "scissors" && Player2 == "paper"))
                result = "Player1 wins";
            else
                result = "Player2 wins";
            return result;
        }

        public static bool DoNotRunEnabled()
        {
            return false;
        }
        [Action]
        [Requirement("SEC 7.1.2.1",
            "This requirement is in the test suite but not executed")]
        public static void DoNotRun()
        {
            // Nothing
        }

        [AcceptingStateCondition]
        public static bool IsAcceptingState()
        {
            return (phase == Phase.checkedResults);
        }

        /// <summary>
        /// Safty requirement for safty analysis
        /// Should be true in every state
        /// Unsafe state when false
        /// </summary>
        /// <returns>Bool: False in unsafe state</returns>
        [StateInvariant]
        public static bool ComputeRightInputs()
        {
            return (!ReadLastResultEnabled() ||
                ((Player1 != "wrongInput") &&
                (Player2 != "wrongInput")));
        }
    }

}


