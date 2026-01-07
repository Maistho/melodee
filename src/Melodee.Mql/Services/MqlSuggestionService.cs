using System.Diagnostics;
using Melodee.Mql.Constants;
using Melodee.Mql.Interfaces;
using Melodee.Mql.Models;

namespace Melodee.Mql.Services;

/// <summary>
/// Service providing intelligent autocomplete suggestions for MQL queries.
/// </summary>
public sealed class MqlSuggestionService : IMqlSuggestionService
{
    private static readonly string[] BooleanValues = ["true", "false"];
    private static readonly string[] Keywords = ["AND", "OR", "NOT"];
    private static readonly string[] RelativeDates = ["today", "yesterday", "last-week", "last-month", "last-year"];

    private static readonly Dictionary<string, string[]> KnownGenres = new(StringComparer.OrdinalIgnoreCase)
    {
        ["songs"] = ["Rock", "Pop", "Jazz", "Blues", "Classical", "Electronic", "Hip-Hop", "Country", "Metal", "Folk", "Indie", "R&B", "Soul", "Reggae", "Punk", "Alternative"],
        ["albums"] = ["Rock", "Pop", "Jazz", "Blues", "Classical", "Electronic", "Hip-Hop", "Country", "Metal", "Folk", "Indie", "R&B", "Soul", "Reggae", "Punk", "Alternative"],
        ["artists"] = []
    };

    private static readonly Dictionary<string, string[]> KnownMoods = new(StringComparer.OrdinalIgnoreCase)
    {
        ["songs"] = ["Chill", "Energetic", "Melancholic", "Happy", "Sad", "Dark", "Uplifting", "Relaxed", "Intense", "Peaceful"],
        ["albums"] = ["Chill", "Energetic", "Melancholic", "Happy", "Sad", "Dark", "Uplifting", "Relaxed", "Intense", "Peaceful"],
        ["artists"] = []
    };

    private static readonly Dictionary<int, string[]> YearRanges = new()
    {
        [1950] = ["1950-1960", "1950-1970", "1950-1980", "1950-1990", "1950-2000"],
        [1960] = ["1960-1970", "1960-1980", "1960-1990", "1960-2000"],
        [1970] = ["1970-1980", "1970-1990", "1970-2000", "1970-2010"],
        [1980] = ["1980-1990", "1980-2000", "1980-2010", "1980-2020"],
        [1990] = ["1990-2000", "1990-2010", "1990-2020", "1990-2030"],
        [2000] = ["2000-2010", "2000-2020", "2000-2030"],
        [2010] = ["2010-2020", "2010-2030"],
        [2020] = ["2020-2030"]
    };

