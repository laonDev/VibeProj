using System.Collections.Generic;
using UnityEngine;

namespace AnimalKitchen
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();

        public ObjectPool(T prefab, Transform parent, int initialSize = 10)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        private T CreateNew()
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
            return instance;
        }

        public T Get()
        {
            T instance = pool.Count > 0 ? pool.Dequeue() : CreateNew();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Return(T instance)
        {
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }

        public void Clear()
        {
            while (pool.Count > 0)
            {
                T instance = pool.Dequeue();
                if (instance != null)
                {
                    Object.Destroy(instance.gameObject);
                }
            }
        }
    }

    public class GameObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform parent;
        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        public GameObjectPool(GameObject prefab, Transform parent, int initialSize = 10)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        private GameObject CreateNew()
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            pool.Enqueue(instance);
            return instance;
        }

        public GameObject Get()
        {
            GameObject instance = pool.Count > 0 ? pool.Dequeue() : CreateNew();
            instance.SetActive(true);
            return instance;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject instance = Get();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }

        public void Return(GameObject instance)
        {
            instance.SetActive(false);
            pool.Enqueue(instance);
        }

        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }
        }
    }
}
