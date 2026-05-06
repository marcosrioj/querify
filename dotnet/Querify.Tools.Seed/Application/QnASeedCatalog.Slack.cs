namespace Querify.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildSlackSpaces()
    {
        var resetPassword = Source(
            "Slack Help",
            "Reset your password",
            "https://slack.com/help/articles/201909068-Reset-your-password");
        var changeEmail = Source(
            "Slack Help",
            "Change your email address",
            "https://slack.com/help/articles/207262907-Change-your-email-address");
        var reactivateAccount = Source(
            "Slack Help",
            "Reactivate your Slack account",
            "https://slack.com/help/articles/360055665434-Reactivate-your-Slack-account");
        var guestRoles = Source(
            "Slack Help",
            "Understand guest roles in Slack",
            "https://slack.com/help/articles/202518103-Understand-guest-roles-in-Slack");
        var addPeopleToChannel = Source(
            "Slack Help",
            "Add people to a channel",
            "https://slack.com/help/articles/201980108-Add-people-to-a-channel");
        var configureNotifications = Source(
            "Slack Help",
            "Configure your Slack notifications",
            "https://slack.com/help/articles/201355156-Configure-your-Slack-notifications-Configure-your-Slack-notifications");
        var mentions = Source(
            "Slack Help",
            "Use mentions in Slack",
            "https://slack.com/help/articles/205240127-Use-mentions-in-Slack");
        var huddles = Source(
            "Slack Help",
            "Use huddles in Slack",
            "https://slack.com/help/articles/4402059015315-Use-huddles-in-Slack");
        var canvases = Source(
            "Slack Help",
            "Use canvases in Slack",
            "https://slack.com/help/articles/5342299325843-Use-canvases-in-Slack");
        var channelSettings = Source(
            "Slack Help",
            "Manage message settings for channels",
            "https://slack.com/help/articles/360022304654-Manage-message-settings-for-channels");

        return
        [
            Space(
                "Slack workspace access and channels",
                ["slack", "workspace", "channels", "access"],
                [
                    Item(
                        "How do I reset my Slack password?",
                        "Use the password reset flow from the sign-in page.",
                        "Slack lets users reset their password from the sign-in page by requesting a reset email for the workspace account.",
                        resetPassword,
                        93,
                        96),
                    Item(
                        "What if I no longer have access to the email used for Slack?",
                        "You usually need a workspace admin to help update it.",
                        "Slack directs users to change the email address from account settings when they still have access, and otherwise a workspace admin may need to help restore access.",
                        changeEmail,
                        89,
                        94),
                    Item(
                        "Can I reactivate my own Slack account after I deactivate it?",
                        "No, an owner or admin has to reactivate it.",
                        "Slack says once an account is deactivated, a workspace owner or admin needs to reactivate it before the user can sign in again.",
                        reactivateAccount,
                        92,
                        95),
                    Item(
                        "What is the practical difference between a Slack guest and Slack Connect?",
                        "Guests are paid accounts in your workspace; Slack Connect links separate workspaces.",
                        "Slack explains that guest accounts are limited users inside your workspace, while Slack Connect is for working with someone from another company in their own workspace.",
                        guestRoles,
                        93,
                        96),
                    Item(
                        "Can I add a lot of people to a Slack channel at once?",
                        "Yes, Slack supports bulk adds.",
                        "Slack says workspace owners, admins, and members with permission can add up to 1,000 people to a channel in one action.",
                        addPeopleToChannel,
                        91,
                        95),
                    Item(
                        "Can invited people be mentioned in Slack before they join a channel?",
                        "Yes, invited members can still be mentioned and messaged.",
                        "Slack notes that people who have been invited to a channel can still be mentioned and sent direct messages even before they accept the invitation.",
                        addPeopleToChannel,
                        88,
                        94),
                    Item(
                        "Are Slack guest accounts available on the free plan?",
                        "No, guest roles require a paid plan.",
                        "Slack says both single-channel and multi-channel guest roles are only available on paid subscriptions.",
                        guestRoles,
                        92,
                        95),
                    Item(
                        "How are single-channel guests counted for Slack billing?",
                        "Slack lets you add several before they equal a paid seat.",
                        "Slack says up to five single-channel guests can be added for the cost of one paid member seat, while multi-channel guests are billed as full members.",
                        guestRoles,
                        90,
                        95),
                    Item(
                        "Can Slack guests be automatically deactivated?",
                        "Yes, guest accounts can have expiration dates.",
                        "Slack lets owners and admins set an automatic deactivation date for guest accounts so access ends without manual cleanup.",
                        guestRoles,
                        89,
                        94),
                    Item(
                        "Can I leave a Slack workspace without deleting my messages?",
                        "Yes, deactivation removes access but leaves message history unless owners delete it.",
                        "Slack says deactivating your account removes your access immediately, but messages and files you posted are not automatically deleted.",
                        reactivateAccount,
                        87,
                        93)
                ]),
            Space(
                "Slack collaboration and notifications",
                ["slack", "notifications", "mentions", "huddles"],
                [
                    Item(
                        "What notification levels can I choose in Slack?",
                        "You can choose mentions only, all messages, or nothing.",
                        "Slack lets users choose whether notifications fire for only direct messages, mentions, and keywords, for all new messages in joined conversations, or for nothing.",
                        configureNotifications,
                        94,
                        96),
                    Item(
                        "Can Slack mobile notifications use different rules than desktop?",
                        "Yes, mobile settings can be different.",
                        "Slack provides a separate option to use different notification triggers for mobile devices than for desktop.",
                        configureNotifications,
                        91,
                        95),
                    Item(
                        "Does Slack support email notifications too?",
                        "Yes, email notifications are part of the notification settings.",
                        "Slack includes desktop, mobile, and email notification preferences so users can tune how they are alerted away from the app.",
                        configureNotifications,
                        88,
                        94),
                    Item(
                        "What is the difference between @channel, @here, and @everyone in Slack?",
                        "@channel alerts everyone in the channel, @here alerts active members, and @everyone targets the general channel.",
                        "Slack distinguishes between broad mention types so teams can notify the full channel, only currently active members, or everyone in the #general channel.",
                        mentions,
                        93,
                        96),
                    Item(
                        "Does Slack warn before I use a mass mention in a big channel?",
                        "Yes, Slack prompts for confirmation in larger channels.",
                        "Slack says a confirmation prompt appears when you use a mass mention in channels with six or more members.",
                        mentions,
                        90,
                        95),
                    Item(
                        "Can I mention someone who is not in a private Slack channel?",
                        "No, private channels limit who can be mentioned.",
                        "Slack notes that only members of a private channel can be mentioned there, while public channels can reference people who are not members.",
                        mentions,
                        90,
                        95),
                    Item(
                        "How many people can join a Slack huddle?",
                        "Free workspaces allow 2 people; paid plans allow up to 50.",
                        "Slack documents that huddles are limited to two participants on free plans and can scale to 50 participants on paid plans.",
                        huddles,
                        92,
                        95),
                    Item(
                        "What is a Slack canvas used for?",
                        "It is a shared space for notes, plans, and reference information.",
                        "Slack describes canvases as persistent documents attached to channels, direct messages, or workflows so teams can keep context close to the conversation.",
                        canvases,
                        89,
                        94),
                    Item(
                        "Did Slack change how canvases appear in channels and DMs?",
                        "Yes, Slack rolled channel and DM canvases into tabs.",
                        "Slack explains that channel and direct message canvases are now shown as tabs, replacing the older standalone entry point in many workspaces.",
                        canvases,
                        86,
                        92),
                    Item(
                        "Can admins hide Slack join and leave messages in channels?",
                        "Yes, message settings can control those notices.",
                        "Slack provides channel message settings so owners and admins can manage whether join and leave notices are shown.",
                        channelSettings,
                        87,
                        93)
                ])
        ];
    }
}
