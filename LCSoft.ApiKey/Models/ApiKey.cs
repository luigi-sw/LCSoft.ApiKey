using System.ComponentModel.DataAnnotations;

namespace LCSoft.ApiKey.Models;

public class ApiKey
{
    [Key]
    public int Id { get; set; }
    public string? Key { get; set; }
    public DateTime Expiration { get; set; }
    public int UserId { get; set; }
    public int ApiKeyId { get; set; }
    //public Guid Key { get; set; }
    public string? Name { get; set; }
}
