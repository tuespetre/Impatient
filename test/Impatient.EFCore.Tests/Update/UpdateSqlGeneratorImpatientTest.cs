using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Text;

namespace Impatient.EFCore.Tests.Update
{
    public class UpdateSqlGeneratorImpatientTest : UpdateSqlGeneratorTestBase
    {
        protected override string RowsAffected => throw new NotImplementedException();

        protected override TestHelpers TestHelpers => throw new NotImplementedException();

        protected override void AppendDeleteOperation_creates_full_delete_command_text_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendInsertOperation_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendInsertOperation_for_only_identity_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendInsertOperation_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendInsertOperation_insert_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendUpdateOperation_appends_where_for_concurrency_token_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendUpdateOperation_for_computed_property_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void AppendUpdateOperation_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        protected override IUpdateSqlGenerator CreateSqlGenerator()
        {
            throw new NotImplementedException();
        }
    }
}