    public MqlSuggestionResponse GetSuggestions(string query, string entityType, int cursorPosition)
    {
        var stopwatch = Stopwatch.StartNew();
        var normalizedEntity = entityType.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(query))
        {
            stopwatch.Stop();
            return new MqlSuggestionResponse
            {
                Suggestions = GetAllFieldSuggestions(normalizedEntity).Take(10).ToList(),
                Query = query,
                CursorPosition = cursorPosition,
                DetectedContext = "startofquery",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var context = DetectContext(query, cursorPosition);
        var suggestions = context.Type switch
        {
            SuggestionContextType.StartOfQuery => GetAllFieldSuggestions(normalizedEntity),
            SuggestionContextType.AfterSpace => GetAfterSpaceSuggestions(query, cursorPosition, normalizedEntity),
            SuggestionContextType.InFieldName => GetFieldSuggestions(context.PartialText ?? string.Empty, normalizedEntity),
            SuggestionContextType.AfterFieldName => GetOperatorSuggestions(context.FieldName, normalizedEntity),
            SuggestionContextType.AfterColon => GetOperatorSuggestions(context.FieldName, normalizedEntity),
            SuggestionContextType.InValue => GetValueSuggestions(context.FieldName ?? string.Empty, context.PartialText ?? string.Empty, normalizedEntity),
            SuggestionContextType.AfterOperator => GetValueSuggestions(context.FieldName ?? string.Empty, context.PartialText ?? string.Empty, normalizedEntity),
            SuggestionContextType.AfterKeyword => GetKeywordSuggestions(context.PartialText ?? string.Empty),
            _ => Enumerable.Empty<MqlSuggestion>()
        };

        stopwatch.Stop();
        return new MqlSuggestionResponse
        {
            Suggestions = suggestions.Take(10).ToList(),
            Query = query,
            CursorPosition = cursorPosition,
            DetectedContext = context.Type.ToString().ToLowerInvariant(),
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public IEnumerable<MqlSuggestion> GetFieldSuggestions(string partialField, string entityType)
    {
        var normalizedEntity = entityType.ToLowerInvariant();
        var fields = MqlFieldRegistry.GetFieldInfos(normalizedEntity);

        if (string.IsNullOrWhiteSpace(partialField))
        {
            return fields.Select(f => new MqlSuggestion
            {
                Text = f.Name,
                Type = MqlSuggestionType.Field,
                Description = f.Description,
                InsertPosition = -1,
                CursorOffset = f.Name.Length,
                Confidence = 1.0,
                Example = $"{f.Name}:value"
            });
        }

        return fields
            .Where(f => f.Name.StartsWith(partialField, StringComparison.OrdinalIgnoreCase) ||
                        f.Aliases.Any(a => a.StartsWith(partialField, StringComparison.OrdinalIgnoreCase)))
            .Select(f =>
            {
                var matchText = f.Name.StartsWith(partialField, StringComparison.OrdinalIgnoreCase)
                    ? f.Name
                    : f.Aliases.First(a => a.StartsWith(partialField, StringComparison.OrdinalIgnoreCase));
                return new MqlSuggestion
                {
                    Text = f.Name,
                    Type = MqlSuggestionType.Field,
                    Description = f.Description,
                    InsertPosition = -1,
                    CursorOffset = f.Name.Length,
                    Confidence = matchText.Length == partialField.Length ? 1.0 : 0.9,
                    Example = $"{f.Name}:value"
                };
            })
            .OrderByDescending(s => s.Confidence);
    }

    public IEnumerable<MqlSuggestion> GetOperatorSuggestions(string? fieldName, string entityType)
    {
        var normalizedEntity = entityType.ToLowerInvariant();
        var suggestions = new List<MqlSuggestion>();

        if (string.IsNullOrEmpty(fieldName))
        {
            foreach (var op in MqlOperators.ComparisonOperators)
            {
                suggestions.Add(new MqlSuggestion
                {
                    Text = op,
                    Type = MqlSuggestionType.Operator,
                    Description = $"Comparison: {op}",
                    InsertPosition = -1,
                    CursorOffset = op.Length,
                    Confidence = 1.0
                });
            }
        }
        else
        {
            var field = MqlFieldRegistry.GetField(fieldName, normalizedEntity);
            if (field != null)
            {
                var operators = GetOperatorsForFieldType(field.Type);
                foreach (var op in operators)
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = op,
                        Type = MqlSuggestionType.Operator,
                        Description = GetOperatorDescription(op),
                        InsertPosition = -1,
                        CursorOffset = op.Length,
                        Confidence = 1.0
                    });
                }
            }
        }

        return suggestions;
    }

    public IEnumerable<MqlSuggestion> GetValueSuggestions(string fieldName, string partialValue, string entityType)
    {
        var normalizedEntity = entityType.ToLowerInvariant();
        var field = MqlFieldRegistry.GetField(fieldName, normalizedEntity);
        if (field == null)
        {
            return Enumerable.Empty<MqlSuggestion>();
        }

        var suggestions = new List<MqlSuggestion>();

        switch (field.Type)
        {
            case MqlFieldType.Boolean:
                foreach (var boolVal in BooleanValues)
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = boolVal,
                        Type = MqlSuggestionType.Boolean,
                        Description = $"Boolean {boolVal}",
                        InsertPosition = -1,
                        CursorOffset = boolVal.Length,
                        Confidence = boolVal.StartsWith(partialValue, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0
                    });
                }
                break;

