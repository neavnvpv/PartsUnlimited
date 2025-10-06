// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using PartsUnlimited.Models;
using PartsUnlimited.UnitTests;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace PartsUnlimited.Models;

public class ProductTests : TestBase
{
    [Fact]
    public async Task Product_CanBeCreatedAndSaved()
    {
        // Arrange
        var product = new Product
        {
            Title = "Test Product",
            Price = 19.99m,
            ProductArtUrl = "/images/test.jpg",
            ProductDetails = "Test product details",
            Description = "Test description",
            SkuNumber = "SKU123",
            Inventory = 10,
            LeadTime = 5
        };

        // Act
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var savedProduct = await Context.Products.FindAsync(product.ProductId);
        savedProduct.Should().NotBeNull();
        savedProduct!.Title.Should().Be("Test Product");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Product_RequiresTitle(string? title)
    {
        // Arrange & Act
        var product = new Product { Title = title! };

        // Assert
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true);
        
        isValid.Should().BeFalse();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Title"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public void Product_RequiresPositivePrice(decimal price)
    {
        // Arrange & Act
        var product = new Product { Title = "Test", Price = price };

        // Assert
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true);
        
        isValid.Should().BeFalse();
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Price"));
    }

    [Fact]
    public async Task Product_CanHaveCategory()
    {
        // Arrange
        var category = new Category { Name = "Electronics", Description = "Electronics category", ImageUrl = "/test.jpg" };
        var product = new Product 
        { 
            Title = "Test Product", 
            Price = 19.99m,
            SkuNumber = "SKU123",
            ProductArtUrl = "/test.jpg",
            Description = "Test",
            ProductDetails = "{}",
            Category = category 
        };

        // Act
        await Context.Categories.AddAsync(category);
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var savedProduct = Context.Products.First(p => p.ProductId == product.ProductId);
        savedProduct.CategoryId.Should().Be(category.CategoryId);
    }
}