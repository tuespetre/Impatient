using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

#pragma warning disable CS0472

namespace Impatient.Tests.ExpressionVisitors.Optimizing
{
    [TestClass]
    public class NullOrDefaultEqualityOptimizingExpressionVisitorTests
    {
        private static Expression parameter = Parameter(typeof(TestClass), "t");

        private static Expression referenceTypeMemberAccess
            = MakeMemberAccess(parameter, typeof(TestClass).GetProperty(nameof(TestClass.ReferenceTypeProperty)));

        private static Expression valueTypeMemberAccess
            = MakeMemberAccess(parameter, typeof(TestClass).GetProperty(nameof(TestClass.ValueTypeProperty)));

        private static Expression nullableTypeMemberAccess
            = MakeMemberAccess(parameter, typeof(TestClass).GetProperty(nameof(TestClass.NullableTypeProperty)));

        [TestMethod]
        public void NullConstant_Equal_NullConstant()
        {
            AssertTransformation(
                Equal(Constant(null), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NullConstant()
        {
            AssertTransformation(
                NotEqual(Constant(null), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_Equal_StringConstant()
        {
            AssertTransformation(
                Equal(Constant(null), Constant("string")),
                Constant(false));

            AssertTransformation(
                Equal(Constant("string"), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_StringConstant()
        {
            AssertTransformation(
                NotEqual(Constant(null), Constant("string")),
                Constant(true));

            AssertTransformation(
                NotEqual(Constant("string"), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_IntConstant()
        {
            AssertTransformation(
                Equal(Constant(null), Convert(Constant(0), typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(Constant(0), typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_IntConstant()
        {
            AssertTransformation(
                NotEqual(Constant(null), Convert(Constant(0), typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(Constant(0), typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_NullableIntConstant()
        {
            AssertTransformation(
                Equal(Constant(null), Constant(0, typeof(int?))),
                Constant(false));

            AssertTransformation(
                Equal(Constant(0, typeof(int?)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NullableIntConstant()
        {
            AssertTransformation(
                NotEqual(Constant(null), Constant(0, typeof(int?))),
                Constant(true));

            AssertTransformation(
                NotEqual(Constant(0, typeof(int?)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_DefaultReferenceType()
        {
            AssertTransformation(
                Equal(Constant(null), Default(typeof(string))),
                Constant(true));

            AssertTransformation(
                Equal(Default(typeof(string)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_NotEqual_DefaultReferenceType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Default(typeof(string))),
                Constant(false));

            AssertTransformation(
                NotEqual(Default(typeof(string)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_Equal_DefaultValueType()
        {
            AssertTransformation(
                Equal(Constant(null), Convert(Default(typeof(int)), typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(Default(typeof(int)), typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_DefaultValueType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Convert(Default(typeof(int)), typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(Default(typeof(int)), typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_DefaultNullableType()
        {
            AssertTransformation(
                Equal(Constant(null), Default(typeof(int?))),
                Constant(true));

            AssertTransformation(
                Equal(Default(typeof(int?)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_NotEqual_DefaultNullableType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Default(typeof(int?))),
                Constant(false));

            AssertTransformation(
                NotEqual(Default(typeof(int?)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_DefaultReferenceType()
        {
            AssertTransformation(
                Equal(Default(typeof(object)), Default(typeof(string))),
                Constant(true));

            AssertTransformation(
                Equal(Default(typeof(string)), Default(typeof(object))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultReferenceType_NotEqual_DefaultReferenceType()
        {
            AssertTransformation(
                NotEqual(Default(typeof(object)), Default(typeof(string))),
                Constant(false));

            AssertTransformation(
                NotEqual(Default(typeof(string)), Default(typeof(object))),
                Constant(false));
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_DefaultValueType()
        {
            AssertTransformation(
                Equal(Default(typeof(object)), Convert(Default(typeof(int)), typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(Default(typeof(int)), typeof(object)), Default(typeof(object))),
                Constant(false));
        }

        [TestMethod]
        public void DefaultReferenceType_NotEqual_DefaultValueType()
        {
            AssertTransformation(
                NotEqual(Default(typeof(object)), Convert(Default(typeof(int)), typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(Default(typeof(int)), typeof(object)), Default(typeof(object))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_DefaultNullableType()
        {
            AssertTransformation(
                Equal(Default(typeof(object)), Convert(Default(typeof(int?)), typeof(object))),
                Constant(true));

            AssertTransformation(
                Equal(Convert(Default(typeof(int?)), typeof(object)), Default(typeof(object))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultReferenceType_NotEqual_DefaultNullableType()
        {
            AssertTransformation(
                NotEqual(Default(typeof(object)), Convert(Default(typeof(int?)), typeof(object))),
                Constant(false));

            AssertTransformation(
                NotEqual(Convert(Default(typeof(int?)), typeof(object)), Default(typeof(object))),
                Constant(false));
        }

        [TestMethod]
        public void DefaultValueType_Equal_DefaultValueType()
        {
            AssertTransformation(
                Equal(Default(typeof(int)), Default(typeof(int))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultValueType_NotEqual_DefaultValueType()
        {
            AssertTransformation(
                NotEqual(Default(typeof(int)), Default(typeof(int))),
                Constant(false));
        }

        [TestMethod]
        public void DefaultValueType_Equal_DefaultNullableType()
        {
            AssertTransformation(
                Equal(Convert(Default(typeof(int)), typeof(int?)), Default(typeof(int?))),
                Constant(false));

            AssertTransformation(
                Equal(Default(typeof(int?)), Convert(Default(typeof(int)), typeof(int?))),
                Constant(false));
        }

        [TestMethod]
        public void DefaultValueType_NotEqual_DefaultNullableType()
        {
            AssertTransformation(
                NotEqual(Convert(Default(typeof(int)), typeof(int?)), Default(typeof(int?))),
                Constant(true));

            AssertTransformation(
                NotEqual(Default(typeof(int?)), Convert(Default(typeof(int)), typeof(int?))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultNullableType_Equal_DefaultNullableType()
        {
            AssertTransformation(
                Equal(Default(typeof(int?)), Default(typeof(int?))),
                Constant(true));
        }

        [TestMethod]
        public void DefaultNullableType_NotEqual_DefaultNullableType()
        {
            AssertTransformation(
                NotEqual(Default(typeof(int?)), Default(typeof(int?))),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_Equal_ReferenceType()
        {
            AssertTransformation(
                Equal(Constant(null), referenceTypeMemberAccess),
                Equal(Constant(null), referenceTypeMemberAccess));

            AssertTransformation(
                Equal(referenceTypeMemberAccess, Constant(null)),
                Equal(referenceTypeMemberAccess, Constant(null)));
        }

        [TestMethod]
        public void NullConstant_NotEqual_ReferenceType()
        {
            AssertTransformation(
                NotEqual(Constant(null), referenceTypeMemberAccess),
                NotEqual(Constant(null), referenceTypeMemberAccess));

            AssertTransformation(
                NotEqual(referenceTypeMemberAccess, Constant(null)),
                NotEqual(referenceTypeMemberAccess, Constant(null)));
        }

        [TestMethod]
        public void NullConstant_Equal_ValueType()
        {
            AssertTransformation(
                Equal(Constant(null), Convert(valueTypeMemberAccess, typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(valueTypeMemberAccess, typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_ValueType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Convert(valueTypeMemberAccess, typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(valueTypeMemberAccess, typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_NullableType()
        {
            AssertTransformation(
                Equal(Constant(null), nullableTypeMemberAccess),
                Equal(Constant(null), nullableTypeMemberAccess));

            AssertTransformation(
                Equal(nullableTypeMemberAccess, Constant(null)),
                Equal(nullableTypeMemberAccess, Constant(null)));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NullableType()
        {
            AssertTransformation(
                NotEqual(Constant(null), nullableTypeMemberAccess),
                NotEqual(Constant(null), nullableTypeMemberAccess));

            AssertTransformation(
                NotEqual(nullableTypeMemberAccess, Constant(null)),
                NotEqual(nullableTypeMemberAccess, Constant(null)));
        }

        [TestMethod]
        public void NullConstant_Equal_NewReferenceType()
        {
            AssertTransformation(
                Equal(Constant(null), New(typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(New(typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NewReferenceType()
        {
            AssertTransformation(
                NotEqual(Constant(null), New(typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(New(typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_NewValueType()
        {
            AssertTransformation(
                Equal(Constant(null), Convert(New(typeof(int)), typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(New(typeof(int)), typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NewValueType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Convert(New(typeof(int)), typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(New(typeof(int)), typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_NewNullableType()
        {
            AssertTransformation(
                Equal(Constant(null), New(typeof(int?))),
                Constant(true));

            AssertTransformation(
                Equal(New(typeof(int?)), Constant(null)),
                Constant(true));

            AssertTransformation(
                Equal(Constant(null), New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0))),
                Constant(false));

            AssertTransformation(
                Equal(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NewNullableType()
        {
            AssertTransformation(
                NotEqual(Constant(null), New(typeof(int?))),
                Constant(false));

            AssertTransformation(
                NotEqual(New(typeof(int?)), Constant(null)),
                Constant(false));

            AssertTransformation(
                NotEqual(Constant(null), New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0))),
                Constant(true));

            AssertTransformation(
                NotEqual(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_MemberInitReferenceType()
        {
            AssertTransformation(
                Equal(Constant(null), MemberInit(New(typeof(object)))),
                Constant(false));

            AssertTransformation(
                Equal(MemberInit(New(typeof(object))), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_MemberInitReferenceType()
        {
            AssertTransformation(
                NotEqual(Constant(null), MemberInit(New(typeof(object)))),
                Constant(true));

            AssertTransformation(
                NotEqual(MemberInit(New(typeof(object))), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_MemberInitValueType()
        {
            AssertTransformation(
                Equal(Constant(null), Convert(MemberInit(New(typeof(int))), typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(Convert(MemberInit(New(typeof(int))), typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_MemberInitValueType()
        {
            AssertTransformation(
                NotEqual(Constant(null), Convert(MemberInit(New(typeof(int))), typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(Convert(MemberInit(New(typeof(int))), typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_MemberInitNullableType()
        {
            AssertTransformation(
                Equal(Constant(null), MemberInit(New(typeof(int?)))),
                Constant(true));

            AssertTransformation(
                Equal(MemberInit(New(typeof(int?))), Constant(null)),
                Constant(true));

            AssertTransformation(
                Equal(Constant(null), MemberInit(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0)))),
                Constant(false));

            AssertTransformation(
                Equal(MemberInit(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0))), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_MemberInitNullableType()
        {
            AssertTransformation(
                NotEqual(Constant(null), MemberInit(New(typeof(int?)))),
                Constant(false));

            AssertTransformation(
                NotEqual(MemberInit(New(typeof(int?))), Constant(null)),
                Constant(false));

            AssertTransformation(
                NotEqual(Constant(null), MemberInit(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0)))),
                Constant(true));

            AssertTransformation(
                NotEqual(MemberInit(New(typeof(int?).GetConstructor(new[] { typeof(int) }), Constant(0))), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_NewArray()
        {
            AssertTransformation(
                Equal(Constant(null), NewArrayInit(typeof(object))),
                Constant(false));

            AssertTransformation(
                Equal(NewArrayInit(typeof(object)), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_NewArray()
        {
            AssertTransformation(
                NotEqual(Constant(null), NewArrayInit(typeof(object))),
                Constant(true));

            AssertTransformation(
                NotEqual(NewArrayInit(typeof(object)), Constant(null)),
                Constant(true));
        }

        [TestMethod]
        public void NullConstant_Equal_ListInit()
        {
            AssertTransformation(
                Equal(Constant(null), ListInit(New(typeof(List<object>)), Array.Empty<Expression>())),
                Constant(false));

            AssertTransformation(
                Equal(ListInit(New(typeof(List<object>)), Array.Empty<Expression>()), Constant(null)),
                Constant(false));
        }

        [TestMethod]
        public void NullConstant_NotEqual_ListInit()
        {
            AssertTransformation(
                NotEqual(Constant(null), ListInit(New(typeof(List<object>)), Array.Empty<Expression>())),
                Constant(true));

            AssertTransformation(
                NotEqual(ListInit(New(typeof(List<object>)), Array.Empty<Expression>()), Constant(null)),
                Constant(true));
        }

        /*
        [TestMethod]
        public void DefaultReferenceType_Equal_ReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_ValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_NullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_NewReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_NewValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_NewNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_MemberInitReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_MemberInitValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_MemberInitNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_NewArray()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultReferenceType_Equal_ListInit()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_ReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_ValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_NullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_NewReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_NewValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_NewNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_MemberInitReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_MemberInitValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_MemberInitNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_NewArray()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultValueType_Equal_ListInit()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_ReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_ValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_NullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_NewReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_NewValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_NewNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_MemberInitReferenceType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_MemberInitValueType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_MemberInitNullableType()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_NewArray()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DefaultNullableType_Equal_ListInit()
        {
            Assert.Fail();
        }
        */

        private static void AssertTransformation<T>(
            Expression<Func<TestClass, T>> input,
            Expression<Func<TestClass, T>> output)
        {
            AssertTransformation(input.Body, output.Body);
        }

        private static void AssertTransformation(
            Expression input,
            Expression output)
        {
            var visitor = new NullOrDefaultEqualityOptimizingExpressionVisitor();

            var result = visitor.Visit(input);

            var inputHash = ExpressionEqualityComparer.Instance.GetHashCode(result);

            var outputHash = ExpressionEqualityComparer.Instance.GetHashCode(output);

            if (inputHash != outputHash)
            {
                Assert.Fail($"Output expression trees do not match.\r\nExpected: {output}\r\nActual: {result}");
            }
        }

        private class TestClass
        {
            public int ValueTypeProperty { get; set; }

            public string ReferenceTypeProperty { get; set; }

            public bool? NullableTypeProperty { get; set; }
        }
    }
}
