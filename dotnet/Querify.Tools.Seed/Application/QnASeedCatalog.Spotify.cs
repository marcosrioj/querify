namespace Querify.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildSpotifySpaces()
    {
        var switchPlans = Source(
            "Spotify Support",
            "How to change Premium plans",
            "https://support.spotify.com/us/article/switch-premium-plans/");
        var cancelPremium = Source(
            "Spotify Support",
            "How to cancel Premium plans",
            "https://support.spotify.com/us/article/cancel-premium/");
        var basicPlans = Source(
            "Spotify Support",
            "Basic plans",
            "https://support.spotify.com/us/article/spotify-basic/");
        var explicitContent = Source(
            "Spotify Support",
            "Explicit content filter",
            "https://support.spotify.com/us/article/explicit-content/");
        var managedAccounts = Source(
            "Spotify Support",
            "Managed accounts for Premium Family",
            "https://support.spotify.com/us/article/managed-accounts-for-premium-family/");
        var familyVerification = Source(
            "Spotify Support",
            "Address and verification for Family plan",
            "https://support.spotify.com/us/article/family-address-verification/");
        var duoVerification = Source(
            "Spotify Support",
            "Address and verification for Duo plan",
            "https://support.spotify.com/us/article/address-verification-duo/");
        var duoMembers = Source(
            "Spotify Support",
            "Invite or remove Duo plan members",
            "https://support.spotify.com/us/article/invite-remove-duo-member/");
        var collaborativePlaylists = Source(
            "Spotify Support",
            "Collaborative playlists",
            "https://support.spotify.com/us/article/collaborative-playlists/");

        return
        [
            Space(
                "Spotify Premium and offline listening",
                ["spotify", "premium", "offline", "billing"],
                [
                    Item(
                        "How do I switch to another Spotify Premium plan?",
                        "Use Change plan from your Spotify account page.",
                        "Spotify handles plan changes through the account page, where eligible users can switch between supported Premium options.",
                        switchPlans,
                        92,
                        95),
                    Item(
                        "How do I cancel Spotify Premium?",
                        "Cancel it from Manage your plan.",
                        "Spotify says you can cancel from your account page and keep Premium features until the next billing date before the account returns to free.",
                        cancelPremium,
                        94,
                        96),
                    Item(
                        "If I cancel Spotify Premium, do I lose my playlists?",
                        "No, playlists and saved music stay with the account.",
                        "Spotify says canceling Premium moves the account to free service, but you keep playlists, saved music, and access to the account.",
                        cancelPremium,
                        92,
                        95),
                    Item(
                        "Why do I not see an option to change or cancel my Spotify plan?",
                        "Your subscription may be billed through a partner.",
                        "Spotify says plan changes may need to be handled through your mobile, internet, or other payment provider if that provider bills the subscription.",
                        cancelPremium,
                        91,
                        95),
                    Item(
                        "What is Spotify Basic?",
                        "A monthly plan for some previous Premium subscribers with fewer extras.",
                        "Spotify describes Basic as a monthly plan available to certain existing Premium subscribers that keeps core music benefits but leaves out some newer extras such as included audiobook time.",
                        basicPlans,
                        88,
                        94),
                    Item(
                        "Can I rejoin Spotify Basic after I cancel it?",
                        "No, Spotify says Basic cannot be resubscribed after cancellation.",
                        "Spotify states that once a Basic plan is canceled, it is not possible to resubscribe to that same Basic plan later.",
                        basicPlans,
                        91,
                        95),
                    Item(
                        "Does Spotify Basic still include offline listening?",
                        "Yes, the supported Basic plans keep offline music downloads.",
                        "Spotify says eligible Basic plans still include offline listening, ad-free playback, and playing songs in any order for music.",
                        basicPlans,
                        87,
                        93),
                    Item(
                        "Can a Spotify Premium Family manager block explicit content for members?",
                        "Yes, the manager can toggle explicit playback for plan members.",
                        "Spotify lets a Premium Family manager allow or block explicit content for other members from the Family management page.",
                        explicitContent,
                        90,
                        95),
                    Item(
                        "How do managed accounts work on Spotify Premium Family?",
                        "They are parent-managed accounts with tighter content controls.",
                        "Spotify managed accounts are designed for younger listeners and let the family manager control explicit content, videos, Canvas, and specific artists or songs.",
                        managedAccounts,
                        89,
                        94),
                    Item(
                        "Can the Family manager's existing members carry over when switching to Basic?",
                        "Yes, Spotify says existing members carry over for Basic Family.",
                        "Spotify documents that when a Premium Family manager switches to Basic Family, the current plan members automatically carry over to the new Basic plan.",
                        basicPlans,
                        86,
                        92)
                ]),
            Space(
                "Spotify playlists and family management",
                ["spotify", "family", "duo", "playlists"],
                [
                    Item(
                        "Can I make a Spotify playlist collaborative with friends?",
                        "Yes, a playlist can be turned collaborative.",
                        "Spotify says any playlist can be made collaborative so invited people can add, remove, and reorder tracks.",
                        collaborativePlaylists,
                        93,
                        96),
                    Item(
                        "Can I remove someone from a Spotify collaborative playlist?",
                        "Yes, collaborators can be removed later.",
                        "Spotify provides controls to remove collaborators after a playlist has been shared as a collaborative playlist.",
                        collaborativePlaylists,
                        90,
                        95),
                    Item(
                        "How does Spotify verify that Family plan members live together?",
                        "Members may need to enter the full home address.",
                        "Spotify asks Family members to verify they live with the plan manager by entering the full address when they join, when the address changes, or when verification is needed.",
                        familyVerification,
                        93,
                        96),
                    Item(
                        "What happens if a Spotify Family verification email is ignored?",
                        "The member can lose access to the plan.",
                        "Spotify says if verification fails or the invited member misses the seven-day window, that account loses access to the Family plan and switches to free.",
                        familyVerification,
                        92,
                        95),
                    Item(
                        "How does Spotify Duo verify members live together?",
                        "Duo also uses address verification.",
                        "Spotify asks Duo members to confirm their full address when joining and may ask again if the manager changes address or verification fails.",
                        duoVerification,
                        91,
                        95),
                    Item(
                        "What if a Spotify Duo member fails address verification?",
                        "They lose plan access and face a 12-month restriction on joining a different Family or Duo plan.",
                        "Spotify says a failed Duo verification can switch the invited account to free and prevent joining another Family or Duo plan for 12 months, except rejoining the same plan.",
                        duoVerification,
                        90,
                        95),
                    Item(
                        "Who can invite or remove members on Spotify Duo?",
                        "The Duo plan manager controls membership.",
                        "Spotify says only the plan manager can invite or remove Duo members from the Duo management page.",
                        duoMembers,
                        88,
                        94),
                    Item(
                        "Can a Duo member cancel the whole Spotify Duo plan?",
                        "No, only the manager can cancel the plan itself.",
                        "Spotify says a member can remove their own account from the plan, but canceling or changing the actual Duo subscription is a manager action.",
                        cancelPremium,
                        89,
                        94),
                    Item(
                        "How often can a Spotify account switch Duo plans?",
                        "Spotify documents a once-per-year limit.",
                        "Spotify notes on Duo membership management that you are only allowed to change Duo plans once a year.",
                        duoMembers,
                        87,
                        93),
                    Item(
                        "Does Spotify track my location for Family or Duo address checks?",
                        "No, Spotify says it checks the address, not your live location.",
                        "Spotify explicitly says it does not track your location for Family or Duo verification and only checks the address information you provide.",
                        familyVerification,
                        91,
                        95)
                ])
        ];
    }
}
