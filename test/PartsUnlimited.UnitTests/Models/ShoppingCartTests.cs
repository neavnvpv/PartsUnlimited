// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using PartsUnlimited.Models;
using PartsUnlimited.UnitTests;
using Xunit;

namespace PartsUnlimited.Models;

public class ShoppingCartTests : TestBase
{
    private readonly ISession _session;

    public ShoppingCartTests()
    {
        _session = Substitute.For<ISession>();
    }

    [Fact]
    public void GetCart_WithValidHttpContext_ReturnsShoppingCart()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Cookies.Append("Session", "test-cart-id");

        // Act
        var cart = ShoppingCart.GetCart(Context, httpContext);

        // Assert
        cart.Should().NotBeNull();
    }

    [Fact]
    public void ShoppingCart_CanBeCreated()
    {
        // Act
        var cart = new ShoppingCart(Context);

        // Assert
        cart.Should().NotBeNull();
    }

    [Fact]
    public void GetCart_WithHttpContext_ReturnsCart()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Cookies.Append("Session", "test-cart-id");

        // Act
        var cart = ShoppingCart.GetCart(Context, httpContext);

        // Assert
        cart.Should().NotBeNull();
    }
}