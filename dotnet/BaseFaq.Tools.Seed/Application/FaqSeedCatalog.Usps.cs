namespace BaseFaq.Tools.Seed.Application;

internal static partial class FaqSeedCatalog
{
    private static IReadOnlyList<SeedFaqDefinition> BuildUspsFaqs()
    {
        var holdMail = Source(
            "USPS",
            "USPS Hold Mail",
            "https://www.usps.com/manage/hold-mail.htm");
        var informedDelivery = Source(
            "USPS",
            "Informed Delivery",
            "https://www.usps.com/manage/informed-delivery.htm");
        var packageIntercept = Source(
            "USPS",
            "USPS Package Intercept",
            "https://www.usps.com/manage/package-intercept.htm");
        var packagePickup = Source(
            "USPS FAQ",
            "What is Package Pickup?",
            "https://faq.usps.com/articles/Knowledge/What-is-Package-Pickup");
        var changeAddress = Source(
            "USPS",
            "Change of Address",
            "https://www.usps.com/manage/forward.htm");
        var redelivery = Source(
            "USPS",
            "Redelivery",
            "https://tools.usps.com/redelivery.htm");
        var accountableMail = Source(
            "USPS FAQ",
            "USPS Mail Requiring a Signature - Accountable Mail",
            "https://faq.usps.com/articles/FAQ/USPS-Mail-Requiring-a-Signature-Accountable-Mail");
        var poBoxes = Source(
            "USPS",
            "PO Boxes",
            "https://www.usps.com/manage/po-boxes.htm");

        return
        [
            Faq(
                "USPS mail holds and package updates",
                ["usps", "mail", "packages", "delivery"],
                [
                    Item(
                        "How long can USPS hold my mail?",
                        "A Hold Mail request can cover 3 to 30 days.",
                        "USPS says Hold Mail can start as early as the next scheduled delivery day and can cover a period from 3 to 30 days.",
                        holdMail,
                        95,
                        97),
                    Item(
                        "Does USPS Hold Mail apply to one person or the whole address?",
                        "It applies to the entire address.",
                        "USPS Hold Mail requests stop delivery for all mail addressed to that location, not just one resident.",
                        holdMail,
                        93,
                        96),
                    Item(
                        "Who can place a USPS Hold Mail request?",
                        "A resident at the address can request it.",
                        "USPS requires the requester to be a current resident at the address and to choose a return date or pickup arrangement for the held mail.",
                        holdMail,
                        91,
                        95),
                    Item(
                        "What is USPS Informed Delivery?",
                        "It is a free preview of incoming letter mail and package updates.",
                        "USPS Informed Delivery sends digital previews of arriving letter-sized mail and package status updates to eligible addresses.",
                        informedDelivery,
                        92,
                        96),
                    Item(
                        "Can USPS stop a package after it has already been mailed?",
                        "Sometimes, through Package Intercept.",
                        "USPS Package Intercept lets eligible mailers redirect or hold a package before final delivery, though fees and eligibility rules apply.",
                        packageIntercept,
                        90,
                        95),
                    Item(
                        "Is regular USPS Package Pickup free?",
                        "Yes, standard carrier pickup is free for eligible packages.",
                        "USPS says regular Package Pickup can be scheduled at no extra charge when the carrier is already making a delivery or route stop.",
                        packagePickup,
                        92,
                        96),
                    Item(
                        "Can I schedule USPS package pickup for a future date?",
                        "Yes, pickup can be scheduled in advance.",
                        "USPS says Package Pickup can be scheduled ahead of time instead of waiting until the day you want the carrier to collect packages.",
                        packagePickup,
                        88,
                        94),
                    Item(
                        "Can USPS pick up multiple outgoing packages in one request?",
                        "Yes, a single pickup request can cover multiple packages.",
                        "USPS Package Pickup is designed for batches of outbound packages as long as they meet the service rules and are ready at the scheduled location.",
                        packagePickup,
                        87,
                        93),
                    Item(
                        "When should I use Package Intercept instead of Redelivery?",
                        "Use Intercept before delivery and Redelivery after a missed delivery attempt.",
                        "USPS Package Intercept is for rerouting an item already in the mailstream, while Redelivery is for a package that had a delivery attempt and needs another delivery or pickup plan.",
                        packageIntercept,
                        89,
                        94),
                    Item(
                        "Can a PO Box help if I need a more stable mailing address?",
                        "Yes, USPS offers PO Box rentals for receiving mail securely.",
                        "USPS PO Boxes give customers a dedicated mailing address and pickup location when home delivery is inconvenient or unreliable.",
                        poBoxes,
                        86,
                        92)
                ]),
            Faq(
                "USPS forwarding and accountable mail",
                ["usps", "forwarding", "redelivery", "signature"],
                [
                    Item(
                        "How does USPS Change of Address forwarding work?",
                        "You can file temporary or permanent forwarding online or at the Post Office.",
                        "USPS lets customers submit temporary or permanent Change of Address requests so eligible mail is forwarded from the old address to the new one.",
                        changeAddress,
                        94,
                        96),
                    Item(
                        "Is there a fee to change my address with USPS online?",
                        "Yes, USPS charges a small identity verification fee online.",
                        "USPS uses a small online identity verification fee when you submit a Change of Address request through the web form.",
                        changeAddress,
                        92,
                        95),
                    Item(
                        "Can I schedule USPS Redelivery online after I miss a package?",
                        "Yes, online redelivery is available for eligible items.",
                        "USPS Redelivery lets you request another delivery date or choose pickup options for many items after a delivery attempt.",
                        redelivery,
                        93,
                        96),
                    Item(
                        "Will USPS automatically redeliver mail that needs a signature?",
                        "Not always.",
                        "USPS accountable mail often requires the recipient or authorized agent to be present to sign, so automatic redelivery is more limited than for ordinary parcels.",
                        accountableMail,
                        92,
                        95),
                    Item(
                        "What counts as accountable mail with USPS?",
                        "Items such as Certified Mail, Registered Mail, Collect on Delivery, Adult Signature, and some insured mail.",
                        "USPS defines accountable mail as items that require a signature and or fee collection to complete delivery and provide evidence of delivery or extra security.",
                        accountableMail,
                        93,
                        96),
                    Item(
                        "Can someone else sign for my USPS accountable mail?",
                        "Yes, an authorized agent can sign.",
                        "USPS says accountable mail may be signed for by the addressee or the addressee's authorized agent when delivery rules allow it.",
                        accountableMail,
                        90,
                        95),
                    Item(
                        "What should I do if I miss accountable mail delivery?",
                        "Use the notice to arrange pickup or a qualifying redelivery.",
                        "USPS instructs customers to use the redelivery notice and follow the accountable mail rules, which still require an eligible person to be present for signature.",
                        accountableMail,
                        91,
                        95),
                    Item(
                        "Can I forward all types of USPS mail after a Change of Address?",
                        "No, some mail classes and services have forwarding limits.",
                        "USPS forwarding covers many mailpieces, but not every class or endorsement follows the same forwarding rules or time limits.",
                        changeAddress,
                        88,
                        94),
                    Item(
                        "Can a USPS PO Box receive packages too?",
                        "Yes, many PO Boxes can receive packages from USPS, and some locations support street addressing for more carriers.",
                        "USPS says PO Boxes can receive many kinds of mail and packages, and some locations offer a street addressing option for broader package delivery support.",
                        poBoxes,
                        87,
                        93),
                    Item(
                        "Should I use Redelivery or pick up signature mail at the Post Office?",
                        "Pickup is often simpler if you need to guarantee someone signs in person.",
                        "Because accountable mail requires a valid signature at delivery, Post Office pickup is often the more reliable option when home availability is uncertain.",
                        accountableMail,
                        89,
                        94)
                ])
        ];
    }
}
