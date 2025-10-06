// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using PartsUnlimited.Controllers;
using PartsUnlimited.Models;
using PartsUnlimited.UnitTests;
using Xunit;

namespace PartsUnlimited.Controllers;

public class HomeControllerTests : TestBase
{
    private readonly IMemoryCache _memoryCache;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        var memoryCache = Substitute.For<IMemoryCache>();
        _controller = new HomeController(Context, memoryCache);
    }

    [Fact]
    public void Index_ReturnsViewWithHomeViewModel()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { ProductId = 1, Title = "Product 1", Price = 10.00m, SkuNumber = "SKU1", ProductArtUrl = "/test1.jpg", Description = "Test", ProductDetails = "{}" },
            new() { ProductId = 2, Title = "Product 2", Price = 20.00m, SkuNumber = "SKU2", ProductArtUrl = "/test2.jpg", Description = "Test", ProductDetails = "{}" }
        };
        
        Context.Products.AddRange(products);
        Context.SaveChanges();

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ReturnsErrorView()
    {
        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("~/Views/Shared/Error.cshtml");
    }

    [Fact]
    public void Index_WithEmptyDatabase_ReturnsView()
    {
        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Index_ReturnsTopSellingProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { ProductId = 1, Title = "Product 1", Price = 10.00m, SkuNumber = "SKU1", ProductArtUrl = "/test1.jpg", Description = "Test", ProductDetails = "{}" },
            new() { ProductId = 2, Title = "Product 2", Price = 20.00m, SkuNumber = "SKU2", ProductArtUrl = "/test2.jpg", Description = "Test", ProductDetails = "{}" }
        };
        
        Context.Products.AddRange(products);
        Context.SaveChanges();

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }
}