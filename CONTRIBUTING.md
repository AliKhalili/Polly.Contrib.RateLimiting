# Guidelines for contributing

Polly.Contrib hosts contributions around the Polly project by the community.  A Polly-Contrib repo is intended for you to own and manage your own contrib! - as such, the owners of each Contrib repository have rights on that repository, for example to merge PRs (no review from the Polly team expected or needed, though the team will be happy to take a look if you alert us).


For _process_, the Polly team recommend that Polly-Contrib projects use **Git-Workflow** ([1](https://guides.github.com/introduction/flow/index.html); [2](https://github.com/App-vNext/Polly/wiki/Git-Workflow)) to manage their content:

### Do

+ Keep the `main` branch containing only the latest-published release (as released to NuGet), or code imminently to be released.  
  - Sensible exceptions to this may be eg to improve documentation by extending the `ReadMe` - the main point is that the `main` branch always represents releasable code and the latest-released code.
+ Carry out development work in feature branches, usually in your own [fork](https://help.github.com/en/github/getting-started-with-github/fork-a-repo) of the repo.
+ Integrate changes into the `Polly-Contrib` `main` branch using PRs.
  - Using PRs allows other developers who may be interested to comment before code is merged (though as mentioned above, it is not expected that the Polly team must do this).
  - Using PRs allows developers coming to the project later to see what was added when, and why.

### Avoid

+ Avoid contributing code without PRs.  Committing code without PRs makes it hard for others coming to the project later to review and understand what code was added when, and why.
+ Avoid committing directly to the `main` branch.  Committing interim contributions directly to the `main` branch:
  - means users cannot review the head of the `main` branch as a reference for the published nuget packages;
  - means the `main` branch is not always in an 'immediately releasable' state - this can be important if an urgent bug fix needs to be released, for example;
  - creates unnecessarily messy merge-commits and rebasing, when there are multiple contributors.

### Consider

+ Consider using [milestones](https://help.github.com/en/github/managing-your-work-on-github/about-milestones) matching release numbers, to label PRs included in and issues closed in a release; see the [core Polly project for example](https://github.com/App-vNext/Polly/milestones?state=closed).
