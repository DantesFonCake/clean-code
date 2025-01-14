﻿using System.Text;

namespace Markdown;

internal class Token
{
    public readonly string Value;
    public readonly int SourceStart;
    protected readonly TagSetting? Setting;
    protected readonly IReadOnlySet<string> excludedParts;

    public Token(string token, int sourceStart, TagSetting? setting, IReadOnlySet<string> excludedParts)
    {
        Value = token;
        SourceStart = sourceStart;
        Setting = setting;
        this.excludedParts = excludedParts;
    }

    public virtual StringBuilder Render()
    {
        if (Setting != null)
            return Setting.Render(Value, excludedParts);

        return new StringBuilder(Value);
    }
}