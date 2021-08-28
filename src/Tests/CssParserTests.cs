﻿using Superpower;

namespace Devlooped.Tests;

public record CssParserTests(ITestOutputHelper Console)
{
    [Fact]
    public void CanParseUniversalSelector()
    {
        var selector = (UniversalSelector)Parser.UniversalSelector.Parse("*");

        Assert.Null(selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseUniversalSelectorWithNoNamespace()
    {
        var selector = (UniversalSelector)Parser.UniversalSelector.Parse("|*");

        Assert.Null(selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseUniversalSelectorWithWidcardNamespace()
    {
        var selector = (UniversalSelector)Parser.UniversalSelector.Parse("*|*");

        Assert.Equal("*", selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseTypeSelector()
    {
        var selector = (TypeSelector)Parser.TypeSelector.Parse("h1");

        Assert.Equal("h1", selector.Name);
        Assert.Null(selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseTypeSelectorWithNoNamespace()
    {
        var selector = (TypeSelector)Parser.TypeSelector.Parse("|h1");

        Assert.Equal("h1", selector.Name);
        Assert.Null(selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseTypeSelectorWithWidcardNamespace()
    {
        var selector = (TypeSelector)Parser.TypeSelector.Parse("*|h1");

        Assert.Equal("h1", selector.Name);
        Assert.Equal("*", selector.NamespacePrefix);
    }

    [Fact]
    public void CanParseTypeAndClassSequence()
    {
        var sequence = Parser.SimpleSelectorSequence.Parse("h1.title");

        Assert.Equal(2, sequence.Length);
        Assert.IsType<TypeSelector>(sequence[0]);
        Assert.IsType<ClassSelector>(sequence[1]);

        Assert.Equal("h1", ((TypeSelector)sequence[0]).Name);
        Assert.Equal("title", ((ClassSelector)sequence[1]).Name);
    }

    [Fact]
    public void CanParseUniversalAndIdSequence()
    {
        var sequence = Parser.SimpleSelectorSequence.Parse("*#foo");

        Assert.Equal(2, sequence.Length);
        Assert.IsType<UniversalSelector>(sequence[0]);
        Assert.IsType<IdSelector>(sequence[1]);

        Assert.Equal("foo", ((IdSelector)sequence[1]).Id);
    }

    [Fact]
    public void CanParseIdSequenceAlone()
    {
        var sequence = Parser.SimpleSelectorSequence.Parse("#foo");

        Assert.Equal(2, sequence.Length);
        Assert.IsType<UniversalSelector>(sequence[0]);
        Assert.IsType<IdSelector>(sequence[1]);

        Assert.Equal("foo", ((IdSelector)sequence[1]).Id);
    }

    [InlineData("=", ValueMatching.Equals)]
    [InlineData("~=", ValueMatching.Includes)]
    [InlineData("|=", ValueMatching.Dash)]
    [InlineData("^=", ValueMatching.Prefix)]
    [InlineData("$=", ValueMatching.Suffix)]
    [InlineData("*=", ValueMatching.Substring)]
    [Theory]
    internal void CanParseMatching(string expression, ValueMatching matching)
        => Assert.Equal(matching, Parser.MatchingParser.Parse(expression));

    [InlineData("[role=ui]", "role", "ui", ValueMatching.Equals)]
    [InlineData("[role = ui]", "role", "ui", ValueMatching.Equals)]
    [InlineData("[ role= ui ]", "role", "ui", ValueMatching.Equals)]
    [InlineData("[role~=ui]", "role", "ui", ValueMatching.Includes)]
    [InlineData("[role|=ui]", "role", "ui", ValueMatching.Dash)]
    [InlineData("[role^=ui]", "role", "ui", ValueMatching.Prefix)]
    [InlineData("[role$=ui]", "role", "ui", ValueMatching.Suffix)]
    [InlineData("[role*=ui]", "role", "ui", ValueMatching.Substring)]
    [InlineData("[role]", "role", null, null)]
    [Theory]
    internal void CanParseAttributeSelector(string expression, string attributeName, string? attributeValue, ValueMatching? matching)
    {
        var selector = (AttributeSelector)Parser.AttributeSelector.Parse(expression);

        Assert.Equal(attributeName, selector.Name);
        Assert.Equal(attributeValue, selector.Value);
        Assert.Equal(matching, selector.Matching);
    }

    [Fact]
    public void CanParseSelector1()
    {
        var selector = Parser.Selector.Parse("h1.title p[align=center][hidden] > span + #foo [type=submit]");

        Assert.Equal(5, selector.Count);

        Assert.Equal(Combinator.None, selector[0].Combinator);
        var h1 = selector[0].SelectorSequence;

        Assert.IsType<TypeSelector>(h1[0]);
        Assert.IsType<ClassSelector>(h1[1]);
        Assert.Equal("h1", ((TypeSelector)h1[0]).Name);
        Assert.Equal("title", ((ClassSelector)h1[1]).Name);

        var p = selector[1];

        Assert.Equal(Combinator.Descendant, p.Combinator);
        Assert.IsType<TypeSelector>(p.SelectorSequence[0]);
    }

    [Fact]
    public void CanParseSelector2()
    {
        var selector = Parser.Selector.Parse("foo > bar");
    }

    [InlineData("foo\\+bar", "foo+bar")]
    [Theory]
    public void ParseIdentifier(string expression, string identifier)
        => Assert.Equal(identifier, Parser.Identifier.Parse(expression));

    [Fact]
    public void CanParseSelectorWithEscapedChar()
    {
        var selector = Parser.Selector.Parse(@".foo\+bar");

        Assert.Single(selector);
        Assert.Equal(2, selector[0].SelectorSequence.Length);

        Assert.IsType<UniversalSelector>(selector[0].SelectorSequence[0]);
        Assert.IsType<ClassSelector>(selector[0].SelectorSequence[1]);

        var bar = (ClassSelector)selector[0].SelectorSequence[1];

        Assert.Equal("foo+bar", bar.Name);
    }
}