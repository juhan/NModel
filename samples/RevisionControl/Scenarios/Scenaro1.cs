using System;
using NModel;
using NModel.Attributes;

namespace RevisionControl
{
    [Feature]
    static class Scenario1
    {
        static readonly Set<string> users = new Set<string>("alice", "bob");
        static readonly Set<string> files = new Set<string>("file1");
        static readonly Set<int> revisions = new Set<int>(0, 1, 2, 3, 4);

        static Set<string> Users() { return users; }
        static Set<string> Files() { return files; }
        static Set<int> Revisions() { return revisions; }

        [Action("Checkout")]
        static void Checkout([Domain("Users")] string users) {}

        [Action("Resolve")]
        [Action("Revert")]
        static void ClientAction([Domain("Users")] string users, [Domain("Files")] string files) {}

        [Action]
        static void Edit([Domain("Users")] string user, [Domain("Files")] string file, Op op) { }

        static bool EditEnabled() { return Repository.currentRevision < 5; } 

        [Action("CommitComplete")]
        static void CommitComplete([Domain("Users")] string users, [Domain("Revisions")] int newRevision) { }
    }
}
