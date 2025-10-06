// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using PartsUnlimited.WebsiteConfiguration;
using Xunit;

namespace PartsUnlimited.Utils;

public class ContentDeliveryNetworkConfigurationTests
{
    [Theory]
    [InlineData("some/test/path")]
    [InlineData("https://cdn.example.com/images")]
    public void Images_ShouldReturnConfiguredValue(string imageCdn)
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["images"].Returns(imageCdn);

        // Act
        var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

        // Assert
        cdnConfig.Images.Should().Be(imageCdn);
    }

    [Fact]
    public void Images_ShouldReturnNull_WhenNotConfigured()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        config["images"].Returns((string?)null);

        // Act
        var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

        // Assert
        cdnConfig.Images.Should().BeNull();
    }

    [Fact]
    public void Scripts_ShouldParseMultiplePaths()
    {
        // Arrange
        var values = new[]
        {
            new ConfigPathHelper { Name = "item1", Path = "path1 path3  path4" },
            new ConfigPathHelper { Name = "item2", Path = "path2" },
        };
        var config = Substitute.For<IConfiguration>();
        var scriptsConfigSection = CreateConfigSection(values);
        config.GetSection("Scripts").Returns(scriptsConfigSection);

        // Act
        var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

        // Assert
        cdnConfig.Scripts.Should().HaveCount(2);
        cdnConfig.Scripts["item1"].Should().BeEquivalentTo(["path1", "path3", "path4"]);
        cdnConfig.Scripts["item2"].Should().BeEquivalentTo(["path2"]);
    }

    [Fact]
    public void Scripts_ShouldHandleEmptyConfiguration()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        var emptySection = Substitute.For<IConfigurationSection>();
        emptySection.GetChildren().Returns([]);
        config.GetSection("Scripts").Returns(emptySection);

        // Act
        var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

        // Assert
        cdnConfig.Scripts.Should().BeEmpty();
    }

    private static IConfigurationSection CreateConfigSection(IEnumerable<ConfigPathHelper> values)
    {
        var section = Substitute.For<IConfigurationSection>();
        var sections = values.Select(value =>
        {
            var childSection = Substitute.For<IConfigurationSection>();
            childSection.Key.Returns(value.Name);
            childSection.Value.Returns(value.Path);
            return childSection;
        }).ToList();

        section.GetChildren().Returns(sections);
        return section;
    }

    private record ConfigPathHelper
    {
        public required string Path { get; init; }
        public required string Name { get; init; }
    }
}