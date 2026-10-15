using KennelTollare.Models;

namespace KennelTollare.Services;

public static class KennelContentDefaults
{
    public static KennelContent Create()
    {
        var now = DateTime.UtcNow;
        return new KennelContent
        {
            Site = new SiteContent
            {
                Name = "Skogsvikens",
                Eyebrow = "Nova Scotia duck tolling retriever · Värmland",
                HeroTitle = "Små tassar. Stora äventyr.",
                HeroIntro = "En liten, familjär kennel där nyfikna tollare får växa upp nära naturen, människorna och det som gör livet roligt.",
                AboutTitle = "En tollare ska få vara både busig och trygg.",
                AboutText = "För oss handlar uppfödning om mer än fina stamtavlor. Det handlar om relationen, vardagen och den där speciella blicken som säger: ”Vad hittar vi på nu?”",
                ContactTitle = "Nyfiken på en tollare i livet?",
                ContactText = "Berätta lite om dig själv, din vardag och vad du drömmer om att göra tillsammans med din framtida hund. Vi svarar gärna på frågor och berättar mer.",
                Email = "hej@skogsvikenstollare.se",
                HeroImage = "/images/iris-tik.png",
                DailyImage = "/images/loke-hane.png"
            },
            Female = new DogProfile
            {
                Name = "Skogsvikens Iris",
                Year = "2019",
                Tags = "SE UCH, Viltspår",
                Bio = "Iris är vår trygga arbetsmyra med en mjuk sida. Älskar apportering, skogspromenader och att ligga nära.",
                Image = "/images/iris-tik.png"
            },
            Male = new DogProfile
            {
                Name = "Rävrikets Loke",
                Year = "2021",
                Tags = "Agility",
                Bio = "Vår glada problemlösare och familjens stora clown.",
                Image = "/images/loke-hane.png"
            },
            Litters = new List<Litter>
            {
                new() { Title = "Första kullen", Year = "2024", Parents = "Iris & Loke", Description = "Vår första kull föddes i trygg hemmiljö och fick växa upp nära människor.", FemalePuppies = 3, MalePuppies = 3, CreatedAt = now.AddSeconds(-3), Images = new() { "/images/iris-kull.png", "/images/kull-lek.png", "/images/kull-ute.png", "/images/kull-sov.png", "/images/kull-skog.png", "/images/kull-mys.png", "/images/kull-sjo.png" } },
                new() { Title = "Ängsblomma", Year = "2025", Parents = "Iris & Loke", Description = "En testkull för att prova kullmenyn och bildspelet.", FemalePuppies = 3, MalePuppies = 3, CreatedAt = now.AddSeconds(-2), Images = new() { "/images/kull-lek.png", "/images/kull-skog.png" } },
                new() { Title = "Skogsgläntan", Year = "2026", Parents = "Iris & Loke", Description = "En testkull för att kontrollera sortering och information.", FemalePuppies = 4, MalePuppies = 3, CreatedAt = now.AddSeconds(-1), Images = new() { "/images/kull-ute.png", "/images/kull-sjo.png" } },
                new() { Title = "Morgondagg", Year = "2027", Parents = "Iris & Loke", Description = "En testkull som ligger överst eftersom den lades in senast.", FemalePuppies = 3, MalePuppies = 2, CreatedAt = now, Images = new() { "/images/kull-sov.png", "/images/kull-mys.png" } }
            }
        };
    }
}
