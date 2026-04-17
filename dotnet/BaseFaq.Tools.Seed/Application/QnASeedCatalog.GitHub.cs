namespace BaseFaq.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildGitHubSpaces()
    {
        var recoveryMethods = Source(
            "GitHub Docs",
            "Configuring two-factor authentication recovery methods",
            "https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/configuring-two-factor-authentication-recovery-methods");
        var primaryEmail = Source(
            "GitHub Docs",
            "Changing your primary email address",
            "https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-personal-account-on-github/managing-email-preferences/changing-your-primary-email-address");
        var renameRepository = Source(
            "GitHub Docs",
            "Renaming a repository",
            "https://docs.github.com/en/repositories/creating-and-managing-repositories/renaming-a-repository");
        var archiveRepository = Source(
            "GitHub Docs",
            "Archiving repositories",
            "https://docs.github.com/en/repositories/archiving-a-github-repository/archiving-repositories");
        var deleteRepository = Source(
            "GitHub Docs",
            "Deleting a repository",
            "https://docs.github.com/en/repositories/creating-and-managing-repositories/deleting-a-repository");
        var restoreRepository = Source(
            "GitHub Docs",
            "Restoring a deleted repository",
            "https://docs.github.com/en/repositories/creating-and-managing-repositories/restoring-a-deleted-repository");
        var transferRepository = Source(
            "GitHub Docs",
            "Transferring a repository",
            "https://docs.github.com/moving-a-repo");
        var repoVisibility = Source(
            "GitHub Docs",
            "Setting repository visibility",
            "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility");
        var forkBehavior = Source(
            "GitHub Docs",
            "What happens to forks when a repository is deleted or changes visibility?",
            "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/what-happens-to-forks-when-a-repository-is-deleted-or-changes-visibility");
        var deletePermissions = Source(
            "GitHub Docs",
            "Setting permissions for deleting or transferring repositories",
            "https://docs.github.com/en/organizations/managing-organization-settings/setting-permissions-for-deleting-or-transferring-repositories");
        var aboutNotifications = Source(
            "GitHub Docs",
            "About notifications",
            "https://docs.github.com/en/subscriptions-and-notifications/concepts/about-notifications");
        var configuringNotifications = Source(
            "GitHub Docs",
            "Configuring notifications",
            "https://docs.github.com/en/account-and-profile/managing-subscriptions-and-notifications-on-github/setting-up-notifications/configuring-notifications");
        var viewingSubscriptions = Source(
            "GitHub Docs",
            "Viewing your subscriptions",
            "https://docs.github.com/articles/watching-and-unwatching-repositories");

        return
        [
            Space(
                "GitHub account security and repositories",
                ["github", "repositories", "security", "ownership"],
                [
                    Item(
                        "What should I set up before I lose access to GitHub 2FA?",
                        "Add recovery methods before you need them.",
                        "GitHub recommends setting up recovery options such as a passkey, security key, recovery codes, or a verified fallback method before you get locked out of two-factor authentication.",
                        recoveryMethods,
                        94,
                        96),
                    Item(
                        "How do I change the primary email on my GitHub account?",
                        "Set another verified email as primary in account settings.",
                        "You can add and verify a new email address, then mark it as the primary email for account notifications and most GitHub account activity.",
                        primaryEmail,
                        92,
                        95),
                    Item(
                        "Can I rename a GitHub repository without breaking everything?",
                        "Most repository links redirect, but you should still update remotes.",
                        "GitHub redirects web traffic and most Git operations after a rename, but GitHub recommends updating local remotes and not reusing the old repository name later.",
                        renameRepository,
                        91,
                        95),
                    Item(
                        "What happens when I archive a repository?",
                        "The repository becomes read-only.",
                        "An archived repository stays visible on GitHub, but it becomes read-only and you can no longer push changes or manage collaborators until it is unarchived.",
                        archiveRepository,
                        90,
                        94),
                    Item(
                        "Who is allowed to delete a GitHub repository?",
                        "You need owner-level or admin-level permission.",
                        "GitHub says a personal repository owner or someone with repository admin access can delete a repository, subject to organization or enterprise policy restrictions.",
                        deleteRepository,
                        93,
                        96),
                    Item(
                        "Can a deleted repository be restored?",
                        "Sometimes, within 90 days.",
                        "GitHub says some deleted repositories can be restored within 90 days, but restoration can be blocked by fork-network rules and does not restore everything such as release attachments or team permissions.",
                        restoreRepository,
                        92,
                        95),
                    Item(
                        "How long does a repository transfer invite stay valid?",
                        "One day for a personal-account transfer invite.",
                        "When you transfer a repository from a personal account to another personal account, GitHub says the new owner must accept the confirmation email within one day or the invitation expires.",
                        transferRepository,
                        88,
                        93),
                    Item(
                        "Can I switch a repository from public to private later?",
                        "Yes, but visibility changes have side effects.",
                        "GitHub lets you change repository visibility, but you should review side effects first because forks, logs, stars, watchers, and access rules can change depending on the direction of the move.",
                        repoVisibility,
                        90,
                        95),
                    Item(
                        "What happens to forks if I delete a public GitHub repository?",
                        "Public forks stay, private forks do not.",
                        "GitHub documents that deleting a public repository does not delete its public forks, while deleting a private repository deletes its private forks.",
                        forkBehavior,
                        94,
                        96),
                    Item(
                        "Can an organization stop repository admins from deleting or transferring repos?",
                        "Yes, owners can restrict that power.",
                        "Organization owners can limit deletion and transfer rights to owners only, even if other members have admin access on a repository.",
                        deletePermissions,
                        89,
                        94)
                ]),
            Space(
                "GitHub collaboration and notifications",
                ["github", "notifications", "watching", "collaboration"],
                [
                    Item(
                        "Why am I getting GitHub notifications for a thread?",
                        "You are probably subscribed because you participated or were mentioned.",
                        "GitHub automatically subscribes people to conversations when they open or comment on an issue or pull request, are assigned, or are mentioned.",
                        aboutNotifications,
                        93,
                        96),
                    Item(
                        "What is the difference between watching and participating on GitHub?",
                        "Watching covers repository activity; participating covers a thread.",
                        "Watching a repository subscribes you to repository updates, while participating means you receive updates on a specific conversation because you interacted with it or were mentioned.",
                        configuringNotifications,
                        91,
                        95),
                    Item(
                        "Can I keep mentions but stop getting every repository update?",
                        "Yes, unwatch the repository or choose custom watch settings.",
                        "GitHub says you can unwatch a repository and still receive notifications when you are participating in a thread or when someone @mentions you.",
                        viewingSubscriptions,
                        94,
                        96),
                    Item(
                        "Can I customize GitHub notifications by event type?",
                        "Yes, use custom watch settings.",
                        "GitHub lets you watch a repository with custom settings so you only get updates for event types like issues, pull requests, releases, security alerts, or discussions.",
                        configuringNotifications,
                        92,
                        95),
                    Item(
                        "How do I reduce a noisy GitHub inbox?",
                        "Review subscriptions and watched repositories regularly.",
                        "GitHub recommends auditing your subscriptions and watched repositories so you can unsubscribe from stale threads and unwatch repositories that are no longer relevant.",
                        viewingSubscriptions,
                        91,
                        95),
                    Item(
                        "Is there a limit to how many repositories I can watch?",
                        "Yes, GitHub documents a 10,000 repository limit.",
                        "GitHub says an account can watch up to 10,000 repositories, so large organizations usually need custom watch settings and periodic cleanup.",
                        configuringNotifications,
                        88,
                        93),
                    Item(
                        "Will GitHub keep sending emails if I only use web notifications?",
                        "Notification delivery can be configured separately.",
                        "GitHub supports inbox, mobile, and email delivery, and you can choose how repository and conversation updates are delivered in notification settings.",
                        configuringNotifications,
                        86,
                        92),
                    Item(
                        "What still changes when I archive a repository used by a team?",
                        "The repo remains visible but stays read-only.",
                        "Archiving is a collaboration signal more than a deletion step: the repository remains on GitHub for reference, but active maintenance stops because writes and collaborator changes are blocked.",
                        archiveRepository,
                        89,
                        94),
                    Item(
                        "What happens to stars and watchers when repository visibility changes?",
                        "Some visibility changes permanently erase them.",
                        "GitHub documents that certain visibility changes, such as public to internal or internal to public, permanently erase stars and watchers for that repository.",
                        repoVisibility,
                        90,
                        95),
                    Item(
                        "Does GitHub automatically watch every repo I can push to?",
                        "It can, unless you change the default behavior.",
                        "GitHub says automatic watching for repositories or teams you join is enabled by default, so repository access can quietly increase inbox volume until you change the setting.",
                        aboutNotifications,
                        87,
                        93)
                ])
        ];
    }
}
