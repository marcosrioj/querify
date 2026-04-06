namespace BaseFaq.Tools.Seed.Application;

internal static class FaqSeedCatalog
{
    public static IReadOnlyList<SeedFaqDefinition> Build() =>
    [
        Faq(
            "GitHub account security and repositories",
            ["github", "account-security", "repository-settings", "developer-tools"],
            [
                Item(
                    "What recovery options should I set up before I lose GitHub 2FA access?",
                    "Save recovery codes and configure more than one authentication or recovery method.",
                    "GitHub recommends downloading your recovery codes and keeping at least one backup way to sign in. Recovery codes can get you back into the account if you lose your phone, and backups such as a TOTP app backup, passkey, or security key reduce the risk of a lockout.",
                    "GitHub Docs",
                    "GitHub Docs: Configuring two-factor authentication recovery methods",
                    "https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/configuring-two-factor-authentication-recovery-methods",
                    92,
                    97),
                Item(
                    "How do I change the primary email on my GitHub account?",
                    "Add the new address, make it primary, then verify it.",
                    "In GitHub settings, add the new email first, choose it in the primary email drop-down, save the change, and verify the new address. GitHub notes that an address already set as your backup email cannot be selected as the primary email until you change that setup.",
                    "GitHub Docs",
                    "GitHub Docs: Changing your primary email address",
                    "https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-personal-account-on-github/managing-email-preferences/changing-your-primary-email-address",
                    88,
                    96),
                Item(
                    "What happens when I rename a GitHub repository?",
                    "Most web traffic and Git remotes redirect, but action references do not.",
                    "Renaming a repository keeps its issues, stars, wiki, and followers, and GitHub redirects ordinary web and Git traffic from the old name to the new one. You should still update local remotes, and any workflow that calls an action from the old repository name must be updated because GitHub does not redirect action references.",
                    "GitHub Docs",
                    "GitHub Docs: Renaming a repository",
                    "https://docs.github.com/en/repositories/creating-and-managing-repositories/renaming-a-repository",
                    84,
                    94),
                Item(
                    "What changes when a GitHub repository is archived?",
                    "The repository stays visible, but most collaboration features become read-only until you unarchive it.",
                    "An archived repository remains searchable and can still be forked or starred, but code, issues, pull requests, releases, comments, and permissions all become read-only. GitHub recommends closing open work and updating the README or description before archiving.",
                    "GitHub Docs",
                    "GitHub Docs: Archiving repositories",
                    "https://docs.github.com/en/repositories/archiving-a-github-repository/archiving-repositories",
                    82,
                    95)
            ]),
        Faq(
            "Google account sign-in and recovery",
            ["google-account", "2-step-verification", "passkeys", "sign-in-recovery"],
            [
                Item(
                    "What should I do if the phone I use for Google 2-Step Verification is lost or stolen?",
                    "Sign out the lost phone, change your password, and use another trusted verification method.",
                    "Google recommends signing the missing phone out of your account and changing your password right away. You may still be able to sign in with another signed-in phone, a backup phone number, a backup code, a security key, a passkey, or a trusted device that was marked not to ask again.",
                    "Google Account Help",
                    "Google Account Help: Fix common issues with 2-Step Verification",
                    "https://support.google.com/accounts/answer/185834?hl=en",
                    91,
                    97),
                Item(
                    "Can I still access my Google account if I saved backup codes?",
                    "Yes. Each backup code works once and can replace the usual second step.",
                    "Backup codes are meant for situations where you cannot receive texts, calls, or authenticator prompts. Google lets you create a set of 10 codes, use each code one time, and refresh the set whenever you think the old codes are exhausted or exposed.",
                    "Google Account Help",
                    "Google Account Help: Sign in with backup codes",
                    "https://support.google.com/accounts/answer/1187538?hl=en",
                    89,
                    96),
                Item(
                    "How do Google passkeys change sign-in?",
                    "A passkey lets you sign in with your device unlock and can also satisfy 2-Step Verification.",
                    "Google passkeys use the security built into your device, such as a fingerprint, face unlock, or PIN, instead of a typed password. They do not remove your existing recovery options, and on accounts with 2-Step Verification or Advanced Protection they can satisfy the extra verification step because the device proves you are present.",
                    "Google Account Help",
                    "Google Account Help: Sign in with a passkey instead of a password",
                    "https://support.google.com/accounts/answer/13548313?hl=en",
                    86,
                    95),
                Item(
                    "Why might Google verification codes not arrive?",
                    "Text delivery can fail because of connectivity, filtering, carrier limits, or device setup.",
                    "Google notes that verification texts may fail if the phone has weak signal, the carrier or plan does not support SMS, or messages from short codes are blocked or filtered. If codes still do not arrive, you should use another verification option or start account recovery.",
                    "Google Account Help",
                    "Google Account Help: Fix common issues with 2-Step Verification",
                    "https://support.google.com/accounts/answer/185834?hl=en",
                    81,
                    94)
            ]),
        Faq(
            "Apple subscriptions, billing, and purchases",
            ["apple-services", "subscriptions", "refunds", "payment-methods"],
            [
                Item(
                    "How do I cancel an Apple subscription?",
                    "Open your Apple Account subscriptions list and choose Cancel Subscription.",
                    "Apple says you can cancel from Settings on iPhone or iPad, or from the web, by opening your subscriptions list and selecting the subscription you want to stop. If there is no cancel button or you already see an expiration notice, the subscription has already been canceled.",
                    "Apple Support",
                    "Apple Support: If you want to cancel a subscription from Apple",
                    "https://support.apple.com/en-us/ht202039",
                    90,
                    97),
                Item(
                    "How do I request a refund for an app, subscription, or other Apple purchase?",
                    "Use reportaproblem.apple.com, choose Request a refund, and wait for Apple's review.",
                    "Apple handles most App Store, iTunes Store, Apple Books, and subscription refund requests through reportaproblem.apple.com. After you submit the request, Apple says to allow 24 to 48 hours for an update and then additional time for the funds to return to the original payment method if the request is approved.",
                    "Apple Support",
                    "Apple Support: Request a refund for apps or content that you bought from Apple",
                    "https://support.apple.com/en-us/118223",
                    92,
                    97),
                Item(
                    "What can Apple Family Sharing include?",
                    "Eligible subscriptions can be shared with up to five family members while each person keeps a separate account.",
                    "After Family Sharing is set up, eligible subscriptions such as Apple TV+, Apple Arcade, Apple News+, Apple Fitness+, and iCloud+ can be shared with up to five other people. Apple notes that each family member keeps their own preferences, recommendations, libraries, and private files even when the subscription is shared.",
                    "Apple Support",
                    "Apple Support: Add a family member to your shared subscriptions",
                    "https://support.apple.com/en-us/108107",
                    85,
                    95),
                Item(
                    "How do I update the payment method on my Apple Account?",
                    "Go to Payment & Shipping, edit the current method, or add a new one and remove the old card.",
                    "Apple lets you manage payment details from Payment & Shipping in account settings. You can edit billing details for an existing card or add a new method first and then remove the outdated one if there is a problem with the previous card.",
                    "Apple Support",
                    "Apple Support: If you need to change or update your Apple Account payment method",
                    "https://support.apple.com/en-us/118293",
                    84,
                    95)
            ]),
        Faq(
            "Spotify Premium and offline listening",
            ["spotify", "premium-plans", "offline-downloads", "family-plan"],
            [
                Item(
                    "How do I switch to a different Spotify Premium plan?",
                    "Manage it from your account page if you pay Spotify directly; partner plans usually need extra steps.",
                    "If your subscription is billed directly by Spotify, you can change plans from the account page without losing playlists, saved music, or settings. If your plan comes through Google Play or a partner like a phone carrier, Spotify directs you to manage the change through that provider or cancel first and wait for the account to return to free.",
                    "Spotify Support",
                    "Spotify Support: How to change Premium plans",
                    "https://support.spotify.com/us/article/switch-premium-plans/",
                    88,
                    95),
                Item(
                    "What happens when I cancel Spotify Premium?",
                    "Premium continues until the next billing date, then the account switches to free and keeps playlists and saved music.",
                    "Spotify says cancellation does not erase your playlists or saved music. If you cancel a normal paid plan, Premium stays active until the next billing date; if you cancel during a zero-priced trial, the account can switch back to free immediately.",
                    "Spotify Support",
                    "Spotify Support: How to cancel Premium plans",
                    "https://support.spotify.com/us/article/cancel-premium/",
                    91,
                    96),
                Item(
                    "How does Spotify offline listening work?",
                    "Download content on up to five devices and go online at least once every 30 days to keep it available.",
                    "Spotify lets Premium users download music and podcasts for offline playback. Downloads can stop working if you exceed the five-device limit, stay offline for more than 30 days, reinstall the app, or use outdated app or storage settings.",
                    "Spotify Support",
                    "Spotify Support: Listen offline",
                    "https://support.spotify.com/us/article/listen-offline/",
                    90,
                    96),
                Item(
                    "Why does Spotify ask Family plan members to confirm an address?",
                    "Spotify verifies that invited members live with the plan manager.",
                    "Spotify asks members to confirm the same home address when they join, when the manager changes the address, or when Spotify cannot verify it automatically. The company says it checks the address only for plan verification, not location tracking, and failed reverification can remove the member from the plan.",
                    "Spotify Support",
                    "Spotify Support: Address and verification for Family plan",
                    "https://support.spotify.com/us/article/family-address-verification/",
                    83,
                    94)
            ]),
        Faq(
            "Slack workspace access and channels",
            ["slack", "workspace-admin", "account-access", "channels"],
            [
                Item(
                    "How do I reset my Slack password?",
                    "Request an email sign-in link, then use account settings to add or reset a password.",
                    "Slack tells users to start at the sign-in page, confirm their email address, use the sign-in code sent by email, and then add or reset a password from account settings. If the workspace uses single sign-on, the password option may not appear at all.",
                    "Slack Help",
                    "Slack Help: Reset your password",
                    "https://slack.com/hc/articles/201909068-Manage-your-password",
                    90,
                    96),
                Item(
                    "What if I lost access to the email address on my Slack account?",
                    "A workspace owner or admin must update the account email, because admins cannot reset your password for you.",
                    "Slack requires access to the account email inbox to reset or add a password. If you no longer have that inbox, a workspace owner or admin needs to update the email linked to your account, but administrators still cannot see or directly reset your password.",
                    "Slack Help",
                    "Slack Help: Reset your password",
                    "https://slack.com/hc/articles/201909068-Manage-your-password",
                    84,
                    95),
                Item(
                    "Can I reactivate my own Slack account?",
                    "No. You need a workspace owner or admin to reactivate it.",
                    "Slack's help center states that users cannot reactivate their own accounts. To regain access, you need to contact the people who manage Slack for your organization, such as IT, help desk staff, a workspace owner, or an admin.",
                    "Slack Help",
                    "Slack Help: Reactivate your Slack account",
                    "https://slack.com/help/articles/360055665434-Reactivate-your-Slack-account",
                    87,
                    95),
                Item(
                    "What's the difference between public and private channels in Slack?",
                    "Public channels are open to all members in the workspace; private channels are visible only to invited members.",
                    "Slack explains that public channels support transparent, searchable communication that workspace members can usually join on their own. Private channels are limited to people who are added by an existing member, and their messages or files only appear in search results for people inside the channel.",
                    "Slack Help",
                    "Slack Help: What is a channel?",
                    "https://slack.com/help/articles/360017938993-What-is-a-channel",
                    86,
                    95)
            ]),
        Faq(
            "TSA checkpoint and travel ID rules",
            ["airport-security", "carry-on-rules", "identification", "real-id"],
            [
                Item(
                    "What is the TSA liquids rule for carry-on bags?",
                    "Liquids, aerosols, gels, creams, and pastes in carry-on bags must be 3.4 ounces or 100 milliliters or less per container.",
                    "TSA allows one quart-size bag of liquids, aerosols, gels, creams, and pastes in carry-on baggage. Each container must be 3.4 ounces or 100 milliliters or less, and larger containers should go in checked baggage unless they fall under a medical or child-care exemption.",
                    "TSA",
                    "TSA: Liquids, Aerosols, and Gels Rule",
                    "https://www.tsa.gov/travel/security-screening/liquids-rule",
                    92,
                    97),
                Item(
                    "Are breast milk, formula, and juice exempt from the 3-1-1 liquids rule?",
                    "Yes. They can be carried in quantities above 3.4 ounces and screened separately.",
                    "TSA treats formula, breast milk, juice, and related cooling accessories as medically necessary liquids. They do not need to fit in the quart-size bag, but you should remove them from your carry-on and tell the officer at the start of screening so they can be screened separately.",
                    "TSA",
                    "TSA: Is breast milk, formula and juice exempt from the 3-1-1 liquids rule?",
                    "https://www.tsa.gov/travel/frequently-asked-questions/breast-milk-formula-and-juice-exempt-3-1-1-liquids-rule",
                    89,
                    96),
                Item(
                    "Can I still fly if I forgot my ID at the airport?",
                    "Sometimes. TSA may let you continue after identity verification and extra screening.",
                    "TSA says passengers who arrive without acceptable identification may still be allowed through the checkpoint after providing information that helps confirm their identity. If TSA cannot verify who you are, you will not be allowed to enter the screening area.",
                    "TSA",
                    "TSA: I forgot my identification; can I still proceed through security screening?",
                    "https://www.tsa.gov/travel/frequently-asked-questions/i-forgot-my-identification-can-i-still-proceed-through-security",
                    88,
                    96),
                Item(
                    "Do adults still need a REAL ID for domestic flights?",
                    "Since May 7, 2025, adults 18 and older need a REAL ID-compliant state ID or another TSA-accepted document such as a passport.",
                    "TSA's REAL ID enforcement began on May 7, 2025. Adults using a state-issued driver's license or ID for domestic flights now need it to be REAL ID compliant, unless they use another accepted document such as a passport or other ID on TSA's accepted list.",
                    "TSA",
                    "TSA: REAL ID",
                    "https://www.tsa.gov/real-id",
                    93,
                    97)
            ]),
        Faq(
            "USPS mail holds and package updates",
            ["usps", "mail-hold", "package-management", "informed-delivery"],
            [
                Item(
                    "How long can USPS Hold Mail last?",
                    "A Hold Mail request must be at least 3 days and no more than 30 days.",
                    "USPS Hold Mail can pause delivery for a minimum of 3 days and a maximum of 30 days. If you need a longer interruption, USPS directs customers to use mail forwarding instead of extending Hold Mail indefinitely.",
                    "USPS",
                    "USPS: Hold Mail service",
                    "https://www.usps.com/manage/hold-mail.htm",
                    91,
                    96),
                Item(
                    "Who can place a USPS Hold Mail request?",
                    "Anyone at an eligible address or their authorized agent can submit it.",
                    "USPS says a resident at the eligible address or an authorized agent can request Hold Mail. Online requests require a USPS.com account and identity verification, and some addresses may still need the request to be completed in person at the local post office.",
                    "USPS",
                    "USPS FAQ: USPS Hold Mail - The Basics",
                    "https://faq.usps.com/articles/Knowledge/USPS-Hold-Mail-The-Basics",
                    85,
                    95),
                Item(
                    "What does USPS Informed Delivery show?",
                    "It sends a daily preview of incoming letter mail and upcoming packages.",
                    "Informed Delivery gives eligible households grayscale previews of the address side of incoming letter-sized mail and package tracking updates in one place. USPS requires account sign-in plus identity and address verification before the service is activated.",
                    "USPS",
                    "USPS: Informed Delivery",
                    "https://www.usps.com/manage/informed-delivery.htm",
                    87,
                    95),
                Item(
                    "Can I stop or redirect a USPS package before it is delivered?",
                    "Yes, if the shipment is eligible and not already out for delivery, Package Intercept can return it or hold it for pickup for a fee.",
                    "USPS Package Intercept lets the sender or an authorized representative request a return to sender or a hold-for-pickup redirection before final delivery. It is not guaranteed, it only works for eligible domestic mail with tracking or extra services, and USPS charges the intercept fee only if the intercept succeeds.",
                    "USPS",
                    "USPS: Package Intercept",
                    "https://www.usps.com/manage/package-intercept.htm",
                    89,
                    96)
            ]),
        Faq(
            "Airbnb reservation changes and refunds",
            ["airbnb", "reservations", "cancellations", "refunds"],
            [
                Item(
                    "Can I see my Airbnb refund amount before I cancel?",
                    "Yes. Airbnb shows a refund breakdown in the cancellation flow before you confirm.",
                    "Airbnb says the expected refund amount is shown in the cancellation flow before you finalize the cancellation. The amount can change as the check-in date gets closer, so Airbnb recommends checking it again if you do not cancel right away.",
                    "Airbnb Help",
                    "Airbnb Help: Find your refund amount before or after canceling",
                    "https://www.airbnb.com/help/article/311",
                    92,
                    96),
                Item(
                    "How are Airbnb refunds paid back?",
                    "Eligible refunds usually go back to the original payment method used for the booking.",
                    "Airbnb issues eligible refunds to the original payment method whenever possible. If that account has been closed, Airbnb instructs guests to contact their bank or card issuer so the refund can be located or transferred.",
                    "Airbnb Help",
                    "Airbnb Help: How you'll get your refund",
                    "https://www.airbnb.com/help/article/987",
                    89,
                    95),
                Item(
                    "Can a canceled Airbnb home reservation be restored?",
                    "No. A canceled reservation cannot be reopened, but you can contact the host and book again.",
                    "Airbnb states that canceled home reservations cannot be restored. If both sides still want the stay, the guest can make a new booking or the host can send a special offer with the agreed details for a replacement reservation.",
                    "Airbnb Help",
                    "Airbnb Help: Rebook a canceled home reservation",
                    "https://www.airbnb.com/help/article/2988",
                    86,
                    95),
                Item(
                    "What should I know before changing an Airbnb home reservation?",
                    "The host must approve the change, and a new check-in date can change the cancellation policy and refund timeline.",
                    "Before you send a trip change request, Airbnb shows whether a new cancellation policy or refund timeline would apply. Hosts must approve the request, and a changed check-in date can alter the policy even when a checkout-only change would not.",
                    "Airbnb Help",
                    "Airbnb Help: Understanding home cancellation policies as a guest",
                    "https://www.airbnb.com/help/article/4052",
                    84,
                    94)
            ])
    ];

    private static SeedFaqDefinition Faq(
        string name,
        IReadOnlyList<string> tags,
        IReadOnlyList<SeedFaqItemDefinition> items)
    {
        return new SeedFaqDefinition(name, tags, items);
    }

    private static SeedFaqItemDefinition Item(
        string question,
        string shortAnswer,
        string answer,
        string sourceName,
        string sourceLabel,
        string sourceUrl,
        int helpfulVotePercent,
        int aiConfidenceScore)
    {
        return new SeedFaqItemDefinition(
            question,
            shortAnswer,
            answer,
            sourceName,
            sourceLabel,
            sourceUrl,
            helpfulVotePercent,
            aiConfidenceScore);
    }
}

internal sealed record SeedFaqDefinition(
    string Name,
    IReadOnlyList<string> Tags,
    IReadOnlyList<SeedFaqItemDefinition> Items);

internal sealed record SeedFaqItemDefinition(
    string Question,
    string ShortAnswer,
    string Answer,
    string SourceName,
    string SourceLabel,
    string SourceUrl,
    int HelpfulVotePercent,
    int AiConfidenceScore);
