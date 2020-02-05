using Microsoft.EntityFrameworkCore;

namespace Metrics.UnitTests.EfCore
{
	public class FakeDbContext : DbContext
	{
		public FakeDbContext(DbContextOptions<FakeDbContext> options)
			: base(options) { }
	}
}
