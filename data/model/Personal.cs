public class Personal
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int GenderId { get; set; }
    public int HobbyId { get; set; }
    public int Age { get; set; }

    public Gender? Gender { get; set; }
    public Hobby? Hobby { get; set; }
}
