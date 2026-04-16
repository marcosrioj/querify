namespace BaseFaq.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildAppleSpaces()
    {
        var cancelSubscription = Source(
            "Apple Support",
            "Cancel a subscription from Apple",
            "https://support.apple.com/en-us/HT202039");
        var billingHub = Source(
            "Apple Support",
            "Billing and Subscriptions",
            "https://support.apple.com/billing");
        var requestRefund = Source(
            "Apple Support",
            "Request a refund for apps or content that you bought from Apple",
            "https://support.apple.com/en-us/118223");
        var removePaymentMethod = Source(
            "Apple Support",
            "Remove a payment method from your Apple Account",
            "https://support.apple.com/en-us/118291");
        var iCloudBackup = Source(
            "Apple Support",
            "What does iCloud back up?",
            "https://support.apple.com/en-us/108770");
        var missingAfterRestore = Source(
            "Apple Support",
            "If you're missing information after you restore your iPhone or iPad with iCloud Backup",
            "https://support.apple.com/en-us/102325");
        var manageBackups = Source(
            "Apple Support",
            "View and manage iCloud device backups",
            "https://support.apple.com/guide/icloud/view-and-manage-icloud-device-backups-mm122d3ef202/icloud");

        return
        [
            Space(
                "Apple subscriptions, billing, and purchases",
                ["apple", "billing", "subscriptions", "purchases"],
                [
                    Item(
                        "How do I cancel an Apple subscription?",
                        "Cancel it from your Apple Account subscription settings.",
                        "Apple says you can cancel subscriptions directly from your device or account settings, and the subscription usually remains active until the next billing date.",
                        cancelSubscription,
                        94,
                        96),
                    Item(
                        "Why do I not see a cancel button for a subscription on Apple?",
                        "It is often already canceled or no longer billed through Apple.",
                        "Apple explains that if there is no cancel option, the subscription may already be canceled or may not be billed through Apple anymore.",
                        cancelSubscription,
                        91,
                        95),
                    Item(
                        "Can I ask Apple for a refund on an app or subscription purchase?",
                        "Yes, if the purchase is eligible.",
                        "Apple routes refund requests through reportaproblem.apple.com, where you choose Request a refund and submit the reason for the request.",
                        requestRefund,
                        92,
                        95),
                    Item(
                        "How long does an Apple refund take?",
                        "Apple says status updates usually arrive in 24 to 48 hours, then payment timing depends on the provider.",
                        "Apple says you can usually expect an update on the refund request in 24 to 48 hours, and the return of funds to your payment method may take additional time.",
                        requestRefund,
                        90,
                        95),
                    Item(
                        "Where do I see my Apple purchase history?",
                        "Use Apple billing or purchase history tools.",
                        "Apple provides purchase history through your Apple Account so you can review apps, media, and subscription transactions tied to the account.",
                        billingHub,
                        89,
                        94),
                    Item(
                        "What should I do if I do not recognize an Apple charge?",
                        "Check your purchase history first.",
                        "Apple recommends reviewing purchase history and subscription billing details before reporting an unfamiliar charge as a possible issue.",
                        billingHub,
                        91,
                        95),
                    Item(
                        "How do I update my Apple payment method?",
                        "Manage payment information from your Apple Account billing settings.",
                        "Apple lets you add, change, or remove payment methods from account billing settings when there are no blocking conditions on the account.",
                        billingHub,
                        88,
                        94),
                    Item(
                        "Why can I not remove my last Apple payment method?",
                        "Something on the account may still require it.",
                        "Apple says you may need to cancel subscriptions, turn off purchase sharing, or pay an unpaid balance before you can remove a payment method.",
                        removePaymentMethod,
                        93,
                        96),
                    Item(
                        "Can I redownload old Apple purchases on another device?",
                        "Usually yes, if they are still available.",
                        "Apple says previously purchased apps and media can often be downloaded again after you sign in, though some items may no longer be available.",
                        billingHub,
                        87,
                        93),
                    Item(
                        "Will refunded or unavailable Apple content always stay downloadable?",
                        "No, redownload availability can change.",
                        "Apple notes that some earlier purchases may no longer be available for redownload, especially if they were refunded or removed from the store.",
                        requestRefund,
                        86,
                        92)
                ]),
            Space(
                "Apple iCloud backup and storage",
                ["apple", "icloud", "backup", "restore"],
                [
                    Item(
                        "What does iCloud Backup actually include?",
                        "It backs up data on the device that is not already syncing to iCloud.",
                        "Apple says iCloud Backup is for device data and settings that are not already synced, such as some app data, Home Screen layout, and locally stored items.",
                        iCloudBackup,
                        94,
                        96),
                    Item(
                        "Are photos already in iCloud Photos included in my daily iCloud Backup?",
                        "No, synced iCloud content is not duplicated in the backup.",
                        "Apple says photos, messages, notes, and other data that already sync to iCloud are generally not included again in the daily backup.",
                        iCloudBackup,
                        92,
                        95),
                    Item(
                        "How do I reduce the size of an iCloud backup?",
                        "Turn off apps you do not need and delete old backups.",
                        "Apple recommends removing unused app backups or deleting old device backups if you need to shrink total iCloud backup storage.",
                        iCloudBackup,
                        89,
                        94),
                    Item(
                        "What happens if I turn off iCloud Backup for a device?",
                        "Apple keeps the stored backups for 180 days before deletion.",
                        "Apple says disabling iCloud Backup stops future automatic backups, but existing backups are kept for 180 days before they are removed.",
                        iCloudBackup,
                        92,
                        95),
                    Item(
                        "Can I delete an old iCloud backup manually?",
                        "Yes, from backup management settings.",
                        "Apple lets you view devices with stored backups and manually delete a backup from iPhone, iPad, Mac, or Windows settings.",
                        manageBackups,
                        88,
                        94),
                    Item(
                        "Why does an Apple restore look finished but some data is still missing?",
                        "The restore may still be downloading in the background.",
                        "Apple says a restore can continue in the background after setup, so media, messages, and other content may take longer to reappear over Wi-Fi.",
                        missingAfterRestore,
                        93,
                        96),
                    Item(
                        "Why am I asked for Apple Account passwords during restore?",
                        "Some purchased content needs account authentication before it downloads.",
                        "Apple says you may need to enter the password for each Apple Account used to purchase content before apps or media can restore completely.",
                        missingAfterRestore,
                        90,
                        95),
                    Item(
                        "Will everything restore if I use an iPad backup on an iPhone or the other way around?",
                        "No, some cross-device data does not restore.",
                        "Apple notes that restoring from a backup of a different device type can leave out certain data, so cross-device restore is not always one-to-one.",
                        missingAfterRestore,
                        89,
                        94),
                    Item(
                        "What should I check if data is still missing after an iCloud restore?",
                        "Check whether the data type was backed up and whether you have a computer backup too.",
                        "Apple recommends confirming that the missing data type is included in iCloud Backup and then checking for a separate backup on a computer.",
                        missingAfterRestore,
                        91,
                        95),
                    Item(
                        "Can previously purchased apps or media fail to redownload after restore?",
                        "Yes, availability can vary by region and store status.",
                        "Apple says purchased content usually downloads again after restore, but availability can differ by region and some past purchases may no longer be offered.",
                        missingAfterRestore,
                        87,
                        93)
                ])
        ];
    }
}
