using System;
using NModel;
using NModel.Attributes;


namespace RevisionControl
{
    [Feature]
    static class SynchronousSteps
    {
        static bool repositoryCanStep = false;

        static bool UserStepEnabled()
        {
            return !repositoryCanStep;
        }

        [Action("Commit")]
        [Action("Synchronize")]
        [Action("Edit")]
        [Action("Revert")]
        [Action("Resolve")]
        static void UserStep()
        {
            repositoryCanStep = true;
        }

        static bool RepositoryStepEnabled()
        {
            return repositoryCanStep;
        }

        [Action("MustResolve")]
        [Action("CommitComplete")]
        static void RepositoryStep()
        {
            repositoryCanStep = false;
        }
    }
}
