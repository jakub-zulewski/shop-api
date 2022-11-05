using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class BasketController : BaseAPIController
{
	private readonly StoreContext _storeContext;

	public BasketController(StoreContext storeContext)
	{
		_storeContext = storeContext;
	}

	[HttpGet(Name = "GetBasket")]
	public async Task<ActionResult<BasketDTO>> GetBasket()
	{
		var basket = await RetrieveBasket();

		if (basket is null)
			return NotFound();

		return MapBasketToDTO(basket);
	}

	[HttpPost]
	public async Task<ActionResult<BasketDTO>> AddItemToBasket(int productId, int quantity)
	{
		var basket = await RetrieveBasket();

		basket ??= await CreateBasket();

		var product = await _storeContext.Products.FindAsync(productId);

		if (product is null)
			return BadRequest(new ProblemDetails { Title = "Product not found." });

		basket.AddItem(product, quantity);

		var result = await _storeContext.SaveChangesAsync() > 0;

		if (result)
			return CreatedAtRoute("GetBasket", MapBasketToDTO(basket));

		return BadRequest(new ProblemDetails { Title = "Problem saving item to basket." });
	}

	[HttpDelete]
	public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
	{
		var basket = await RetrieveBasket();

		if (basket is null)
			return NotFound();

		basket.RemoveItem(productId, quantity);

		var result = await _storeContext.SaveChangesAsync() > 0;

		if (result)
			return Ok();

		return BadRequest(new ProblemDetails
		{
			Title = "Problem removing item from the basket."
		});
	}

	private async Task<Basket?> RetrieveBasket()
	{
		return await _storeContext.Baskets
			.Include(i => i.Items)
			.ThenInclude(p => p.Product)
			.FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]);
	}

	private async Task<Basket> CreateBasket()
	{
		var buyerId = Guid.NewGuid().ToString();
		var cookieOptions = new CookieOptions
		{
			IsEssential = true,
			Expires = DateTime.Now.AddDays(30),
			SameSite = SameSiteMode.None,
			Secure = true
		};

		Response.Cookies.Append("buyerId", buyerId, cookieOptions);

		var basket = new Basket { BuyerId = buyerId };

		await _storeContext.Baskets.AddAsync(basket);

		return basket;
	}

	private static BasketDTO MapBasketToDTO(Basket basket)
	{
		return new BasketDTO
		{
			BuyerId = basket.BuyerId,
			Id = basket.Id,
			Items = basket.Items.Select(item => new BasketItemDTO
			{
				ProductId = item.ProductId,
				Name = item.Product!.Name,
				Brand = item.Product.Brand,
				PictureUrl = item.Product.PictureUrl,
				Price = item.Product.Price,
				Quantity = item.Quantity,
				Type = item.Product.Type
			}).ToList()
		};
	}
}
