public class Review
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public int Rating { get; set; }

    public int CarId { get; set; }

    public Car Car { get; set; } = null!;
}