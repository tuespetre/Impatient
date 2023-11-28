using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Threading.Tasks;

#pragma warning disable xUnit1024 // Test methods cannot have overloads
namespace Impatient.EFCore.Tests.Update
{
    public class StoredProcedureUpdateImpatientTest : StoredProcedureUpdateTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public override Task Delete(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Delete_and_insert(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Input_or_output_parameter_with_input(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Input_or_output_parameter_with_output(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Insert_twice_with_output_parameter(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Insert_with_output_parameter(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Insert_with_output_parameter_and_result_column(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Insert_with_result_column(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Insert_with_two_result_columns(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Non_sproc_followed_by_sproc_commands_in_the_same_batch(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Original_and_current_value_on_non_concurrency_token(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_parameter(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_parameter_and_concurrency_failure(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_result_column(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_result_column_and_concurrency_failure(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_return_value(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Rows_affected_return_value_and_concurrency_failure(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Store_generated_concurrency_token_as_in_out_parameter(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Store_generated_concurrency_token_as_two_parameters(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Tpc(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Tph(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Tpt(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Tpt_mixed_sproc_and_non_sproc(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Update(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Update_partial(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Update_with_output_parameter_and_rows_affected_result_column(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async)
        {
            throw new NotImplementedException();
        }

        public override Task User_managed_concurrency_token(bool async)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore xUnit1024 // Test methods cannot have overloads