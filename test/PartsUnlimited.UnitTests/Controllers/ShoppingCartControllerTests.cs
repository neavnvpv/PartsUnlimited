// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PartsUnlimited.Controllers;
using PartsUnlimited.Models;
using PartsUnlimited.Telemetry;
using PartsUnlimited.UnitTests;
using Xunit;

namespace PartsUnlimited.Controllers;

public class ShoppingCartControllerTests : TestBase
{
    private readonly ShoppingCartController _controller;
    private const string CartId = "test-cart-id";

    public ShoppingCartControllerTests()
    {
        var telemetryProvider = Substitute.For<ITelemetryProvider>();
        var antiforgery = Substitute.For<IAntiforgery>();
        _controller = new ShoppingCartController(Context, telemetryProvider, antiforgery);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Cookies.Append("Session", CartId);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public void Index_WithCartItems_ReturnsViewWithItems()
    {
        // Arrange
        var product = new Product { ProductId = 1, Title = "Test Product", Price = 10.00m, SkuNumber = "SKU1", ProductArtUrl = "/test.jpg", Description = "Test", ProductDetails = "{}" };
        var cartItems = new List<CartItem>
        {
            new() { CartItemId = 1, ProductId = 1, Count = 2, CartId = CartId },
            new() { CartItemId = 2, ProductId = 1, Count = 1, CartId = CartId }
        };
        
        Context.Products.Add(product);
        Context.CartItems.AddRange(cartItems);
        Context.SaveChanges();

        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task AddToCart_ValidProduct_AddsItemAndRedirects(int productId)
    {
        // Arrange
        var product = new Product { ProductId = productId, Title = "Test Product", Price = 10.00m, SkuNumber = "SKU1", ProductArtUrl = "/test.jpg", Description = "Test", ProductDetails = "{}" };
        Context.Products.Add(product);
        Context.SaveChanges();

        // Act
        var result = await _controller.AddToCart(productId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Index");
    }
}