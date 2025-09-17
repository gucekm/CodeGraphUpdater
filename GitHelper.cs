using LibGit2Sharp;
using System.Collections.Generic;
using System.Linq;

public static class GitHelper
{
    public static List<string> GetChangedFiles(string repoPath)
    {
        using var repo = new Repository(repoPath);
        var changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.WorkingDirectory);
        return changes.Select(c => c.Path).ToList();
    }
}
