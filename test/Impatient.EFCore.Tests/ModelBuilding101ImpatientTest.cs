using Microsoft.EntityFrameworkCore;
using System;

namespace Impatient.EFCore.Tests
{
    public class ModelBuilding101ImpatientTest : ModelBuilding101RelationalTestBase
    {
        protected override DbContextOptionsBuilder ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
