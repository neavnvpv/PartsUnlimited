// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace PartsUnlimited.Utils;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void ToLookup_ShouldParsePathsCorrectly()
    {
        // Arrange
        var values = new[]
        {
            new ConfigPathHelper { Name = "item1", Path = "path1 path3  path4\tpath5\rpath6\n\rpath7" },
            new ConfigPathHelper { Name = "item2", Path = "path2" },
        };
        var config = CreateConfig(values);

        // Act
        var lookup = config.ToLookup();

        // Assert
        lookup.Should().HaveCount(2);
        lookup["item1"].Should().BeEquivalentTo(["path1", "path3", "path4", "path5", "path6", "path7"]);
        lookup["item2"].Should().BeEquivalentTo(["path2"]);
    }

    [Theory]
    [InlineData("", new string[0])]
    [InlineData("single", new[] { "single" })]
    [InlineData("  multiple   spaces  ", new[] { "multiple", "spaces" })]
    public void ToLookup_ShouldHandleEdgeCases(string path, string[] expected)
    {
        // Arrange
        var values = new[] { new ConfigPathHelper { Name = "test", Path = path } };
        var config = CreateConfig(values);

        // Act
        var lookup = config.ToLookup();

        // Assert
        lookup["test"].Should().BeEquivalentTo(expected);
    }

    private static IConfiguration CreateConfig(IEnumerable<ConfigPathHelper> values)
    {
        var config = Substitute.For<IConfiguration>();
        var sections = values.Select(value =>
        {
            var section = Substitute.For<IConfigurationSection>();
            section.Key.Returns(value.Name);
            section.Value.Returns(value.Path);
            return section;
        }).ToList();

        config.GetChildren().Returns(sections);
        return config;
    }

    private record ConfigPathHelper
    {
        public required string Path { get; init; }
        public required string Name { get; init; }
    }
}