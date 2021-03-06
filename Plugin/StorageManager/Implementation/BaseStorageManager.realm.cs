using System.Linq;
using Realms;

namespace Plugin.StorageManager
{
    /// <summary>
    /// DB implementation using Realm
    /// </summary>
    public class BaseStorageManagerImplementation : IStorageManager
    {
        public Realm RealmInstance;

        /// <summary>
        /// Add item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Add<T>(T item) where T : RealmObject, IStorageModel
        {
            RealmInstance.Write(() => RealmInstance.Add(item));
        }

        /// <summary>
        /// Get all
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> All<T>() where T : RealmObject, IStorageModel
        {
            return RealmInstance.All<T>();
        }

        /// <summary>
        /// Remove item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Remove<T>(T item) where T : RealmObject, IStorageModel
        {
            using (Transaction trans = RealmInstance.BeginWrite())
            {
                RealmInstance.Remove(item);
                trans.Commit();
            }
        }

        /// <summary>
        /// Remove items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void RemoveRange<T>(IQueryable<T> items) where T : RealmObject, IStorageModel
        {
            using (Transaction trans = RealmInstance.BeginWrite())
            {
                RealmInstance.RemoveRange(items);
                trans.Commit();
            }
        }
    }
}
