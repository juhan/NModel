using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;

namespace RevisionControl
{
    /// <summary>
    /// Revision control operations
    /// </summary>
    enum Op { Add, Delete, Change }

    /// <summary>
    /// A data record representing a (operation, revisionNumber) pair.
    /// </summary>
    class Revision : CompoundValue
    {
        public readonly Op op;                     // the operation performed
        public readonly int revisionNumber;        // the revision number of the database for this change

        public Revision(Op op, int revisionNumber)
        {
            this.op = op;
            this.revisionNumber = revisionNumber;
        }
    }

    static class Repository
    {
        /// <summary>
        /// The number of times a change has been committed to the repository
        /// </summary>
        public static int currentRevision = 0;

        /// <summary>
        /// The database of revisions. Each entry in the map associates a file name with
        /// the change log for that file. A change log is a sequence of revisions. The
        /// first element of a change log is the most recent revision.
        /// </summary>
        static Map<string, Sequence<Revision>> db = Map<string, Sequence<Revision>>.EmptyMap;

        /// <summary>
        /// For each revision, the check-in message supplied by the user for that revision.
        /// </summary>
        static Sequence<string> revisionMessages = Sequence<string>.EmptySequence;

        /// <summary>
        /// For each revision, the client who requested that revision.
        /// </summary>
        static Sequence<string> revisionClients = Sequence<string>.EmptySequence;

        #region Invariants
        [StateInvariant]
        [Requirement("Revision number of the repository must be nonnegative.")]
        static bool ValidRevision() { return currentRevision >= 0; }

        [StateInvariant]
        [Requirement("The number of check-in messages must match the currrent revision.")]
        static bool ValidMessageCount() { return revisionMessages.Count == currentRevision; }

        [StateInvariant]
        [Requirement("The number of check-in clients must match the currrent revision.")]
        static bool ValidClientCount() { return revisionClients.Count == currentRevision; }

        [StateInvariant]
        [Requirement("The revision numbers of every change log in the repository must be a valid revision number.")]
        static bool ValidDatabaseRevisions()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
            {
                foreach (Revision revision in changeLog)
                {
                    int rev = revision.revisionNumber;
                    if (rev < 0 || rev > currentRevision)
                        return false;
                }
            }
            return true;
        }

        [StateInvariant]
        [Requirement("The revision numbers of every change log in the repository must decreasing.")]
        static bool ValidDatabaseRevisionOrder()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
            {
                int prevRev = -1;
                foreach (Revision revision in changeLog)
                {
                    int rev = revision.revisionNumber;
                    if (prevRev > -1 && rev <= prevRev)
                        return false;
                    prevRev = rev;
                }
            }
            return true;
        }

        [StateInvariant]
        [Requirement("Every change list in the repository must contain at least one change.")]
        static bool ValidDatabaseRevisionLength()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
            {
                if (changeLog.Count < 1)
                    return false;
            }
            return true;
        }

        [StateInvariant]
        [Requirement("The last element in every change list in the repository must be an Add operation.")]
        static bool ValidDatabaseRevisionTail()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
                if (changeLog.Count > 0 && changeLog.Last.op != Op.Add)
                    return false;

            return true;
        } 
        #endregion

        /// <summary>
        /// Returns the version number of the most recent checkin of this file
        /// </summary>
        public static int FileVersion(string file)
        {
            Sequence<Revision> revisions;
            return (db.TryGetValue(file, out revisions) ?
                        revisions.Head.revisionNumber : -1);

        }

        public static bool FileExists(string file)
        {
            Sequence<Revision> revisions;
            if (db.TryGetValue(file, out revisions))
                return (revisions.Head.op != Op.Delete);
            else
                return false;
        }

        /// <summary>
        /// Does the file exist in the given version? 
        /// </summary>
        public static bool FileExists(string file, int version)
        {
            Sequence<Revision> revisions;
            if (db.TryGetValue(file, out revisions))
               foreach (Revision r in revisions)
                    if (r.revisionNumber <= version)
                        return (r.op != Op.Delete);
            return false;
        }

        static public Set<string> CheckForConflicts(int clientRevisionNumber, Map<string, Op> changes)
        {
            Set<string> result = Set<string>.EmptySet;
            foreach (Pair<string, Op> change in changes)
            {
                string file = change.First;
                Op op = change.Second;

                if (FileExists(file) && op == Op.Change && FileVersion(file) > clientRevisionNumber)
                    result = result.Add(file);
            }
            return result;
        }

        public static int Commit(string client, string message, Map<string, Op> changes)
        {
            foreach (Pair<string, Op> change in changes)
            {
                string file = change.First;
                Op op = change.Second;
                Revision revision = new Revision(op, currentRevision + 1);
                Sequence<Revision> revisions;

                if (db.TryGetValue(file, out revisions))
                     db = db.Override(file, revisions.AddFirst(revision));
                else
                     db = db.Add(file, new Sequence<Revision>(revision));
            }

            currentRevision = currentRevision + 1;
            revisionMessages = revisionMessages.AddLast(message);
            revisionClients = revisionClients.AddLast(client);

            return currentRevision;
        }

        static bool CommitEnabled(string client, string message, Map<string, Op> changes)
        {
            if (string.IsNullOrEmpty(client)) return false;
            if (string.IsNullOrEmpty(message)) return false; 
            foreach(Pair<string, Op> change in changes)
            {
                string file = change.First;
                Op op = change.Second;

                if (!db.ContainsKey(file) && op != Op.Add)
                    return false;
            }
            return true;
        }
    }
}
