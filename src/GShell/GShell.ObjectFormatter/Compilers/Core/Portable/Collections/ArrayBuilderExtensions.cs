// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal static class ArrayBuilderExtensions
    {
        public static void AddOptional<T>(this ArrayBuilder<T> builder, T? item)
            where T : class
        {
            if (item != null)
            {
                builder.Add(item);
            }
        }

        // The following extension methods allow an ArrayBuilder to be used as a stack. 
        // Note that the order of an IEnumerable from a List is from bottom to top of stack. An IEnumerable 
        // from the framework Stack is from top to bottom.
        public static void Push<T>(this ArrayBuilder<T> builder, T e)
        {
            builder.Add(e);
        }

        public static T Pop<T>(this ArrayBuilder<T> builder)
        {
            var e = builder.Peek();
            builder.RemoveAt(builder.Count - 1);
            return e;
        }

        public static bool TryPop<T>(this ArrayBuilder<T> builder, out T? result)
        {
            if (builder.Count > 0)
            {
                result = builder.Pop();
                return true;
            }

            result = default;
            return false;
        }

        public static T Peek<T>(this ArrayBuilder<T> builder)
        {
            return builder[builder.Count - 1];
        }

        public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T? value)
            where T : struct
        {
            if (value != null)
            {
                builder.Add(value.Value);
            }
        }

        public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T? value)
            where T : class
        {
            if (value != null)
            {
                builder.Add(value);
            }
        }

#if COMPILERCORE

        /// <summary>
        /// Realizes the OneOrMany and disposes the builder in one operation.
        /// </summary>
        public static OneOrMany<T> ToOneOrManyAndFree<T>(this ArrayBuilder<T> builder)
        {
            if (builder.Count == 1)
            {
                var result = OneOrMany.Create(builder[0]);
                builder.Free();
                return result;
            }
            else
            {
                return OneOrMany.Create(builder.ToImmutableAndFree());
            }
        }

#endif
    }
}
