using NModel;
using NModel.Terms;
using NModel.Attributes;
using NModel.Execution;

namespace TriangleWithEnums
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
//        MAGENTA,
//        YELLOW,
//        CYAN
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

        public static LibraryModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                        "TriangleWithEnums");
        }
    }



}

namespace TriangleWithLabeledInstances
{
    public static class Contract
    {

        public static Mode mode = Mode.Initializing;

        /// <summary>
        /// The triangle consists of three sides.
        /// </summary>
        public static Set<Side> triangle = Set<Side>.EmptySet;
        // it is currently not allowed to populate sets before the Init() action. 
        // new Set<Side>(Side.Create(),Side.Create(), Side.Create());
        public static Set<Color> colors = Set<Color>.EmptySet;//new Set<Color>(Color.BLUE, Color.RED);


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

        public static LibraryModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                        "TriangleWithLabeledInstances");
        }
    }

    public enum Mode
    {
        Initializing,
        Coloring
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