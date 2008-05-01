using System;
using NModel;
using NModel.Attributes;

namespace RevisionControl
{
    static class User
    {
        /// <summary>
        /// The last version checked out by each user
        /// </summary>
        static Map<string, int> version = Map<string, int>.EmptyMap;

        /// <summary>
        /// The pending changes made by each user that have not yet been committed to the repository
        /// </summary>
        static Map<string, Map<string, Op>> revisions = Map<string, Map<string, Op>>.EmptyMap;

        /// <summary>
        /// The files that must be resolved before check in may complete
        /// </summary>
        static Map<string, Set<string>> conflicts = Map<string, Set<string>>.EmptyMap;

        /// <summary>
        /// The set of users with pending commit operations. Simultaneous commits result in a race condition. 
        /// </summary>
        static Set<string> commitPending = Set<string>.EmptySet;

        // State queries

        /// <summary>
        /// Is this user waiting for a Commit action to respond?
        /// </summary>
        static bool CommitPending(string user) { return commitPending.Contains(user); }

        /// <summary>
        /// Has this user performed at least one Checkout action?
        /// </summary>
        static bool IsUser(string user) { return version.ContainsKey(user); }

        /// <summary>
        /// Are the actions Checkout, Commit, Resolve, Edit and Revert
        /// </summary>
        static bool CanStep(string user) { return IsUser(user) && !CommitPending(user); }

        /// <summary>
        /// Which users have performed at least one Checkout action?
        /// </summary>
        static Set<string> Users() { return version.Keys; }



        static bool CheckoutEnabled(string user)
        {
            return !CommitPending(user);
        }

        [Action]
        static void Checkout(string user)
        {
            int newVersion = Repository.currentRevision;

            if (IsUser(user))
            {
                IdentifyConflicts(user, version[user]);
                version = version.Override(user, newVersion);
            }
            else
            {
                version = version.Add(user, newVersion);
                revisions = revisions.Add(user, Map<string, Op>.EmptyMap);
                conflicts = conflicts.Add(user, Set<string>.EmptySet);
            }
        }

        static void IdentifyConflicts(string user, int currentVersion)
        {
            Set<string> userConflicts = conflicts[user];
            foreach (Pair<string, Op> revision in revisions[user])
            {
                string file = revision.First;
                Op op = revision.Second;

                if (!userConflicts.Contains(file) &&
                    currentVersion < Repository.FileVersion(file) &&
                    Repository.FileExists(file, currentVersion) &&
                    op != Op.Delete)
                    userConflicts = userConflicts.Add(file);
            }
            conflicts = conflicts.Override(user, userConflicts);
        }


        static bool ResolveEnabled(string user, string file)
        {
            return (CanStep(user) && conflicts[user].Contains(file));
        }

        [Action]
        static void Resolve([Domain("Users")] string user, string file)
        {
            Set<string> remainingFiles = conflicts[user].Remove(file);
            conflicts = conflicts.Override(user, remainingFiles);
        }

        static bool EditEnabled(string user, string file, Op op)
        {
            return (CanStep(user) &&
                    !revisions[user].ContainsKey(file) &&
                    (Repository.FileExists(file, version[user]) ?
                                                 op != Op.Add : op == Op.Add));
        }        
     
        
        [Action]
        static void Edit([Domain("Users")] string user, string file, Op op)
        {
            Map<string, Op> userRevisions = revisions[user];
            revisions = revisions.Override(user, userRevisions.Add(file, (op == Op.Delete ? Op.Delete : Op.Add)));
        }


        static bool RevertEnabled(string user, string file)
        {
            return CanStep(user) && revisions[user].ContainsKey(file);
        }

        [Action]

        static void Revert([Domain("Users")] string user, string file)
        {
            revisions = revisions.Override(user, revisions[user].RemoveKey(file));
            conflicts = conflicts.Override(user, conflicts[user].Remove(file));
        }

        static bool CommitEnabled()
        {
            return commitPending.IsEmpty;
        }


        static bool CommitEnabled(string user)
        {
            return CanStep(user);
        }

        [Action]
        static void Commit([Domain("Users")] string user)
        {
            commitPending = commitPending.Add(user);

        }



        static Set<Set<string>> ResolveSets()
        {
            Set<Set<string>> result = Set<Set<string>>.EmptySet;
            foreach (string user in Users())
                if (commitPending.Contains(user))
                {
                    Set<string> fileConflicts = FileConflicts(user);
                    if (!fileConflicts.IsEmpty)
                        result = result.Add(fileConflicts);
                }
            return result;
        }

        static Set<string> FileConflicts(string user)
        {
            Set<string> result = conflicts[user];

            foreach (Pair<string, Op> revision in revisions[user])
            {
                string file = revision.First;
                Op op = revision.Second;

                if (version[user] < Repository.FileVersion(file) &&
                    Repository.FileExists(file) &&
                    op != Op.Delete)
                    result = result.Add(file);
            }
            return result;
        }

        static bool MustResolveEnabled(string user, Set<string> files)
        {
            return IsUser(user) && 
                   CommitPending(user) && 
                   !files.IsEmpty && Object.Equals(files, FileConflicts(user));
        } 
        
        [Action]
        static void MustResolve([Domain("Users")] string user, 
                                [Domain("ResolveSets")] Set<string> files)
        {
            commitPending = commitPending.Remove(user);
            IdentifyConflicts(user, version[user]);
            version = version.Override(user, Repository.currentRevision);
        }


        static Set<int> NextVersion() 
        { 
            return new Set<int>(Repository.currentRevision, Repository.currentRevision + 1); 
        }

        static bool CommitCompleteEnabled(string user, int newVersion)
        {
            return IsUser(user) && CommitPending(user) && FileConflicts(user).IsEmpty
                && newVersion == (revisions[user].IsEmpty ? 
                       Repository.currentRevision : (Repository.currentRevision + 1));
        }       
        
        [Action]
        static void CommitComplete([Domain("Users")] string user, 
                                   [Domain("NextVersion")] int newVersion)
        {
            Map<string, Op> userRevisions = revisions[user];

            version = version.Override(user, newVersion);
            revisions = revisions.Override(user, Map<string, Op>.EmptyMap);
            commitPending = commitPending.Remove(user);
            Repository.Commit(user, "Check in", userRevisions);


        }










        //add
        //blame (praise, annotate, ann)
        //cat
        //checkout (co)
        //cleanup
        //commit (ci)
        //copy (cp)
        //delete (del, remove, rm)
        //diff (di)
        //export
        //help (?, h)
        //import
        //info
        //list (ls)
        //lock
        //log
        //merge
        //mkdir
        //move (mv, rename, ren)
        //propdel (pdel, pd)
        //propedit (pedit, pe)
        //propget (pget, pg)
        //proplist (plist, pl)
        //propset (pset, ps)
        //resolved
        //revert
        //status (stat, st)
        //switch (sw)
        //unlock
        //update (up)

    }
}
