﻿using System.Text;

namespace Markdown;
public class Md
{
    private readonly Tokenizer tokenizer;
    private readonly string text;

    public Md(string text, WrapperSettingsProvider settings)
    {
        this.text = text;
        tokenizer = new Tokenizer(settings);
    }

    public Md(string text)
    {
        this.text = text;
        var settings = new WrapperSettingsProvider();
        settings.TryAddSetting(new("#", "<h1>", "#$(text)", "<h1>$(text)</h1>", true));
        settings.TryAddSetting(new("_", "<em>", "_$(text)_", "<em>$(text)</em>", nestingLevel: 2));
        settings.TryAddSetting(new("__", "<strong>", "__$(text)__", "<strong>$(text)</strong>", nestingLevel: 1));

        tokenizer = new Tokenizer(settings);
    }

    public string Render()
    {
        var builder = new StringBuilder();
        foreach (var token in tokenizer.ParseLines(text))
        {
            builder.Append(token.Render());
        }

        return builder.ToString();
    }
}
