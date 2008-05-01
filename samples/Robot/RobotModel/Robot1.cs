using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using NModel.Terms;

namespace RobotModel2
{
     static class RobotModel
    {
        static int nextId = 1;
        static Set<int> robots = Set<int>.EmptySet;
        static Map<int, double> power = Map<int, double>.EmptyMap;
        static Map<int, double> reward = Map<int, double>.EmptyMap;

        static Set<int> NextRobot()
        {
            robots = robots.Add(nextId);
            return robots;
        }
        [Action]
        static void CreateRobot([Domain("NextRobot")]int rid)
        {
            nextId = nextId + 1;
            robots = robots.Add(rid);
            power = power.Add(rid, 1.0);
            reward = reward.Add(rid, 0.0);
        }
        static bool CreateRobotEnabled(int rid)
        {
            return !robots.Contains(rid);
        }
        [Action]
        static void DeleteRobot([Domain("robots")]int rid)
        {
            robots = robots.Remove(rid);
            power = power.RemoveKey(rid);
            reward = reward.RemoveKey(rid);
        }
        static bool DeleteRobotEnabled(int rid)
        {
            return robots.Contains(rid) && power.ContainsKey(rid) && reward.ContainsKey(rid);
        }

        [Action]
        static void Search([Domain("robots")]int rid)
        {
            reward = reward.Override(rid, reward[rid] + 1.0);
            power = power.Override(rid, power[rid] - 0.5);
        }
        static bool SearchEnabled(int rid)
        {
            return robots.Contains(rid) && power[rid] >= 0.5;
        }
        [Action]
        static void Wait([Domain("robots")]int rid)
        {
            power = power.Override(rid, power[rid] - 0.1);
        }
        static bool WaitEnabled(int rid)
        {
            return robots.Contains(rid) && power[rid] >= 0.1;
        }
        [Action]
        static void Recharge([Domain("robots")]int rid)
        {
            power = power.Override(rid, power[rid] + 1.0);
            reward = reward.Override(rid, reward[rid] - 1.5);
        }
        static bool RechargeEnabled(int rid)
        {
            return robots.Contains(rid) && power[rid] <= 0.1 && reward[rid] >= 1.5;
        }
    }
    public static class Factory
    {
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(RobotModel).Assembly,
                "RobotModel2", new Set<string>());
        }
    }
}
