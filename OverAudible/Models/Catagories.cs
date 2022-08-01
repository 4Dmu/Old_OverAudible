using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public enum Categorie : long
    {
        [Description("All Categories")]
        AllCategories = 0,
        [Description("Arts & Entertainment")]
        ArtsAndEntertainment = 185719100111,
        [Description("Biographies & Memoirs")]
        BiographiesAndMemoirs = 18571951011,
        [Description("Business & Careers")]
        BusinessAndCareers = 18572029011,
        [Description("Children's Audiobooks")]
        ChildrensAudiobooks = 18572091011,
        [Description("Computers & Technology")]
        ComputersAndTechnology = 18573211011,
        [Description("Education & Learning")]
        EducationAndLearning = 18573267011,
        [Description("Erotica")]
        Erotica = 18573351011,
        [Description("Health & Wellness")]
        HealthAndWellness = 18573370011,
        [Description("History")]
        History = 18573518011,
        [Description("Home & Garden")]
        HomeAndGarden = 18573701011,
        [Description("LGBTQ+")]
        LGBTQPlus = 18573743011,
        [Description("Literature & Fiction")]
        LiteratureAndFiction = 18574426011,
        [Description("Money & Finance")]
        MoneyAndFinance = 18574547011,
        [Description("Mystery, Thriller & Suspense")]
        MysteryThrillerAndSuspense = 18574597011,
        [Description("Politics & Social Sciences")]
        PoliticsAndSocialSciences = 18574641011,
        [Description("Relationships, Parenting & Personal Development")]
        RelationshipsParentingAndPersonalDevelopment = 18574784011,
        [Description("Religion & Spirituality")]
        ReligionAndSpirituality = 18574839011,
        [Description("Romance")]
        Romance = 18580518011,
        [Description("Science & Engineering")]
        ScienceAndEngineering = 18580540011,
        [Description("Science Fiction & Fantasy")]
        ScienceFictionAndFantasy = 18580606011,
        [Description("Sports & Outdoors")]
        SportsAndOutdoors = 18580648011,
        [Description("Teen & Young Adult")]
        TeenAndYoungAdult = 18580715011,
        [Description("Travel & Tourism")]
        TravelAndTourism = 18581095011
    }
}
