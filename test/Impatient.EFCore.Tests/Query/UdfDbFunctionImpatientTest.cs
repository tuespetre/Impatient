// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable InconsistentNaming

namespace Impatient.EFCore.Tests.Query
{
    public class UdfDbFunctionImpatientTest : UdfDbFunctionTestBase<UdfDbFunctionImpatientTest.SqlServerUDFFixture>
    {
        public UdfDbFunctionImpatientTest(SqlServerUDFFixture fixture) : base(fixture)
        {
            Fixture.ListLoggerFactory.Clear();
        }

        public class SqlServerUDFFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "UDFDbFunctionSqlServerTests";

            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
