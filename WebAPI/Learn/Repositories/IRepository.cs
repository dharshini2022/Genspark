namespace Learn.Repositories
{
    public interface IRepository<K,T> where T : class
    {
        public Task<T> Create(T item);
        Task<ICollection<T>> CreateBulk(ICollection<T> items);
        public Task<T?> GetById(K key);
        public Task<ICollection<T>> GetAll();
    }
}