            case MqlFieldType.Date:
                foreach (var date in RelativeDates)
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = date,
                        Type = MqlSuggestionType.Value,
                        Description = $"Relative date: {date}",
                        InsertPosition = -1,
                        CursorOffset = date.Length,
                        Confidence = date.StartsWith(partialValue, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0,
                        Example = $"added:{date}"
                    });
                }
                break;

            case MqlFieldType.Number when fieldName.Equals("year", StringComparison.OrdinalIgnoreCase):
                var currentYear = DateTime.UtcNow.Year;
                for (var year = currentYear; year >= 1950; year--)
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = year.ToString(),
                        Type = MqlSuggestionType.Value,
                        Description = $"Year {year}",
                        InsertPosition = -1,
                        CursorOffset = year.ToString().Length,
                        Confidence = year.ToString().StartsWith(partialValue) ? 1.0 : 0.0
                    });
                }
                if (int.TryParse(partialValue, out var partialYear) && YearRanges.TryGetValue(partialYear / 10 * 10, out var ranges))
                {
                    foreach (var range in ranges)
                    {
                        suggestions.Add(new MqlSuggestion
                        {
                            Text = range,
                            Type = MqlSuggestionType.Value,
                            Description = $"Year range {range}",
                            InsertPosition = -1,
                            CursorOffset = range.Length,
                            Confidence = 0.9,
                            Example = $"year:{range}"
                        });
                    }
                }
                break;

            case MqlFieldType.ArrayString when fieldName.Equals("genre", StringComparison.OrdinalIgnoreCase):
                foreach (var genre in KnownGenres.GetValueOrDefault(normalizedEntity, []))
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = $"\"{genre}\"",
                        Type = MqlSuggestionType.Value,
                        Description = $"Genre: {genre}",
                        InsertPosition = -1,
                        CursorOffset = genre.Length + 2,
                        Confidence = genre.StartsWith(partialValue.Trim('"'), StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0,
                        Example = $"genre:\"{genre}\""
                    });
                }
                break;

            case MqlFieldType.ArrayString when fieldName.Equals("mood", StringComparison.OrdinalIgnoreCase):
                foreach (var mood in KnownMoods.GetValueOrDefault(normalizedEntity, []))
                {
                    suggestions.Add(new MqlSuggestion
                    {
                        Text = $"\"{mood}\"",
                        Type = MqlSuggestionType.Value,
                        Description = $"Mood: {mood}",
                        InsertPosition = -1,
                        CursorOffset = mood.Length + 2,
                        Confidence = mood.StartsWith(partialValue.Trim('"'), StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0,
                        Example = $"mood:\"{mood}\""
                    });
                }
                break;
        }

        return suggestions
            .Where(s => string.IsNullOrEmpty(partialValue) || s.Confidence > 0)
            .OrderByDescending(s => s.Confidence);
    }

    public IEnumerable<MqlSuggestion> GetKeywordSuggestions(string partialKeyword)
    {
        var suggestions = new List<MqlSuggestion>();

        foreach (var keyword in Keywords)
        {
            suggestions.Add(new MqlSuggestion
            {
                Text = keyword,
                Type = MqlSuggestionType.Keyword,
                Description = $"Logical operator: {keyword}",
                InsertPosition = -1,
                CursorOffset = keyword.Length,
                Confidence = keyword.StartsWith(partialKeyword, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0,
                Example = $"{keyword} expression"
            });
        }

        return suggestions
            .Where(s => string.IsNullOrEmpty(partialKeyword) || s.Confidence > 0)
            .OrderByDescending(s => s.Confidence);
    }

    private static SuggestionContext DetectContext(string query, int cursorPosition)
    {
        var position = Math.Min(cursorPosition, query.Length);
        var beforeCursor = position > 0 ? query[..position] : string.Empty;
        var afterCursor = position < query.Length ? query[position..] : string.Empty;

        if (string.IsNullOrWhiteSpace(beforeCursor))
        {
            return new SuggestionContext { Type = SuggestionContextType.StartOfQuery };
        }

        var trimmedBefore = beforeCursor.TrimEnd();
        var lastChar = beforeCursor[^1];

        if (char.IsWhiteSpace(lastChar))
        {
            return new SuggestionContext { Type = SuggestionContextType.AfterSpace };
        }

        if (beforeCursor.EndsWith(":"))
        {
            var fieldName = trimmedBefore[..^1].TrimEnd();
            var spaceIndex = fieldName.LastIndexOf(' ');
            var potentialField = spaceIndex >= 0 ? fieldName[(spaceIndex + 1)..] : fieldName;
            return new SuggestionContext
            {
                Type = SuggestionContextType.AfterColon,
                FieldName = potentialField
            };
        }

        var colonIndex = beforeCursor.LastIndexOf(':');
        if (colonIndex >= 0)
        {
            var afterColon = beforeCursor[(colonIndex + 1)..];
            var potentialOperator = afterColon.TrimStart();

            if (MqlOperators.ComparisonOperators.Any(op => op.Equals(":" + potentialOperator, StringComparison.OrdinalIgnoreCase)) ||
                MqlOperators.StringOperators.Any(op => op.Equals(potentialOperator, StringComparison.OrdinalIgnoreCase)) ||
                MqlOperators.ComparisonOperators.Any(op => op.EndsWith(potentialOperator, StringComparison.OrdinalIgnoreCase)))
            {
                var fieldPart = beforeCursor[..colonIndex];
                var spaceIndex = fieldPart.LastIndexOf(' ');
                var fieldName = spaceIndex >= 0 ? fieldPart[(spaceIndex + 1)..] : fieldPart;
                return new SuggestionContext
                {
                    Type = SuggestionContextType.AfterOperator,
                    FieldName = fieldName,
                    PartialText = potentialOperator
                };
            }

            if (!string.IsNullOrWhiteSpace(potentialOperator))
            {
                var fieldPart = beforeCursor[..colonIndex];
                var spaceIndex = fieldPart.LastIndexOf(' ');
                var fieldName = spaceIndex >= 0 ? fieldPart[(spaceIndex + 1)..] : fieldPart;
                return new SuggestionContext
                {
                    Type = SuggestionContextType.InValue,
                    FieldName = fieldName,
                    PartialText = potentialOperator
                };
            }

            var fieldName2 = beforeCursor[..colonIndex].TrimEnd();
            var lastSpaceIndex = fieldName2.LastIndexOf(' ');
            var field = lastSpaceIndex >= 0 ? fieldName2[(lastSpaceIndex + 1)..] : fieldName2;
            return new SuggestionContext
            {
                Type = SuggestionContextType.AfterColon,
                FieldName = field
            };
        }

        foreach (var keyword in Keywords)
        {
            if (trimmedBefore.EndsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return new SuggestionContext
                {
                    Type = SuggestionContextType.AfterKeyword,
                    PartialText = keyword
                };
            }
        }

        var lastWordIndex = Math.Max(trimmedBefore.LastIndexOf(' '), Math.Max(trimmedBefore.LastIndexOf('('), -1));
        var lastWord = lastWordIndex >= 0 ? trimmedBefore[(lastWordIndex + 1)..] : trimmedBefore;

        if (char.IsLetterOrDigit(lastWord[0]) || lastWord[0] == '"')
        {
            var potentialField = lastWord.Trim('"');
            var isExactField = MqlFieldRegistry.FieldExists(potentialField, "songs") ||
                               MqlFieldRegistry.FieldExists(potentialField, "albums") ||
                               MqlFieldRegistry.FieldExists(potentialField, "artists");

            var isPartialField = MqlFieldRegistry.GetFieldInfos("songs").Any(f => f.Name.StartsWith(potentialField, StringComparison.OrdinalIgnoreCase)) ||
                                 MqlFieldRegistry.GetFieldInfos("albums").Any(f => f.Name.StartsWith(potentialField, StringComparison.OrdinalIgnoreCase)) ||
                                 MqlFieldRegistry.GetFieldInfos("artists").Any(f => f.Name.StartsWith(potentialField, StringComparison.OrdinalIgnoreCase));

            if (isExactField)
            {
                return new SuggestionContext
                {
                    Type = SuggestionContextType.AfterFieldName,
                    PartialText = potentialField,
                    FieldName = potentialField
                };
            }

            if (isPartialField)
            {
                return new SuggestionContext
                {
                    Type = SuggestionContextType.InFieldName,
                    PartialText = potentialField
                };
            }
        }

        return new SuggestionContext
        {
            Type = SuggestionContextType.AfterSpace,
            PartialText = lastWord
        };
    }

    private IEnumerable<MqlSuggestion> GetAllFieldSuggestions(string entityType)
    {
        var fields = MqlFieldRegistry.GetFieldInfos(entityType);
        return fields.Select(f => new MqlSuggestion
        {
            Text = f.Name,
            Type = MqlSuggestionType.Field,
            Description = f.Description,
            InsertPosition = -1,
            CursorOffset = f.Name.Length,
            Confidence = 1.0,
            Example = $"{f.Name}:value"
        });
    }

    private IEnumerable<MqlSuggestion> GetAfterSpaceSuggestions(string query, int cursorPosition, string entityType)
    {
        var suggestions = new List<MqlSuggestion>();

        suggestions.AddRange(GetAllFieldSuggestions(entityType));

        foreach (var keyword in Keywords)
        {
            suggestions.Add(new MqlSuggestion
            {
                Text = keyword,
                Type = MqlSuggestionType.Keyword,
                Description = $"Logical operator: {keyword}",
                InsertPosition = -1,
                CursorOffset = keyword.Length,
                Confidence = 0.8,
                Example = $"{keyword} expression"
            });
        }

        return suggestions.Take(10);
    }

    private static IEnumerable<string> GetOperatorsForFieldType(MqlFieldType fieldType)
    {
        return fieldType switch
        {
            MqlFieldType.Number or MqlFieldType.Date => MqlOperators.ComparisonOperators,
            MqlFieldType.String or MqlFieldType.ArrayString => MqlOperators.ComparisonOperators.Concat(MqlOperators.StringOperators),
            MqlFieldType.Boolean => new[] { ":=", ":!" },
            _ => MqlOperators.ComparisonOperators
        };
    }

    private static string GetOperatorDescription(string op)
    {
        return op switch
        {
            ":=" => "Equals",
            ":!=" or ":!" => "Not equals",
            ":<" => "Less than",
            ":<=" => "Less than or equal",
            ":>" => "Greater than",
            ":>=" => "Greater than or equal",
            "contains" => "Contains substring",
            "startsWith" => "Starts with",
            "endsWith" => "Ends with",
            "wildcard" => "SQL LIKE wildcard",
            "matches" => "Regex match",
            _ => op
        };
    }

    private sealed record SuggestionContext
    {
        public SuggestionContextType Type { get; init; }
        public string? FieldName { get; init; }
        public string? PartialText { get; init; }
    }

    private enum SuggestionContextType
    {
        StartOfQuery,
        AfterSpace,
        InFieldName,
        AfterFieldName,
        AfterColon,
        AfterOperator,
        InValue,
        AfterKeyword
    }
}
