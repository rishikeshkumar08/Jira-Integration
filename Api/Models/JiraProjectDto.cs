namespace Api.Models;

public class JiraProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class JiraBoardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class JiraSprintDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
