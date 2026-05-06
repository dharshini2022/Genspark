namespace NotificationSystem.Interfaces
{
    internal interface IRepository<K,T> where T : class
    {
        public T Create(T item);
        public T? GetEntity(K key);
        public List<T>? GetAllEntities();
        public T? Update(K key,T item);
        public T? Delete(K key);
        
    }
}