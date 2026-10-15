namespace KennelTollare.Models;

public sealed class KennelContent
{
    public SiteContent Site { get; set; } = new();
    public DogProfile Female { get; set; } = new();
    public DogProfile Male { get; set; } = new();
    public List<Litter> Litters { get; set; } = new();
}

public sealed class SiteContent
{
    public string Name { get; set; } = "Skogsvikens";
    public string Eyebrow { get; set; } = "Nova Scotia duck tolling retriever · Värmland";
    public string HeroTitle { get; set; } = "Små tassar. Stora äventyr.";
    public string HeroIntro { get; set; } = "En liten, familjär kennel där nyfikna tollare får växa upp nära naturen, människorna och det som gör livet roligt.";
    public string AboutTitle { get; set; } = "En tollare ska få vara både busig och trygg.";
    public string AboutText { get; set; } = "För oss handlar uppfödning om mer än fina stamtavlor. Det handlar om relationen, vardagen och den där speciella blicken som säger: ”Vad hittar vi på nu?”";
    public string ContactTitle { get; set; } = "Nyfiken på en tollare i livet?";
    public string ContactText { get; set; } = "Berätta lite om dig själv, din vardag och vad du drömmer om att göra tillsammans med din framtida hund. Vi svarar gärna på frågor och berättar mer.";
    public string Email { get; set; } = "hej@skogsvikenstollare.se";
    public string HeroImage { get; set; } = "/images/iris-tik.png";
    public string DailyImage { get; set; } = "/images/loke-hane.png";
}

public sealed class DogProfile
{
    public string Name { get; set; } = "";
    public string Year { get; set; } = "";
    public string Tags { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Image { get; set; } = "";
}

public sealed class Litter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Year { get; set; } = "";
    public string Parents { get; set; } = "Iris & Loke";
    public string Description { get; set; } = "";
    public int FemalePuppies { get; set; }
    public int MalePuppies { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Images { get; set; } = new();
    public int TotalPuppies => FemalePuppies + MalePuppies;
}
