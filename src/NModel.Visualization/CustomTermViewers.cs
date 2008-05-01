//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;
using System.Text;
using NModel.Terms;

namespace NModel.Visualization
{
    //Term viewers here are used to define how terms are displayed in a property grid

    internal class TermViewer : DefaultCustomPropertyConverter
    {
        internal Term t;
        protected TermViewer[] argumentViewers;
        internal string keyName;
        protected string repr;
        internal TermViewer(string keyName, Term t)
        {
            this.keyName = keyName;
            this.t = t;
            argumentViewers = new TermViewer[t.Arguments.Count];
            for (int i = 0; i < t.Arguments.Count; i++)
                argumentViewers[i] = Create("Argument " + i, t.Arguments[i]);
        }
        internal TermViewer() { }

        internal static bool TermNeedsNoBrowsing(Term t)
        {
            return t is Literal; //|| t.Arguments.Forall(delegate(Term s) { return s is Literal; });
        }

        internal static TermViewer Create(string keyName, Term t)
        {
            CompoundTerm tc = t as CompoundTerm;
            if (tc != null && tc.Arguments.Count > 0)
            {
                switch (tc.Symbol.Name)
                {
                    case "Map":
                        return new MapViewer(keyName, tc, false);
                    case "Sequence":
                        return new SequenceViewer(keyName, tc);
                    case "Set":
                        return new SetViewer(keyName, tc);
                    case "Bag":
                        return new MapViewer(keyName, tc, true);
                    default:
                        {
                            if (TermNeedsNoBrowsing(t))
                                return new ElementViewer(keyName, t);
                            else
                                return new TermViewer(keyName, t);
                        }
                }
            }
            else
                return new ElementViewer(keyName, t);
        }

        public override ICollection GetKeys()
        {
            return argumentViewers;
        }

        public override object ValueOf(object key)
        {
            ElementViewer ev = key as ElementViewer;
            if (ev != null) return ev.t.ToCompactString();
            else return key;
        }

        public override string CategoryOf(object key)
        {
            return "Term";
        }

        public override string ToString()
        {
            return t.ToCompactString();
        }

        public override string DisplayNameOf(object key)
        {
            return ((TermViewer)key).keyName;
        }

        public override string DescriptionOf(object key)
        {
            return ((TermViewer)key).t.ToString();
        }

        public override bool IsDefaultExpanded(object key)
        {
            return (key is ElementViewer);
        }
    }

    internal class MapletViewer : TermViewer
    {
        internal Term k;
        internal MapletViewer(Term k, Term t, bool isBagEntry)
        {
            this.keyName = "Entry";
            this.t = t;
            this.k = k;
            this.argumentViewers = new TermViewer[2];
            this.argumentViewers[0] = TermViewer.Create((isBagEntry ? "Element" : "Key"), k);
            this.argumentViewers[1] = TermViewer.Create((isBagEntry ? "Multiplicity" : "Value"), t);
        }

        public override string ToString()
        {
            if (repr == null)
                repr = argumentViewers[0].ToString() + " -> " +
                  argumentViewers[1].ToString();
            return repr;
        }

        public override bool IsDefaultExpanded(object key)
        {
            return (key is ElementViewer);
        }
    }

    internal class MapViewer : TermViewer
    {
        bool isBag;
        internal MapViewer(string keyName, CompoundTerm t, bool isBag)
        {
            this.isBag = isBag;
            this.t = t;
            this.keyName = keyName;
            this.argumentViewers = new TermViewer[t.Arguments.Count/2];
            for (int i = 0; i < argumentViewers.Length; i++)
            {
                argumentViewers[i] = new MapletViewer(t.Arguments[2 * i], t.Arguments[(2 * i) + 1], isBag);
            }
        }
        public override string DescriptionOf(object key)
        {
            MapletViewer mv = (MapletViewer)key;
            return (isBag ? "" : "Key: ") + mv.k.ToString() + "\n" +
                (isBag ? "Multiplicity: " : "Value: ") + mv.t.ToString();
        }
    }

    internal class SequenceViewer : TermViewer
    {
        internal SequenceViewer(string keyName, CompoundTerm t)
        {
            this.t = t;
            this.keyName = keyName;
            this.argumentViewers = new TermViewer[t.Arguments.Count];
            for (int i = 0; i < t.Arguments.Count; i++)
                argumentViewers[i] = Create("Element " + i.ToString(MaxPositions(t.Arguments.Count)), t.Arguments[i]);
        }

        static string MaxPositions(int count)
        {
            if (count <= 10) return "0";
            else if (count < 100) return "00";
            else if (count < 1000) return "000";
            else  return "0000";
        }
    }

    internal class SetViewer : TermViewer
    {
        internal SetViewer(string keyName, CompoundTerm t)
        {
            this.t = t;
            this.keyName = keyName;
            this.argumentViewers = new TermViewer[t.Arguments.Count];
            for (int i = 0; i < t.Arguments.Count; i++)
                argumentViewers[i] = Create("Member", t.Arguments[i]);
        }
    }

    internal class ElementViewer : TermViewer
    {
        internal ElementViewer(string keyName, Term t)
        {
            this.t = t;
            this.keyName = keyName;
            this.argumentViewers = new TermViewer[] { };
        }
    }
}