using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ApiKey), IsUnique = true)]
public class Request
{
    public int Id { get; set; }

    [Required]
    public Guid ApiKey { get; set; } = Guid.NewGuid();

    [RequiredGreaterThanZero]
    public int Category { get; set; }

    [NotMapped]
    public RequestCategory CategoryValue => SafeParser.ToEnum<RequestCategory>(Category);

    [RequiredGreaterThanZero]
    public int Status { get; set; } = (int)RequestStatus.Pending;

    [NotMapped]
    public RequestStatus StatusValue => SafeParser.ToEnum<RequestStatus>(Status);

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public required string Description { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? ArtistName { get; set; }

    public Guid? TargetArtistApiKey { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? AlbumTitle { get; set; }

    public Guid? TargetAlbumApiKey { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SongTitle { get; set; }

    public Guid? TargetSongApiKey { get; set; }

    public int? ReleaseYear { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? ExternalUrl { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? Notes { get; set; }

    public Instant CreatedAt { get; set; }

    [RequiredGreaterThanZero]
    public int CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Instant UpdatedAt { get; set; }

    [RequiredGreaterThanZero]
    public int UpdatedByUserId { get; set; }

    public User UpdatedByUser { get; set; } = null!;

    public Instant LastActivityAt { get; set; }

    public int? LastActivityUserId { get; set; }

    public User? LastActivityUser { get; set; }

    [RequiredGreaterThanZero]
    public int LastActivityType { get; set; }

    [NotMapped]
    public RequestActivityType LastActivityTypeValue => SafeParser.ToEnum<RequestActivityType>(LastActivityType);

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? ArtistNameNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? AlbumTitleNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SongTitleNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? DescriptionNormalized { get; set; }

    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();

    public ICollection<RequestUserState> UserStates { get; set; } = new List<RequestUserState>();

    public ICollection<RequestParticipant> Participants { get; set; } = new List<RequestParticipant>();
}
