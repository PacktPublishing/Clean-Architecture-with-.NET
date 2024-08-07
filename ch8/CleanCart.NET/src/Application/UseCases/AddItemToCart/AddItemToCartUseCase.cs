﻿using Application.Interfaces.Data;
using Application.Interfaces.UseCases;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.UseCases.AddItemToCart
{
    public class AddItemToCartUseCase : IAddItemToCartUseCase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IProductRepository _productRepository;

        public AddItemToCartUseCase(
            IShoppingCartRepository shoppingCartRepository,
            IProductRepository productRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _productRepository = productRepository;
        }

        public async Task AddItemToCartAsync(AddItemToCartInput input)
        {
            ShoppingCart cart = await _shoppingCartRepository.GetByUserIdAsync(input.UserId) ?? new ShoppingCart(input.UserId);
            Product product = await _productRepository.GetByIdAsync(input.ProductId);

            cart.AddItem(product, input.Quantity);

            await _shoppingCartRepository.SaveAsync(cart);
        }
    }
}
