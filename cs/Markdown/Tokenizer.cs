﻿namespace Markdown;

internal class Tokenizer
{
    private readonly WrapperSettingsProvider settings;

    public Tokenizer(WrapperSettingsProvider settings)
    {
        this.settings = settings;
    }

    public IEnumerable<Token> ParseLines(string text)
    {
        var tokenSeparators = GetTokenSeparators();
        foreach (var line in text.Split('\n'))
        {
            var lineTag = "";
            var root = new TokenBuilder(this, line, 0, null)
            .WithEnd(line.Length)
            .WithMdTag(lineTag);
            root.AddToken(ParseLine(line, tokenSeparators, root));
            yield return root.Build();
        }
    }

    public Token WrapToken(string token, int start, string? mdTag)
    {
        if (mdTag == null)
            return new(token, start, null);

        return settings.TryGetSetting(mdTag!, out var setting) ? (new(token, start, setting)) : (new(token, start, null));
    }

    internal TagSetting? GetSetting(string? mdTag)
    {
        if (mdTag != null && settings.TryGetSetting(mdTag!, out var setting))
            return setting;
        return null;
    }

    private TokenBuilder ParseLine(string line, string[] separators, TokenBuilder? root)
    {
        return ParseToken(line, 0, null, separators, root);
    }

    private TokenBuilder ParseToken(string line, int start, string? openingSeparator, string[] separators, TokenBuilder? root)
    {
        var openingSeparatorLength = openingSeparator?.Length ?? 0;
        var token = new TokenBuilder(this, line, start - openingSeparatorLength, root)
            .WithMdTag(openingSeparator);
        var lastKnownTokenEnd = start;
        for (var i = start; i < line.Length; i++)
        {
            var currentSeparator = GetCurrentSeparator(i, line, separators, openingSeparator);

            if (currentSeparator != null)
            {
                if (openingSeparator != currentSeparator)
                {
                    if (lastKnownTokenEnd != i)
                    {
                        var plainToken = CreateTokenBuilder(line, lastKnownTokenEnd, i, null, token);
                        token.AddToken(plainToken);
                    }

                    var childToken = ParseToken(line, i + currentSeparator.Length, currentSeparator, separators, token);
                    token.AddToken(childToken);
                    i = childToken.End - 1;
                    lastKnownTokenEnd = i + 1;
                }
                else
                {
                    token.WithEnd(i + currentSeparator.Length);
                    return token;
                }
            }
        }

        if (lastKnownTokenEnd != line.Length)
        {
            var plainToken = CreateTokenBuilder(line, lastKnownTokenEnd, line.Length, null, token);
            token.AddToken(plainToken);
        }

        token.WithEnd(line.Length);
        return token;
    }

    private TokenBuilder CreateTokenBuilder(string source, int start, int end, string? mdTag, TokenBuilder? root)
    {
        return new TokenBuilder(this, source, start, root)
                  .WithMdTag(mdTag)
                  .WithEnd(end);
    }

    private string? GetCurrentSeparator(int i, string line, string[] separators, string? lastSeparator = null)
    {
        IEnumerable<string> finalSeparators = separators;
        if (lastSeparator != null)
            finalSeparators = finalSeparators.Where(x => x != lastSeparator).Prepend(lastSeparator);
        foreach (var separator in finalSeparators)
        {
            if (i + separator.Length > line.Length)
                continue;
            var separatorEnd = i + separator.Length;
            if (line[i..separatorEnd] == separator)
            {
                return separator;
            }
        }

        return null;
    }

    private string[] GetTokenSeparators()
    {
        return settings
            .Select(x => x.Key)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .OrderByDescending(x => x.Length)
            .ToArray();
    }
}