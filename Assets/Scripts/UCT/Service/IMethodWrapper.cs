using System;

namespace UCT.Service
{
    public interface IMethodWrapper
    {
        void Invoke(params object[] args);
    }

    public class MethodWrapper : IMethodWrapper
    {
        private readonly Action<object[]> _method;

        public MethodWrapper(Action<object[]> method)
        {
            _method = method;
        }

        public void Invoke(params object[] args)
        {
            _method(args);
        }
    }

    public interface IMethodWrapper<out TResult>
    {
        TResult Invoke(params object[] args);
    }

    public class MethodWrapper<TResult> : IMethodWrapper<TResult>
    {
        private readonly Func<object[], TResult> _method;

        public MethodWrapper(Func<object[], TResult> method)
        {
            _method = method;
        }

        public TResult Invoke(params object[] args)
        {
            return _method(args);
        }
    }
}