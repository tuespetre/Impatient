using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class UpdatesImpatientTest : UpdatesRelationalTestBase<UpdatesImpatientTest.UpdatesImpatientFixture>
    {
        public UpdatesImpatientTest(UpdatesImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public override void Identifiers_are_generated_correctly()
        {
            using (var context = CreateContext())
            {
                var entityType = context.Model.FindEntityType(typeof(LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
                Assert.Equal("LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorking~", entityType.Relational().TableName);
                Assert.Equal("PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetKeys().Single().Relational().Name);
                Assert.Equal("FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetForeignKeys().Single().Relational().Name);
                Assert.Equal("IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetIndexes().Single().Relational().Name);
            }
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class UpdatesImpatientFixture : UpdatesRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning)
                        .Log(SqlServerEventId.DecimalTypeDefaultWarning));

                new SqlServerDbContextOptionsBuilder(options).MinBatchSize(1);

                return options;
            }
        }
    }
}
