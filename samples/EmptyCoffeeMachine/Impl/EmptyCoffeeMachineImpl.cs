using System;
using System.Collections.Generic;
using NModel.Conformance;
using NModel.Terms;
using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace EmptyCoffeeMachineImpl
{
    [Sort("Coin")]
    internal enum Coin
    {
        Nickel,
        Dime
    }

    /// <summary>
    /// Represents a sample implementation of an empty coffee machine
    /// </summary>
    internal class EmptyCoffeeMachineImpl
    {
        internal EmptyCoffeeMachineImpl() { }

        /// <summary>
        /// The bag of coins is implemented as a list of coins
        /// </summary>
        internal static List<Coin> coins = new List<Coin>();

        /// <summary>
        /// A new coin is inserted
        /// </summary>
        internal static void InsertACoin(Coin coin)
        {
            coins.Add(coin);
        }

        /// <summary>
        /// Cancel button is pressed and a coin is returned 
        /// </summary>
        internal static Coin Cancel()
        {
            if (coins.Count > 0)
            {
                //a coin is selected randomly and returned
                Random rand = new Random();
                int i = rand.Next(coins.Count);
                Coin coin = coins[i];
                coins.RemoveAt(i);
                return coin;
            }
            else
                throw new InvalidOperationException("Cannot cancel when no coins have been inserted");
        }
    }

    /// <summary>
    /// Implements the IStepper interface for testing EmptyCoffeeMachineImpl
    /// </summary>
    public class Stepper : IStepper
    {
        public Stepper() { }
        #region IStepper Members

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="action">the given action</param>
        /// <returns>the returned action (or null)</returns>
        public CompoundTerm DoAction(CompoundTerm action)
        {
            switch (action.FunctionSymbol.ToString())
            {
                case "Insert":
                    {
                        Coin coin = GetCoin(action.Arguments[0]);
                        EmptyCoffeeMachineImpl.InsertACoin(coin);
                        return null;
                    }
                case "Cancel":
                    {
                        Term coin = CompoundValue.GetTerm(EmptyCoffeeMachineImpl.Cancel());
                        return new CompoundTerm(Symbol.Parse("Return"), coin);
                    }
                default:
                    throw new InvalidOperationException("Unrecognized action: " + action.ToString());
            }
        }

        /// <summary>
        /// Removes all the coins from the machine
        /// </summary>
        public void Reset()
        {
            EmptyCoffeeMachineImpl.coins.Clear();
        }

        #endregion

        static Coin GetCoin(Term t)
        {
            if (t.ToString().Equals("Coin(\"Nickel\")"))
                return Coin.Nickel;
            else if (t.ToString().Equals("Coin(\"Dime\")"))
                return Coin.Dime;
            else
                throw new ArgumentException("Unrecognized coin: " + t.ToString());
        }

        /// <summary>
        /// Factory method that provides the IStepper interface for testing
        /// </summary>
        /// <returns>the interface for testing</returns>
        public static IStepper Make()
        {
            return new Stepper();
        }
    }
}
