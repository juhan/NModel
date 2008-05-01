using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
namespace EmptyCoffeeMachine
{
    /// <summary>
    /// The possible coins that can be inserted into the coffee machine
    /// </summary>
    public enum Coin 
    {
        /// <summary>
        /// A coin worth five cents
        /// </summary>
        Nickel = 5,

        /// <summary>
        /// A coin worth ten cents
        /// </summary>
        Dime = 10
    }

    /// <summary>
    /// Models an empty coffee machine. The only actions one can do is 
    /// to insert coins and get them returned by pushing the cancel button.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The cancel button has been pressed
        /// </summary>
        static bool cancelled = false;

        /// <summary>
        /// The bag (multiset) of coins inserted so far into the machine
        /// </summary>
        static Bag<Coin> coins = Bag<Coin>.EmptyBag;

        /// <summary>
        /// Insert a new coin into the coffee machine. 
        /// A coin cannot be inserted if the cancel button is pressed.
        /// </summary>
        /// <param name="coin">the coin to be inserted</param>
        [Action]
        static void Insert(Coin coin)
        {
            coins = coins.Add(coin);
        }
        static bool InsertEnabled(Coin coin)
        {
            return !cancelled;
        }

        /// <summary>
        /// Press the cancel button. Once it has been pressed it cannot be pressed again 
        /// until a coin has been returned. Moreover, the cancel button cannot be 
        /// pressed if there are no coins in the vending machine.
        /// </summary>
        [Action]
        static void Cancel()
        {
            cancelled = true;
        }
        static bool CancelEnabled()
        {
            return !cancelled && coins.Count > 0;
        }

        /// <summary>
        /// One coin is returned after the cancel button is pressed. 
        /// The cancelled status of the machine is reset to false and 
        /// the returned coin is removed from the bag of coins that 
        /// were inserted into the machine. 
        /// </summary>
        /// <param name="coin">the coin being returned</param>
        [Action]
        static void Return(Coin coin)
        {
            coins = coins.Remove(coin);
            cancelled = false;
        }

        static bool ReturnEnabled(Coin coin)
        {
            return cancelled && coins.Contains(coin);
        }

        [AcceptingStateCondition]
        static bool IsAccepting
        {
            get
            {
                return coins.IsEmpty;
            }
        }

        /// <summary>
        /// Factory method
        /// </summary>
        public static ModelProgram Make()
        {
            return new LibraryModelProgram(typeof(Model).Assembly, "EmptyCoffeeMachine");
        }
    }
}
