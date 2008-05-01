using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using NModel.Terms;

namespace RobotModel1
{
    public class Robot:CompoundValue
    {
        [ExcludeFromState]
        public readonly int id;
        public Robot(int i){this.id = i;}           
    }
    static class RobotModel
    {
        //[ExcludeFromState]
        static int nextId = 1;
        //[ExcludeFromState]
        static int maxNoRobots = 1;
        static Set<Robot> robots = Set<Robot>.EmptySet;
        static Map<Robot, double> power = Map<Robot, double>.EmptyMap;
        static Map<Robot, double> reward = Map<Robot, double>.EmptyMap;
        [ExcludeFromState]
        static internal Set<int> maxCountSet = Set<int>.EmptySet;

        static Set<Robot> NextRobot()
        {
            return new Set<Robot>(new Robot(nextId));
        }
        [Action]
        static void CreateRobot([Domain("NextRobot")]Robot r)
        {
            maxCountSet = new Set<int>(nextId);
            nextId = nextId + 1;
            robots = robots.Add(r);
            power = power.Add(r, 1.0);
            reward = reward.Add(r, 0.0);
        }
       // [Requirement("nextId<=maxNoOfRobots")]
       [Requirement("Cond(Params(),Leq(nextId,maxNoRobots))")]
        static bool CreateRobotEnabled1(Robot r)
        {
            return nextId <= maxNoRobots;
        }
        //[Requirement("!robots.Contains"+ (string)r.id)]
       // [Requirement("Not(Contains(robots,r)))")]        
        static bool CreateRobotEnabled2(Robot r)
        {
            return !robots.Contains(r);
        }
        [Action]
        static void DeleteRobot([Domain("robots")]Robot r)
        {
            robots = robots.Remove(r);
            power = power.RemoveKey(r);
            reward = reward.RemoveKey(r);
        }
        //[Requirement("Contains(r)")]
        [Requirement("Contains(robots,r))")]     
        static bool DeleteRobotEnabled1(Robot r)
        {
            return robots.Contains(r); 
        }
        //[Requirement("powercontains(r)")]
        static bool DeleteRobotEnabled2(Robot r)
        {
            return power.ContainsKey(r); 
        }
        //[Requirement("rewardcontains(r)")]
        static bool DeleteRobotEnabled3(Robot r)
        {
            return reward.ContainsKey(r);
        }

        [Action]
        static void Search([Domain("robots")]Robot r)
        {
            reward =  reward.Override(r,reward[r] + 1.0);
            power = power.Override(r,power[r] - 0.5);
        }
        //[Requirement("robotsContains(r)")]
        [Requirement("Contains(robots,r))")]     
        static bool SearchEnabled1(Robot r)
        {
            return robots.Contains(r);
        }
        [Requirement("geq(power(r),double(\"0.5\"))")]
        static bool SearchEnabled2(Robot r)
        {
            return power[r] >= 0.5;
        }
        [Action]
        static void Wait([Domain("robots")]Robot r)
        {
            power = power.Override(r, power[r] - 0.1);
        }
        //[Requirement("robotsContains(r)")]
        [Requirement("Contains(robots,r))")]     
        static bool WaitEnabled1(Robot r)
        {
            return robots.Contains(r); 
        }
        [Requirement("geq(power(r),double(\"0.1\"))")]
        static bool WaitEnabled2(Robot r)
        {
            return power[r] >= 0.1;
        }
        [Action]
        static void Recharge([Domain("robots")]Robot r)
        {
            power = power.Override(r, power[r] + 1.0);
            reward = reward.Override(r, reward[r] - 0.6);
        }
       // [Requirement("robotsContains(r)")]
        [Requirement("Contains(robots,r))")]     
        static bool RechargeEnabled1(Robot r)
        {
            return robots.Contains(r) ;
        }
        [Requirement("geq(reward(r),double(\"0.6\"))")]
        static bool RechargeEnabled2(Robot r)
        {
            return reward[r] > 0.6;
        }
        
        //[Action]
        static void CheckState([Domain("maxCountSet")] int i)
        {
        }
        static bool CheckStateEnabled(int i)
        {
            return i == nextId - 1;
        }
    }
    public static class Factory
    {
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(RobotModel).Assembly,
                "RobotModel1", new Set<string>());
        }
    }
}
