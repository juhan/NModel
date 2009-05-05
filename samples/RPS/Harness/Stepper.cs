using System;
using System.Collections.Generic;
using System.Text;

using NModel.Conformance;
using NModel.Terms;

using System.Drawing;
using System.Threading;

using WinFormUnitDrive;


namespace WinFormHarness
{
    public class Stepper : IStepper
    {
        static EventWaitHandle wh = new AutoResetEvent(false); // For a thread that times-out after the last test-case 

        // Get a unitDrive object
        private const string formName = "WinFormImpl.WinFormImpl";
        //string path = "..\\..\\..\\WinFormImpl\\bin\\Debug\\WinFormImpl.exe";
        private const string path = "..\\WinFormImpl\\bin\\Debug\\WinFormImpl.exe";//"WinFormImpl.exe";
        private static UnitDrive unitDrive = null;

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="action">the given action</param>
        /// <returns>the returned action (or null)</returns>
        public CompoundTerm DoAction(CompoundTerm action)
        {            
            switch (action.FunctionSymbol.ToString())
            {                    
                case "SetPlayer1":                    
                    wh.Set(); // Signal the waiting thread to proceed (test-cases continue, don't exit)
                    Console.WriteLine("\nSetting Player1 to " + (string)action[0]);
                    unitDrive.setPlayer("Player1", (string)action[0]);
                    return null;
                case "SetPlayer2":
                    Console.WriteLine("\nSetting Player2 to " + (string)action[0]);
                    unitDrive.setPlayer("Player2", (string)action[0]);
                    return null;                     
                case "ReadLastResult_Start":
                    Console.WriteLine("\nChecking the results list-box");
                    Thread.Sleep(200);
                    return CompoundTerm.Create("ReadLastResult_Finish", unitDrive.getLastResult());
                default:
                    throw new InvalidOperationException("Unrecognized action: " + action.ToString());
            }
        }

        /// <summary>
        /// Create new IUT
        /// </summary>
        public void Reset()
        {            
            if (unitDrive != null)
            {                
                Console.WriteLine("\nClose the implementation window");                
                unitDrive.closeImplWindow();                
            }
            if (unitDrive == null)
                unitDrive = UnitDrive.getUnitDrive(formName, path);
            unitDrive.LaunchApp();
            Thread.Sleep(1000);
            // Start the waiting for timout thread
            // Will exit the IUT after the last test-case
            new Thread(exitAppInTimeout).Start();
            // Move form           
            //unitDrive.moveForm(new Point(500, 500));  
        }

        /// <summary>
        /// Factory method that provides the IStepper interface for testing
        /// </summary>
        /// <returns>the interface for testing</returns>
        public static Stepper Create()
        {
            return new Stepper();
        }

        private Stepper()
        {            
            this.Reset();
        }

        // Exit the IUT after the last test-case
        // (when a timeout occured without test-case action)
        static void exitAppInTimeout()
        {
            if (!wh.WaitOne(1000)) // Wait for notification, returns false when timeout
            {
                Console.WriteLine("\nTest run stopped. Exit the implementation for the last time");                
                unitDrive.closeImplWindow();                

            }
            else
                Console.WriteLine("Exit thread notified");
        }

    }
}
