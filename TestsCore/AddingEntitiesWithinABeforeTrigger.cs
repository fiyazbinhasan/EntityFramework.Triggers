﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
# if NETCOREAPP2_0
namespace EntityFrameworkCore.Triggers.Tests {
# else
namespace EntityFrameworkCore.Triggers.Tests.Net461 {
# endif
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
namespace EntityFramework.Triggers.Tests {
#endif
    public class InsideAnInsertingTriggerInsertAnEntityWhichHasInsertingTriggers : ThingTestBase {
		// Note that `Person` has triggers via its base classes `EntityWithTracking` and `EntityWithInsertTracking`
		private readonly String lastName = Guid.NewGuid().ToString();
		private void TriggersOnInserting(IBeforeEntry<Thing, Context> beforeEntry) => beforeEntry.Context.People.Add(new Person { LastName = lastName });

		protected override void Setup() {
			base.Setup();
			Triggers<Thing, Context>.Inserting += TriggersOnInserting;
		}
		protected override void Teardown() {
			Triggers<Thing, Context>.Inserting -= TriggersOnInserting;
			base.Teardown();
		}

		[Fact]
		public void Sync() => DoATest(() => {
			var guid = Guid.NewGuid().ToString();
			Context.Things.Add(new Thing { Value = guid });
			Context.SaveChanges();
			Assert.True(Context.Things.SingleOrDefault(x => x.Value == guid) != null);
			Assert.True(Context.People.SingleOrDefault(x => x.LastName == lastName) != null);
		});
        
		[Fact]
		public Task Async() => DoATestAsync(async () => {
			var guid = Guid.NewGuid().ToString();
			Context.Things.Add(new Thing { Value = guid });
			await Context.SaveChangesAsync().ConfigureAwait(false);
			Assert.True(await Context.Things.SingleOrDefaultAsync(x => x.Value == guid).ConfigureAwait(false) != null);
			Assert.True(await Context.People.SingleOrDefaultAsync(x => x.LastName == lastName).ConfigureAwait(false) != null);
		});
	}
}
