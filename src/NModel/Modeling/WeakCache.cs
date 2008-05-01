//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Internals
{
    /// <summary>
    /// Generic object cache that weakly caches the result of a variable list of arguments.
    /// </summary>
    /// <typeparam name="T">Type of object to cache.</typeparam>
    internal abstract class WeakCache<T>
    {
        private Dictionary<object/*?*/[], WeakReference> cache = new Dictionary<object/*?*/[], WeakReference>(ArrayEqualityComparer<object/*?*/>.Default);

        protected T Get(params object/*?*/[] args)
        {
            object target = null;

            WeakReference reference;
            if (cache.TryGetValue(args, out reference))
                target = reference.Target;

            if (reference == null || !reference.IsAlive)
            {
                target = Compute(args);
                cache[args] = new WeakReference(target);
            }

            return (T)target;
        }

        protected abstract T Compute(object/*?*/[] args);
    }

    /// <summary>
    /// Generic object cache that caches the result of a single argument.
    /// </summary>
    /// <typeparam name="T">Type of object to cache.</typeparam>
    /// <typeparam name="A">Type of argument.</typeparam>
    internal class WeakCache<T, A> : WeakCache<T>
    {
        public delegate T Computer(A arg);
        private Computer compute;
        public WeakCache(Computer computer) { compute = computer; }
        public T Get(A arg) { return base.Get(arg); }
        protected override T Compute(object/*?*/[] args) { return compute((A)args[0]); }
    }

    /// <summary>
    /// Generic object cache that caches the result of several arguments.
    /// </summary>
    /// <typeparam name="T">Type of object to cache.</typeparam>
    /// <typeparam name="A1">Type of first argument.</typeparam>
    /// <typeparam name="A2">Type of second argument.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    internal class WeakCache<T, A1, A2> : WeakCache<T>
    {
        public delegate T Computer(A1 arg1, A2 arg2);
        private Computer compute;
        public WeakCache(Computer computer) { compute = computer; }
        public T Get(A1 arg1, A2 arg2) { return base.Get(arg1, arg2); }
        protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1]); }
    }

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3) { return base.Get(arg1, arg2, arg3); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4) { return base.Get(arg1, arg2, arg3, arg4); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    ///// <typeparam name="A5">Type of fifth argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4, A5> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5) { return base.Get(arg1, arg2, arg3, arg4, arg5); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    ///// <typeparam name="A5">Type of fifth argument.</typeparam>
    ///// <typeparam name="A6">Type of sixth argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4, A5, A6> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6) { return base.Get(arg1, arg2, arg3, arg4, arg5, arg6); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4], (A6)args[5]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    ///// <typeparam name="A5">Type of fifth argument.</typeparam>
    ///// <typeparam name="A6">Type of sixth argument.</typeparam>
    ///// <typeparam name="A7">Type of seventh argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4, A5, A6, A7> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7) { return base.Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4], (A6)args[5], (A7)args[6]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    ///// <typeparam name="A5">Type of fifth argument.</typeparam>
    ///// <typeparam name="A6">Type of sixth argument.</typeparam>
    ///// <typeparam name="A7">Type of seventh argument.</typeparam>
    ///// <typeparam name="A8">Type of eighth argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4, A5, A6, A7, A8> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7, A8 arg8);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7, A8 arg8) { return base.Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4], (A6)args[5], (A7)args[6], (A8)args[7]); }
    //}

    ///// <summary>
    ///// Generic object cache that caches the result of several arguments.
    ///// </summary>
    ///// <typeparam name="T">Type of object to cache.</typeparam>
    ///// <typeparam name="A1">Type of first argument.</typeparam>
    ///// <typeparam name="A2">Type of second argument.</typeparam>
    ///// <typeparam name="A3">Type of third argument.</typeparam>
    ///// <typeparam name="A4">Type of fourth argument.</typeparam>
    ///// <typeparam name="A5">Type of fifth argument.</typeparam>
    ///// <typeparam name="A6">Type of sixth argument.</typeparam>
    ///// <typeparam name="A7">Type of seventh argument.</typeparam>
    ///// <typeparam name="A8">Type of eighth argument.</typeparam>
    ///// <typeparam name="A9">Type of ninth argument.</typeparam>
    //[SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    //internal class WeakCache<T, A1, A2, A3, A4, A5, A6, A7, A8, A9> : WeakCache<T>
    //{
    //    public delegate T Computer(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7, A8 arg8, A9 arg9);
    //    private Computer compute;
    //    public WeakCache(Computer computer) { compute = computer; }
    //    public T Get(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5, A6 arg6, A7 arg7, A8 arg8, A9 arg9) { return base.Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9); }
    //    protected override T Compute(object/*?*/[] args) { return compute((A1)args[0], (A2)args[1], (A3)args[2], (A4)args[3], (A5)args[4], (A6)args[5], (A7)args[6], (A8)args[7], (A9)args[8]); }
    //}
}
