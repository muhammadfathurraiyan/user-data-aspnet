public class Request
{
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Email { get; set; }
    public required List<PersonalPayload> Payload { get; set; }
}

public class PersonalPayload
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int GenderId { get; set; }
    public required string GenderName { get; set; }
    public required string HobbyName { get; set; }
    public int Age { get; set; }
}
