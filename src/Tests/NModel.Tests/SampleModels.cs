using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace SampleModels.PowerSwitch
{
    public enum Power { On, Off };
    public static class Contract
    {
        static Power power = Power.Off;

        [Action]
        static void PowerOn() { power = Power.On; }
        static bool PowerOnEnabled() { return power == Power.Off; }

        [Action]
        static void PowerOff() { power = Power.Off; }
        static bool PowerOffEnabled() { return power == Power.On; }
    }
}

namespace SampleModels.Fan
{
    public enum CONTROL { Power, Speed };
    [Feature]
    public static class Control
    {
        public static CONTROL focus = CONTROL.Power;

        [Action]
        static void ControlPower() { focus = CONTROL.Power; }
        static bool ControlPowerEnabled() { return focus == CONTROL.Speed; }

        [Action]
        static void ControlSpeed() { focus = CONTROL.Speed; }
        static bool ControlSpeedEnabled() { return focus == CONTROL.Power; }

        [Action("PowerOn")]
        [Action("PowerOff")]
        static void PowerLoop() { }
        static bool PowerLoopEnabled() { return focus == CONTROL.Power; }

        [Action("IncrementSpeed")]
        static void SpeedLoop() { }
        static bool SpeedLoopEnabled() { return focus == CONTROL.Speed; }
    }

    public enum POWER { On, Off };
    [Feature]
    public static class Power
    {
        public static POWER power = POWER.Off;

        [Action]
        static void PowerOn() { power = POWER.On; }
        static bool PowerOnEnabled() { return power == POWER.Off; }

        [Action]
        static void PowerOff() { power = POWER.Off; }
        static bool PowerOffEnabled() { return power == POWER.On; }
    }

    public enum SPEED { Low, Medium, High };
    [Feature]
    public static class Speed
    {
        public static SPEED speed = SPEED.Low;

        [Action]
        static void IncrementSpeed()
        {
            speed = (speed == SPEED.Low ? SPEED.Medium : (speed == SPEED.Medium ? SPEED.High : SPEED.Low));
        }
    }

    [Feature]
    public static class Filter1
    {
        [StateFilter]
        public static bool Filter()
        {
            return !(Control.focus == CONTROL.Speed &&
                   Power.power == POWER.On &&
                   Speed.speed == SPEED.High);
        }
        
    }
}

namespace SampleModels.TriangleWithEnums
{

    [Abstract]
    public enum SideOfTriangle
    {
        S1,
        S2,
        S3
    }

    [Abstract]
    public enum Color
    {
        RED,
        GREEN,
        BLUE,
        MAGENTA,
        YELLOW,
        CYAN
    }

    public static class Contract
    {

        public static Map<SideOfTriangle, Color> colorAssignments = new Map<SideOfTriangle, Color>();

        [Action]
        public static void AssignColor(SideOfTriangle s, Color c)
        {
            colorAssignments = colorAssignments.Add(s, c);
        }
        public static bool AssignColorEnabled(SideOfTriangle s) { return !colorAssignments.ContainsKey(s); }

    }



}

namespace SampleModels.TriangleWithLabeledInstances
{
    public static class Contract
    {

        public static Mode mode = Mode.Initializing;

        /// <summary>
        /// The triangle consists of three sides.
        /// </summary>
        public static Set<Side> triangle = Set<Side>.EmptySet;
        public static Set<Color> colors = Set<Color>.EmptySet;


        [Action]
        static void Init()
        {
            triangle = new Set<Side>(Side.Create(), Side.Create(), Side.Create());
            colors = new Set<Color>(Color.BLUE, Color.RED);
            mode = Mode.Coloring;
        }
        public static bool InitEnabled() { return mode == Mode.Initializing; }

        [Action]
        public static void AssignColor([Domain("triangle")] Side s, [Domain("colors")]Color c)
        {
            s.color = c;
        }
        public static bool AssignColorEnabled(Side s) { return ((mode == Mode.Coloring) && (s.color.Equals(Color.unassigned))); }

    }

    [Sort("ColoringMode")]
    public enum Mode
    {
        Initializing,
        Coloring        // Assigning colors to sides
    }

    [Abstract]
    public enum Color
    {
        unassigned,
        RED,
        BLUE
    }

    public class Side : LabeledInstance<Side>
    {

        public Side() { }

        public Color color = Color.unassigned;
    }
}


