using LibGit2Sharp; //Requires external package
using StoryMaker.Properties;

public static class GithubSaver
{
    private static Repository GetRepository() => new Repository(Settings.Default.TextsFolderPath);

    static bool IsGitRepository()
    {
        return Repository.IsValid(Settings.Default.TextsFolderPath);
    }

    static bool CheckIfBranchExists(string branch)
    {
        using (var repo = GetRepository())
        {
            var branches = repo.Branches.Where(b => !b.IsRemote);
            var branchInRepo = branches.FirstOrDefault(b => b.FriendlyName == branch);

            return branchInRepo is not null;
        }
    }

    static string GetMostRecentBranch()
    {
        using (var repo = GetRepository())
        {
            var branches = repo.Branches
                .Where(b => !b.IsRemote && b.Tip != null)
                .OrderByDescending(b => b.Tip.Committer.When)
                .FirstOrDefault();

            if (branches is not null)
            {
                return branches.FriendlyName;
            }
        }
        throw new GitException("There is no avaliable branches that meet the requirements. Update one to remote or push a new commit to the existing ones");
    }

    static void CommitAndPushChanges(string branchName)
    {
        using (var repo = GetRepository())
        {
            var branch = repo.Branches[branchName];
            if (branch is null)
            {
                throw new GitException($"Branch '{branchName}' doesn't exist.");
            }

            Commands.Checkout(repo, branch);
            Commands.Stage(repo, "*");

            //TODO: BUSCAR COSAS 
            throw new NotImplementedException();
            var author = new Signature("Tu Nombre", "tuemail@example.com", DateTime.Now);
            var committer = author;

            Commit commit = repo.Commit("Automated commit", author, committer); // MENSAJE DE COMMIT PERSONALIZABLE

            var remote = repo.Network.Remotes["origin"];
            var options = GetCredentials();

            repo.Network.Push(remote, $"refs/heads/{branchName}", options);
        }
    }

    //TODO: CAMBIAR
    private static PushOptions GetCredentials()
    {
        throw new NotImplementedException();
        var options = new PushOptions();

        options.CredentialsProvider = (_url, _user, _cred) =>
        new UsernamePasswordCredentials
        {
            Username = "TuNombreDeUsuario",
            Password = "TuTokenDeAcceso"
        };

        return options;
    }

    public class GitException : Exception
    {
        public GitException(string message) : base(message) { }
    }
}