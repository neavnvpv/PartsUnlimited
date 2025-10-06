// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NSubstitute;
using PartsUnlimited.Models;
using Xunit;

namespace PartsUnlimited.Search;

public class StringContainsProductSearcherTests
{
    private static readonly string[] ProductTitles = ["word in the middle", "something", "something outside", "inside where outside"];

    [Fact]
    public async Task SearchSuccess()
    {
        // Arrange
        var productList = ProductTitles.Select(title => new Product { Title = title }).ToList();
        var context = Substitute.For<IPartsUnlimitedContext>();
        var productDbSet = productList.ToDbSet();
        context.Products.Returns(productDbSet);
        var searcher = new StringContainsProductSearch(context);

        // Act
        var results = await searcher.Search("thing");

        // Assert
        results.Select(p => p.Title).Should().BeEquivalentTo(["something", "something outside"]);
    }

    [Theory]
    [InlineData("", 4)]
    [InlineData("outside", 2)]
    [InlineData("nonexistent", 0)]
    public async Task Search_ReturnsExpectedCount(string searchTerm, int expectedCount)
    {
        // Arrange
        var productList = ProductTitles.Select(title => new Product { Title = title }).ToList();
        var context = Substitute.For<IPartsUnlimitedContext>();
        var productDbSet = productList.ToDbSet();
        context.Products.Returns(productDbSet);
        var searcher = new StringContainsProductSearch(context);

        // Act
        var results = await searcher.Search(searchTerm);

        // Assert
        results.Should().HaveCount(expectedCount);
    }
}