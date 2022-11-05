using API.Data;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class ProductsController : BaseAPIController
{
	private readonly StoreContext _storeContext;

	public ProductsController(StoreContext storeContext)
	{
		_storeContext = storeContext;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Product>>> GetProducts(string? orderBy, string? searchTerm)
	{
		var query = _storeContext.Products.Sort(orderBy!).Search(searchTerm!).AsQueryable();

		return await query.ToListAsync();
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<Product>> GetProduct(int id)
	{
		var product = await _storeContext.Products.FindAsync(id);

		if (product is null)
			return NotFound();

		return Ok(product);
	}
}
