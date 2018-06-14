using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsBuilder
    {
        private readonly DbContextOptionsBuilder builder;

        public ImpatientDbContextOptionsBuilder(DbContextOptionsBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public ImpatientDbContextOptionsBuilder WithCompatibility(ImpatientCompatibility compatibility)
        {
            return WithOption(e => e.WithCompatibility(compatibility));
        }

        private ImpatientDbContextOptionsBuilder WithOption(Func<ImpatientDbContextOptionsExtension, ImpatientDbContextOptionsExtension> setter)
        {
            var extension 
                = builder.Options.FindExtension<ImpatientDbContextOptionsExtension>() 
                ?? new ImpatientDbContextOptionsExtension();

            extension = setter(extension);

            ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
