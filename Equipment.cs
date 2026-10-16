// A laptop, projector or lab item. Maps to the EquipmentItems table.
public class Equipment
{
    public int Id { get; set; }                          // primary key
    public string AssetTag { get; set; } = string.Empty; // sticker code, e.g. LAP-014
    public string Name { get; set; } = string.Empty;

    // One equipment item can have many loans over time.
    public List<Loan> Loans { get; set; } = new();
}
