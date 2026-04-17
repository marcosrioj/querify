namespace BaseFaq.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildGoogleSpaces()
    {
        var twoStepLostPhone = Source(
            "Google Account Help",
            "Sign in with backup codes",
            "https://support.google.com/accounts/answer/1187538?hl=en");
        var recoveryInfo = Source(
            "Google Account Help",
            "Set up a recovery phone number or email address",
            "https://support.google.com/accounts/answer/183723?hl=en");
        var changePassword = Source(
            "Google Account Help",
            "Change or reset your password",
            "https://support.google.com/accounts/answer/41078?hl=en");
        var hackedAccount = Source(
            "Google Account Help",
            "Secure a hacked or compromised Google Account",
            "https://support.google.com/accounts/answer/6294825?hl=en");
        var suspiciousActivity = Source(
            "Google Account Help",
            "Investigate suspicious activity on your account",
            "https://support.google.com/accounts/answer/140921?hl=en");
        var secureAccount = Source(
            "Google Account Help",
            "Make your account more secure",
            "https://support.google.com/accounts/answer/46526?hl=en");
        var passkeys = Source(
            "Google Help",
            "Manage passkeys in Chrome",
            "https://support.google.com/chrome/answer/13168025?hl=en");
        var removeProducts = Source(
            "Google Account Help",
            "Remove products",
            "https://support.google.com/accounts/answer/81987?hl=en");
        var signInWithGoogle = Source(
            "Google Account Help",
            "Sign in with Google",
            "https://support.google.com/accounts/answer/12849458?hl=en");

        return
        [
            Space(
                "Google account sign-in and recovery",
                ["google", "account", "sign-in", "recovery"],
                [
                    Item(
                        "What can I use if I lose the phone I use for Google 2-Step Verification?",
                        "Backup codes can get you back in.",
                        "Google says backup codes are meant for cases where you lose your phone, change numbers, or cannot get a verification code by text, call, or Authenticator.",
                        twoStepLostPhone,
                        94,
                        96),
                    Item(
                        "How many Google backup codes do I get at a time?",
                        "Google issues a set of 10.",
                        "Google provides backup codes in sets of 10 and invalidates the old set when you create a new one.",
                        twoStepLostPhone,
                        90,
                        95),
                    Item(
                        "How do I make sure I can recover my Google Account later?",
                        "Add a recovery phone number and recovery email now.",
                        "Google recommends adding recovery information ahead of time so you can reset your password or prove ownership if you get locked out.",
                        recoveryInfo,
                        93,
                        96),
                    Item(
                        "Can old Google recovery info still be used after I change it?",
                        "Yes, Google may still offer the old option for 7 days.",
                        "Google says a previous recovery phone number or email can still be offered for verification for seven days after you change it.",
                        recoveryInfo,
                        91,
                        95),
                    Item(
                        "How do I reset my Google password if I forgot it?",
                        "Use the password recovery flow and answer the prompts carefully.",
                        "Google routes password resets through account recovery, and the more accurate your answers are, the more likely the recovery process is to succeed.",
                        changePassword,
                        89,
                        94),
                    Item(
                        "What should I do first if I think my Google Account was hacked?",
                        "Sign in, review activity, and secure the account immediately.",
                        "Google recommends reviewing recent security events, checking devices signed in to the account, changing the password, and turning on stronger account protection.",
                        hackedAccount,
                        94,
                        96),
                    Item(
                        "Where do I check which devices are signed in to my Google Account?",
                        "Open Your devices in your Google Account security settings.",
                        "Google tells users to review the Your devices section to remove any device they do not recognize after suspicious activity or a possible compromise.",
                        hackedAccount,
                        90,
                        95),
                    Item(
                        "Does Google recommend passkeys for sign-in?",
                        "Yes, Google positions passkeys as a secure password alternative.",
                        "Google describes passkeys as a phishing-resistant sign-in method that can be stored on your device or in Google Password Manager.",
                        passkeys,
                        88,
                        94),
                    Item(
                        "What if Google flags unusual activity on my account?",
                        "Review security settings and remove anything unfamiliar.",
                        "Google says unfamiliar changes to recovery options, forwarding rules, linked apps, devices, or payments should be reviewed and corrected immediately.",
                        suspiciousActivity,
                        92,
                        95),
                    Item(
                        "Where can I get a full Google security review in one place?",
                        "Use Security Checkup from your Google Account.",
                        "Google points users to Security Checkup and Recommended actions for personalized security reviews and follow-up tasks.",
                        secureAccount,
                        89,
                        94)
                ]),
            Space(
                "Google account security and connected data",
                ["google", "security", "data", "connected-apps"],
                [
                    Item(
                        "What do Google security warning colors mean?",
                        "Blue is informational, yellow is important, and red is urgent.",
                        "Google uses colored Recommended actions to show how urgent an account security task is, with a green shield meaning no immediate action is required.",
                        secureAccount,
                        90,
                        95),
                    Item(
                        "Will a Google recovery phone help if someone else is using my account?",
                        "Yes, recovery options help with lockouts and compromise recovery.",
                        "Google says recovery phone numbers and emails help when you forget a password, get locked out, or need to secure an account someone else is using.",
                        recoveryInfo,
                        92,
                        96),
                    Item(
                        "Can work or school Google accounts hide recovery options?",
                        "Yes, some managed accounts do not allow the same recovery steps.",
                        "Google notes that recovery phone and email steps may not work for work, school, or other administrator-managed accounts.",
                        recoveryInfo,
                        86,
                        92),
                    Item(
                        "What should I review after a suspicious Google sign-in alert?",
                        "Check recent security events, devices, and key settings.",
                        "Google recommends reviewing recent security events, signed-in devices, recovery options, and connected apps whenever you receive an unusual activity alert.",
                        hackedAccount,
                        91,
                        95),
                    Item(
                        "Does deleting a Google Account affect Sign in with Google on other apps?",
                        "Yes, those sign-in credentials stop working.",
                        "Google says deleting your Google Account also deletes associated Sign in with Google credentials, so you may need another login method for third-party apps.",
                        signInWithGoogle,
                        90,
                        95),
                    Item(
                        "Can I remove only one Google service instead of deleting the whole account?",
                        "Yes, some products can be removed separately.",
                        "Google lets you remove certain products or services from your account without deleting the entire Google Account.",
                        removeProducts,
                        88,
                        94),
                    Item(
                        "If I delete my Google Account, do third-party apps lose access automatically?",
                        "Sign-in credentials are removed, but third parties may still keep prior data.",
                        "Google says deleting the account removes Sign in with Google access, but third-party services may retain data they already received under their own policies.",
                        signInWithGoogle,
                        87,
                        93),
                    Item(
                        "Can I still use a passkey across devices with Google?",
                        "Yes, if it is stored in Google Password Manager or another supported manager.",
                        "Google says passkeys can be saved to a device or password manager and then used across supported devices and platforms.",
                        passkeys,
                        86,
                        92),
                    Item(
                        "What kinds of changes should make me suspect Google account misuse?",
                        "Unexpected edits to recovery info, forwarding, apps, devices, or payments.",
                        "Google lists unfamiliar recovery settings, email behavior, linked apps, devices, and payment activity as common signals that someone else may be using the account.",
                        suspiciousActivity,
                        90,
                        95),
                    Item(
                        "What is the fastest general hardening step for a Google Account?",
                        "Run Security Checkup and complete the recommended actions.",
                        "Google explicitly recommends Security Checkup as the central place to review recovery information, sign-in protection, and other account hardening tasks.",
                        secureAccount,
                        89,
                        94)
                ])
        ];
    }
}
