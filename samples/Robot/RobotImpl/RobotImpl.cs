using System;
using System.Collections.Generic;
using NModel.Conformance;
using NModel.Terms;
using NModel.Attributes;
using NModel;

namespace RobotImpl1
{
    class RobotImpl
    {
        internal Set<int> aliveRobots ;
        internal int maxId ;
        internal Dictionary<int,double> power;
        internal Dictionary<int, double> reward;
        
        public RobotImpl()
        {
            this.aliveRobots = new Set<int>();
            this.maxId = 1;
            this.power = new Dictionary<int, double>();
            this.reward = new Dictionary<int, double>();
        }
        
        public void AddRobot(int newId)
        {
            if (newId == maxId)
            {
                this.maxId = this.maxId+1;
                aliveRobots = aliveRobots.Add(newId);
                power.Add(newId, 1.0);
                reward.Add(newId, 0.0);
            }
        }
        public void KillRobot(int id)
        {
            if (aliveRobots.Contains(id)){
                aliveRobots  = aliveRobots.Remove(id);
                power.Remove(id);
                reward.Remove(id);
            }
        }
        public void Search(int id)
        {
            if (aliveRobots.Contains(id))
            {
                reward[id] = reward[id] + 1.0;
                power[id] = power[id] - 0.5;
            }
        }
        public void Wait(int id)
        {
            if (aliveRobots.Contains(id))
            {
                power[id] = power[id] - 0.1;
            }
        }
        public CompoundTerm Recharge(int id)
        {
            if (aliveRobots.Contains(id))
            {
                power[id] = power[id] + 1.0;
                reward[id] = reward[id] - 1.5;
                Random r = new Random();
                if (r.Next(0,2) == 0)
                {
                    return new CompoundTerm(Symbol.Parse("Recharge"), CompoundTerm.Create("Robot", id));
                }
            }
           return null;
           
        }
    }
    public class Stepper : IStepper
    {
        RobotImpl impl = new RobotImpl();
        public CompoundTerm DoAction(CompoundTerm action)
        {
            System.Console.WriteLine("robot" + action.Name + action.Arguments[0].ToString());
            int id;
            switch (action.FunctionSymbol.ToString())
            {                 
                case ("CreateRobot"):
                    id = (int)((CompoundTerm)action[0])[0];
                    impl.AddRobot(id);
                    return null;

                case ("DeleteRobot"):
                    id = (int)((CompoundTerm)action[0])[0];
                    impl.KillRobot(id);
                    return null;

                case ("Search"):
                    id = (int)((CompoundTerm)action[0])[0];
                    impl.Search(id);
                    return impl.Recharge(id);
                    
                case("Wait"):
                    id = (int)((CompoundTerm)action[0])[0];
                    impl.Wait(id);
                    return null;

               /* case("Recharge"):
                    id = (int)((CompoundTerm)action[0])[0];
                    impl.Recharge(id);
                    return null;
*/
                case("CheckState"):
                    id = (int)action[0];
                    if (id != impl.maxId)
                        throw new Exception("Wrong Number of Robots in Model and Implementation");
                    else
                        return null;

                default:
                    throw new Exception("Unexpected action " + action); 
            }            
        }
        public void Reset()
        {
            impl = new RobotImpl();
        }
        public static Stepper Create()
        {
            return new Stepper();
        }
    }
}
