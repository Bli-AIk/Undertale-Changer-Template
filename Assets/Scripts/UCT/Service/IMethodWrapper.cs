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

        public MethodWrapper(Action<object[]> method) => _method = method;

        public void Invoke(params object[] args) => _method(args);
    }

}