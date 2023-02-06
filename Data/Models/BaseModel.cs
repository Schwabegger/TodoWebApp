namespace Data.Models;

public abstract record BaseModel
{
    public int Id { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime AddedDate { get; set; }
}