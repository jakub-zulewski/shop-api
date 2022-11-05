using API.Entities;

namespace API.Extensions;

public static class ProductExtensions
{
	public static IQueryable<Product> Sort(this IQueryable<Product> query, string orderBy)
	{
		if (string.IsNullOrWhiteSpace(orderBy))
			return query.OrderBy(p => p.Name);

		query = orderBy.ToLower() switch
		{
			"price" => query.OrderBy(p => p.Price),
			"pricedesc" => query.OrderByDescending(p => p.Price),
			_ => query.OrderBy(p => p.Name)
		};

		return query;
	}
}
