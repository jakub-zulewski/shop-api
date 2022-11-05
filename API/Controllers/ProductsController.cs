using API.Data;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController : BaseAPIController
{
	private readonly StoreContext _storeContext;

	public ProductsController(StoreContext storeContext)
	{
		_storeContext = storeContext;
	}

	[HttpGet]
	public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery] ProductParams productParams)
	{
		var query = _storeContext.Products
			.Sort(productParams.OrderBy!)
			.Search(productParams.SearchTerm!)
			.Filter(productParams.Brands!, productParams.Types!).AsQueryable();

		var products = await PagedList<Product>.ToPagedList(query, productParams.PageNumber, productParams.PageSize);

		Response.AddPaginationHeader(products.MetaData);

		return Ok(products);
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
