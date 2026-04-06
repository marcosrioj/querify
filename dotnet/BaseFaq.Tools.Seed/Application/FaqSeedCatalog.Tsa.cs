namespace BaseFaq.Tools.Seed.Application;

internal static partial class FaqSeedCatalog
{
    private static IReadOnlyList<SeedFaqDefinition> BuildTsaFaqs()
    {
        var realId = Source(
            "TSA",
            "REAL ID",
            "https://www.tsa.gov/real-id");
        var identification = Source(
            "TSA",
            "Identification",
            "https://www.tsa.gov/travel/security-screening/identification");
        var precheck = Source(
            "TSA",
            "TSA PreCheck",
            "https://www.tsa.gov/precheck");
        var breastMilk = Source(
            "TSA",
            "Statement regarding screening breast milk, formula and other medically necessary liquids at TSA checkpoints",
            "https://www.tsa.gov/news/press/statements/2022/05/13/statement-regarding-screening-breast-milk-formula-and-other");
        var laptops = Source(
            "TSA",
            "Laptops",
            "https://www.tsa.gov/travel/security-screening/whatcanibring/items/laptops");
        var liquidMedication = Source(
            "TSA",
            "Liquid Medications",
            "https://www.tsa.gov/travel/security-screening/whatcanibring/items/medications-liquid");
        var pills = Source(
            "TSA",
            "Medications in pill or solid form",
            "https://www.tsa.gov/travel/security-screening/whatcanibring/items/medications-pills");
        var lithiumBatteries = Source(
            "TSA",
            "Lithium batteries",
            "https://www.tsa.gov/travel/security-screening/whatcanibring/items/lithium-batteries");
        var powerBanks = Source(
            "TSA",
            "Power banks",
            "https://www.tsa.gov/travel/security-screening/whatcanibring/items/power-banks");
        var passengerSupport = Source(
            "TSA",
            "Passenger Support",
            "https://www.tsa.gov/travel/passenger-support");

        return
        [
            Faq(
                "TSA checkpoint and travel ID rules",
                ["tsa", "travel", "checkpoint", "id"],
                [
                    Item(
                        "Do I need a REAL ID to fly in the United States now?",
                        "For standard adult screening, yes from May 7, 2025 unless you use another accepted ID.",
                        "TSA says that starting May 7, 2025, passengers 18 or older need a REAL ID-compliant license or another acceptable form of identification for domestic air travel.",
                        realId,
                        95,
                        97),
                    Item(
                        "Can I still fly if I forgot my driver's license at home?",
                        "Possibly, but TSA will use an identity verification process.",
                        "TSA says travelers without acceptable ID may still be allowed to fly after completing an identity verification process and additional screening.",
                        identification,
                        93,
                        96),
                    Item(
                        "Do I have to remove my laptop at airport security?",
                        "Usually yes, unless you are in a lane with an exception such as TSA PreCheck or advanced scanners.",
                        "TSA says standard screening often requires laptops to be removed from the bag and placed in a separate bin, unless the checkpoint process says otherwise.",
                        laptops,
                        92,
                        95),
                    Item(
                        "What does TSA PreCheck let me keep on at security?",
                        "Most members can keep on shoes, belts, and light jackets and keep laptops and compliant liquids packed.",
                        "TSA says PreCheck travelers generally do not need to remove shoes, belts, light jackets, laptops, or compliant liquids at eligible checkpoints.",
                        precheck,
                        94,
                        96),
                    Item(
                        "Can I bring breast milk or baby formula over 3.4 ounces through TSA?",
                        "Yes, in reasonable quantities.",
                        "TSA says breast milk, formula, toddler drinks, and other medically necessary liquids are allowed in carry-on bags in reasonable quantities and should be declared at the checkpoint.",
                        breastMilk,
                        95,
                        97),
                    Item(
                        "Are medically necessary liquids over 3.4 ounces allowed in carry-on bags?",
                        "Yes, when declared for screening.",
                        "TSA allows medically necessary liquids larger than the standard liquid limit, but travelers should tell the officer and expect additional screening.",
                        liquidMedication,
                        93,
                        96),
                    Item(
                        "Can I pack pills in my carry-on bag?",
                        "Yes, solid medications are allowed.",
                        "TSA says medications in pill or other solid form are allowed in unlimited amounts in carry-on and checked bags, though screening may still occur.",
                        pills,
                        94,
                        96),
                    Item(
                        "Does TSA make the final call on what gets through the checkpoint?",
                        "Yes, the officer has final discretion at screening.",
                        "TSA states on What Can I Bring guidance that the final decision rests with the TSA officer on whether an item is allowed through the checkpoint.",
                        laptops,
                        91,
                        95),
                    Item(
                        "Can I bring contact lens solution or liquid medicine through security?",
                        "Yes, if it is medically necessary and declared.",
                        "TSA treats liquid medication and necessary medical liquids differently from ordinary 3-1-1 toiletries when the traveler declares them for screening.",
                        liquidMedication,
                        90,
                        95),
                    Item(
                        "Should I expect extra screening for oversized medical liquids or formula?",
                        "Yes, TSA says they are screened separately.",
                        "TSA says oversized medical liquids, formula, and cooling accessories such as gel packs may receive separate screening, including inspection for explosives.",
                        breastMilk,
                        92,
                        96)
                ]),
            Faq(
                "TSA medical devices and batteries",
                ["tsa", "medical", "batteries", "accessibility"],
                [
                    Item(
                        "Can I put a power bank in checked luggage?",
                        "No, power banks must go in carry-on baggage.",
                        "TSA says spare lithium batteries, including power banks and charging cases, must be carried in carry-on baggage only.",
                        powerBanks,
                        95,
                        97),
                    Item(
                        "Where do spare lithium batteries have to be packed for a flight?",
                        "Carry-on only.",
                        "TSA says spare or uninstalled lithium ion and lithium metal batteries are allowed only in carry-on baggage.",
                        lithiumBatteries,
                        95,
                        97),
                    Item(
                        "Can I travel with larger spare lithium batteries over 100 watt-hours?",
                        "Sometimes, with airline approval and quantity limits.",
                        "TSA says spare lithium ion batteries from 101 to 160 watt-hours may be allowed with airline approval, usually limited to two spares in carry-on bags.",
                        lithiumBatteries,
                        93,
                        96),
                    Item(
                        "Can TSA inspect medication or medical devices visually instead of opening them fully?",
                        "Yes, you can request alternative screening.",
                        "TSA says travelers can ask for visual inspection or other accommodations for sensitive medical items and devices at the checkpoint.",
                        passengerSupport,
                        92,
                        96),
                    Item(
                        "What is TSA Cares and when should I contact it?",
                        "It is an accessibility support helpline; contact it at least 72 hours before travel.",
                        "TSA recommends contacting TSA Cares at least 72 hours before travel if you need checkpoint support related to disability, medical conditions, or special assistance.",
                        passengerSupport,
                        94,
                        96),
                    Item(
                        "Can I ask for a Passenger Support Specialist at the airport?",
                        "Yes, TSA provides that option.",
                        "TSA says a Passenger Support Specialist can help travelers who need extra assistance during the screening process.",
                        passengerSupport,
                        91,
                        95),
                    Item(
                        "Are frozen gel packs for medical items allowed through TSA?",
                        "Yes, when they are needed for medical purposes.",
                        "TSA says frozen, partially frozen, or slushy gel packs needed to cool medically necessary items can be allowed, but they remain subject to screening.",
                        breastMilk,
                        92,
                        95),
                    Item(
                        "Should prescription bottles be labeled for TSA?",
                        "It is not required, but TSA recommends labeling medication.",
                        "TSA says labeling medication can help speed the screening process even though federal security rules do not require a prescription bottle for every item.",
                        liquidMedication,
                        89,
                        94),
                    Item(
                        "Can battery-powered medical devices go in checked baggage?",
                        "Some can, but installed batteries must be protected and airline rules still apply.",
                        "TSA allows many battery-powered devices in checked baggage if the installed batteries are protected against accidental activation, though airline safety rules can add limits.",
                        lithiumBatteries,
                        88,
                        94),
                    Item(
                        "Can I travel with an oxygen concentrator or other medical device?",
                        "Often yes, but airline approval may still be required.",
                        "TSA screening rules may allow the device, but travelers should still confirm airline rules for use, approval, and in-flight power requirements before they fly.",
                        passengerSupport,
                        90,
                        95)
                ])
        ];
    }
}
