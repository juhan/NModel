using System;
using NModel;
using NModel.Terms;
using NModel.Attributes;
using NModel.Execution;

namespace DiningPhilosophers
{
    public enum State
    {
        Thinking, Waiting, Eating, Finishing
    }

    
    public enum Mode
    { Initializing, Running }

    public class Philosopher : LabeledInstance<Philosopher>
    {
        public State state;
        public Fork left, right;

        public override void Initialize()
        {
            state = State.Thinking;
        }

        public bool leftFree()
        {
            return left.isFree();
        }

        public bool rightFree()
        {
            return right.isFree();
        }
    }

    public class Fork : LabeledInstance<Fork>
    {
        public Philosopher hasMe;

        public override void Initialize()
        {
            hasMe = default(Philosopher);
        }

        public bool isFree()
        {
            return hasMe == default(Philosopher);
        }

        public void take(Philosopher p)
        {
            hasMe = p;
        }

        public void release(Philosopher p)
        {
            hasMe = default(Philosopher);
        }
    }


    public static class Contract
    {
        public static Set<Philosopher> phils = Set<Philosopher>.EmptySet;
        public static Mode mode = Mode.Initializing;
        public const int numberOfPhilosophers = 3;

        [Action]
        public static void Init()
        {
            Philosopher[] tmpP = new Philosopher[numberOfPhilosophers];
            Fork[] tmpF = new Fork[numberOfPhilosophers];
            for (int i = 0; i < numberOfPhilosophers; i++) tmpF[i] = Fork.Create();
            for (int i = 0; i < numberOfPhilosophers; i++)
            {
                Philosopher p = Philosopher.Create();
                p.left = tmpF[i];
                p.right = tmpF[(i + 1) % numberOfPhilosophers];
                tmpP[i] = p;
            }
            for (int i = 0; i < numberOfPhilosophers; i++)
                phils = phils.Add(tmpP[i]);
            mode = Mode.Running;
        }
        public static bool InitEnabled() { return (mode == Mode.Initializing); }

        [Action]
        public static void TakeLeft([Domain("phils")] Philosopher p)
        {
            p.left.take(p);
            p.state = State.Waiting;
        }
        public static bool TakeLeftEnabled(Philosopher p)
        { return mode == Mode.Running && p.leftFree() && p.state == State.Thinking; }

        [Action]
        public static void TakeRight([Domain("phils")] Philosopher p)
        {
            p.right.take(p);
            p.state = State.Eating;
        }
        public static bool TakeRightEnabled(Philosopher p)
        { return mode == Mode.Running && p.rightFree() && p.state == State.Waiting; }

        [Action]
        public static void ReleaseLeft([Domain("phils")] Philosopher p)
        {
            p.left.release(p);
            p.state = State.Finishing;
        }
        public static bool ReleaseLeftEnabled(Philosopher p)
        { return mode == Mode.Running && p.state == State.Eating; }

        [Action]
        public static void ReleaseRight([Domain("phils")] Philosopher p)
        {
            p.right.release(p);
            p.state = State.Thinking;
        }
        public static bool ReleaseRightEnabled(Philosopher p)
        { return mode == Mode.Running && p.state == State.Finishing; }


        public static LibraryModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly,
                        "DiningPhilosophers");
        }
    }
}
