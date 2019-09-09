namespace RPCExp.Store
{
    public abstract class ClassWrapperAbstract<T>
    {
        public virtual string ClassName => GetType().Name;

        public abstract void Wrap(T obj);

        public abstract T Unwrap();
    }

}
