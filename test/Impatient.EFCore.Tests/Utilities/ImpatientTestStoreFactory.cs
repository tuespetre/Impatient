﻿using Impatient.EntityFrameworkCore.SqlServer;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Impatient.EFCore.Tests.Utilities
{
    public class ImpatientTestStoreFactory : ITestStoreFactory
    {
        public static ImpatientTestStoreFactory Instance { get; } = new ImpatientTestStoreFactory();

        private readonly ImpatientCompatibility compatibility = ImpatientCompatibility.Default;

        private ImpatientTestStoreFactory()
        {
        }

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddEntityFrameworkSqlServer()
                .AddImpatientEFCoreQueryCompiler(compatibility)
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

        public TestStore Create(string storeName)
        {
            if (storeName.Equals("Northwind", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ImpatientNorthwindTestStore();
            }
            else
            {
                return new ImpatientTestStore(storeName, false);
            }
        }

        public TestStore GetOrCreate(string storeName)
        {
            if (storeName.Equals("Northwind", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ImpatientNorthwindTestStore();
            }
            else
            {
                return new ImpatientTestStore(storeName, true);
            }
        }

        public ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        {
            return new TestSqlLoggerFactory(shouldLogCategory);
        }
    }
}
