﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal sealed class LegacyReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
    {
        private LegacyReferenceEqualityComparer()
        {
        }

        public static LegacyReferenceEqualityComparer Instance { get; } = new LegacyReferenceEqualityComparer();

        public new bool Equals(object x, object y)
            => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);

        bool IEqualityComparer<object>.Equals(object x, object y)
            => ReferenceEquals(x, y);

        int IEqualityComparer.GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);

        bool IEqualityComparer.Equals(object x, object y)
            => ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);
    }
}
