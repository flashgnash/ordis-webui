public class Buff
{
    public string Name { get; set; } = "";
    public List<string> Formulae { get; set; } = new(); // e.g. ["str+=1", "con+=dex/2"]
}
