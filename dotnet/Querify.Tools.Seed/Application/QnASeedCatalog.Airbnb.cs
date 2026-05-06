namespace Querify.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    private static IReadOnlyList<SeedSpaceDefinition> BuildAirbnbSpaces()
    {
        var refundAmount = Source(
            "Airbnb Help",
            "Find your refund amount before or after canceling",
            "https://www.airbnb.com/help/article/311");
        var cancellationsHub = Source(
            "Airbnb Help",
            "Cancellations",
            "https://www.airbnb.com/help/article/3122");
        var hostCancels = Source(
            "Airbnb Help",
            "If your host cancels your home reservation",
            "https://www.airbnb.com/help/article/170");
        var rebookCanceled = Source(
            "Airbnb Help",
            "Rebook a canceled home reservation",
            "https://www.airbnb.com/help/article/2988");
        var changeService = Source(
            "Airbnb Help",
            "Change the date or time of your service or experience reservation",
            "https://www.airbnb.com/help/article/1566/");
        var monthlyStays = Source(
            "Airbnb Help",
            "Monthly stay cancellations by guest",
            "https://www.airbnb.com/help/article/1361/");
        var servicePolicy = Source(
            "Airbnb Help",
            "What guests need to know about a service or experience cancellation policy",
            "https://www.airbnb.com/city-hosts/cancellation-policy");
        var homePolicies = Source(
            "Airbnb",
            "Cancellation policies for your home",
            "https://www.airbnb.com/terms/cancellation_policies_en");

        return
        [
            Space(
                "Airbnb reservation changes and refunds",
                ["airbnb", "reservations", "refunds", "changes"],
                [
                    Item(
                        "Can I see my Airbnb refund amount before I cancel?",
                        "Yes, Airbnb shows the breakdown before you confirm cancellation.",
                        "Airbnb says guests can open the reservation details, start the cancellation flow, and review the projected refund amount before finalizing the cancellation.",
                        refundAmount,
                        95,
                        97),
                    Item(
                        "How are Airbnb refunds sent back to me?",
                        "Usually to the original payment method.",
                        "Airbnb says refunds are generally sent to the original payment method, though timing can still depend on the bank or payment provider.",
                        refundAmount,
                        93,
                        96),
                    Item(
                        "Can Airbnb restore a canceled home reservation?",
                        "No, canceled reservations cannot be restored.",
                        "Airbnb says a canceled home reservation cannot be restored, but you can message the host and create a new reservation instead.",
                        rebookCanceled,
                        94,
                        96),
                    Item(
                        "Can I change an Airbnb reservation instead of canceling it?",
                        "Often yes, but the host has to approve and the price or policy may change.",
                        "Airbnb lets guests send a change request for eligible reservations, and the host can accept or reject the updated dates, guest count, or other details.",
                        cancellationsHub,
                        91,
                        95),
                    Item(
                        "What should I do if my Airbnb host tells me to cancel because they cannot host me?",
                        "Ask the host to cancel instead.",
                        "Airbnb tells guests not to cancel on behalf of a host who cannot accommodate the stay, because host cancellation protects the guest's refund eligibility.",
                        hostCancels,
                        95,
                        97),
                    Item(
                        "What happens if my Airbnb host cancels before check-in?",
                        "You get a full refund and may get rebooking help.",
                        "Airbnb says guests receive a full refund if the host cancels before check-in, and Airbnb may help rebook a similar stay depending on availability and pricing.",
                        hostCancels,
                        95,
                        97),
                    Item(
                        "Does Airbnb help rebook me if a host cancels close to check-in?",
                        "Yes, Airbnb says it offers rebooking support for eligible host cancellations.",
                        "Airbnb states that when a host cancels, it can help the guest find a similar place to stay based on availability and comparable pricing.",
                        hostCancels,
                        92,
                        96),
                    Item(
                        "What if I booked the wrong dates on Airbnb?",
                        "Check the cancellation policy and send a change request quickly.",
                        "Airbnb says guests who booked the wrong dates should review the policy, request a reservation change if possible, or cancel promptly while the refund terms are still favorable.",
                        cancellationsHub,
                        90,
                        95),
                    Item(
                        "How do service or experience reservation changes work on Airbnb?",
                        "You may be able to rebook instantly inside the free cancellation window or send a host request later.",
                        "Airbnb says service or experience reservations can often be changed directly if you are still inside the free cancellation period, otherwise the host has to approve the change.",
                        changeService,
                        89,
                        94),
                    Item(
                        "Why is my Airbnb refund not showing up immediately?",
                        "Airbnb may issue it quickly, but the bank still controls posting time.",
                        "Airbnb says refund status depends on when the funds leave Airbnb and how long the original payment provider takes to post the credit.",
                        refundAmount,
                        91,
                        95)
                ]),
            Space(
                "Airbnb guest booking and cancellation policies",
                ["airbnb", "cancellation", "policy", "guest"],
                [
                    Item(
                        "Where can I find an Airbnb cancellation policy before booking?",
                        "It is shown on the listing page and again during checkout.",
                        "Airbnb says guests can find cancellation details on the listing page before booking and review them again during the booking flow before payment.",
                        servicePolicy,
                        94,
                        96),
                    Item(
                        "Where do I find the policy after I already booked?",
                        "Open Trips and check Reservation details.",
                        "Airbnb says the confirmed reservation page in Trips shows the applicable cancellation policy under Reservation details.",
                        servicePolicy,
                        93,
                        96),
                    Item(
                        "Do monthly Airbnb stays use a different cancellation policy?",
                        "Yes, stays of 28 nights or more use a long-term policy.",
                        "Airbnb says reservations for 28 nights or longer are treated as monthly stays and use a long-term cancellation policy instead of the standard short-stay options.",
                        monthlyStays,
                        95,
                        97),
                    Item(
                        "Can the first payment on a monthly Airbnb stay be nonrefundable?",
                        "Yes, depending on timing and the host's long-term policy.",
                        "Airbnb says the first monthly payment may be nonrefundable depending on how far in advance the booking was made and which long-term policy applies.",
                        monthlyStays,
                        92,
                        96),
                    Item(
                        "What happens if I cancel a monthly Airbnb stay after it has started?",
                        "The next month is often nonrefundable.",
                        "Airbnb says that if you cancel after a monthly stay begins, the following month of the reservation is generally nonrefundable.",
                        monthlyStays,
                        93,
                        96),
                    Item(
                        "How long is the free cancellation window for most Airbnb services and experiences?",
                        "Usually 1 day before start time, though some use 3 days.",
                        "Airbnb says most services and experiences allow free cancellation until one day before start time, while some listings use a three-day window.",
                        servicePolicy,
                        94,
                        96),
                    Item(
                        "Can I still get an Airbnb refund outside the free cancellation period?",
                        "Sometimes, under special policies or if the host agrees.",
                        "Airbnb says refunds outside the normal free cancellation window may still happen under major disruptive event rules, reservation issue policies, or a host-approved exception.",
                        servicePolicy,
                        90,
                        95),
                    Item(
                        "Can a host offer an exception after I already canceled a service or experience?",
                        "Yes, Airbnb says the host can choose to grant a refund exception in some cases.",
                        "Airbnb notes that even outside the free cancellation period, a host may still decide to offer a full refund for a service or experience reservation.",
                        changeService,
                        88,
                        94),
                    Item(
                        "Do major disruptive events override Airbnb's normal cancellation rules?",
                        "Yes, Airbnb has a separate Major Disruptive Events policy.",
                        "Airbnb says large-scale events that make the reservation impracticable or illegal can be handled under the Major Disruptive Events policy instead of the standard cancellation policy.",
                        cancellationsHub,
                        91,
                        95),
                    Item(
                        "Where do I check long-term home cancellation details for monthly stays?",
                        "Review the reservation in Trips and the long-term policy terms.",
                        "Airbnb points guests to the reservation details in Trips and to the long-term cancellation terms for monthly home stays when they need the exact refund rules.",
                        homePolicies,
                        89,
                        94)
                ])
        ];
    }
}
