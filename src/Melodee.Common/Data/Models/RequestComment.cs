using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ApiKey), IsUnique = true)]
public class RequestComment
{
    public int Id { get; set; }

    [Required]
    public Guid ApiKey { get; set; } = Guid.NewGuid();

    [RequiredGreaterThanZero]
    public int RequestId { get; set; }

    public Request Request { get; set; } = null!;

    public int? ParentCommentId { get; set; }

    public RequestComment? ParentComment { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public required string Body { get; set; }

    public bool IsSystem { get; set; }

    public Instant CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    public ICollection<RequestComment> Replies { get; set; } = new List<RequestComment>();
}
