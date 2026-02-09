using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class made to handle multiple object pools throughout the game's life. The pools expand infinetly,
/// depending of the demand of the objects.
/// </summary>
public class MultipleObjectPool : MonoBehaviour
{
    /// <summary>
    /// Determines whether the pool automatically enables objects when dequeued
    /// and disables them when enqueued.
    /// </summary>
    public bool ShouldHandleObjectDisables { get; set; } = true;

    Dictionary<Type, Component> StoredPrefabs { get; } = new();
    Dictionary<Type, Queue<Component>> Pools { get; } = new();

    /// <summary>
    /// Creates a new object pool for the specified component type using the given prefab.
    /// Instantiates an initial number of pooled objects and stores them inactive in the pool.
    /// </summary>
    /// <typeparam name="T">
    /// The component type to pool. Must be a Component that implements IPoolable.
    /// </typeparam>
    /// <param name="prefab">
    /// The prefab used to instantiate new pooled objects.
    /// </param>
    /// <param name="startingSize">
    /// The initial number of instances created for the pool.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a pool for the given type already exists.
    /// </exception>
    public void CreateNewPool<T>(T prefab, uint startingSize)
        where T : Component, IPoolable
    {
        Type poolType = typeof(T);

        if (Pools.ContainsKey(poolType))
        {
            throw new InvalidOperationException($"Pool with Type {poolType} already exists in the pool");
        }

        var newQueue = new Queue<Component>();
        StoredPrefabs.Add(poolType, prefab);

        for (int i = 0; i < startingSize; i++)
        {
            var element = UnityEngine.Object.Instantiate(prefab);
            if (ShouldHandleObjectDisables)
                element.gameObject.SetActive(false);

            newQueue.Enqueue(element);
        }

        Pools[poolType] = newQueue;
    }

    /// <summary>
    /// Returns an object to its corresponding pool.
    /// The object is optionally deactivated and notified that it has been despawned.
    /// </summary>
    /// <typeparam name="T">
    /// The component type of the object being returned to the pool.
    /// </typeparam>
    /// <param name="item">
    /// The instance to enqueue back into the pool.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool exists for the given type.
    /// </exception>
    public void Enqueue<T>(T item)
        where T : Component, IPoolable
    {
        if (!Pools.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException($"No pool exists for type {typeof(T)}");
        }

        if (ShouldHandleObjectDisables)
        {
            item.gameObject.SetActive(false);
        }

        item.OnDespawn();
        Pools[item.GetType()].Enqueue(item);
    }

    /// <summary>
    /// Retrieves an object from the pool.
    /// If the pool is empty, a new instance is created using the stored prefab.
    /// The object is optionally activated and notified that it has been spawned.
    /// </summary>
    /// <typeparam name="T">
    /// The component type to retrieve from the pool.
    /// </typeparam>
    /// <returns>
    /// An active instance of the requested component type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool exists for the given type.
    /// </exception>
    public T Dequeue<T>()
        where T : Component, IPoolable
    {
        if (!Pools.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException($"No pool exists for type {typeof(T)}");
        }

        if (!Pools[typeof(T)].TryDequeue(out var pooled))
        {
            pooled = CreateNewElement<T>();
        }

        var item = (T)pooled;

        if (ShouldHandleObjectDisables)
        {
            item.gameObject.SetActive(true);
        }

        item.OnSpawn();
        return item;
    }

    /// <summary>
    /// Instantiates a new pooled object using the prefab associated with the given type.
    /// This method is called when a pool is empty and a new instance is required.
    /// </summary>
    /// <typeparam name="T">
    /// The component type to instantiate.
    /// </typeparam>
    /// <returns>
    /// A newly instantiated component of the requested type.
    /// </returns>
    T CreateNewElement<T>()
        where T : Component, IPoolable
    {
        var type = typeof(T);
        var prefab = StoredPrefabs[type];
        var newElement = UnityEngine.Object.Instantiate(prefab);
        return (T)newElement;
    }
}

/// <summary>
/// Defines lifecycle callbacks for objects managed by an object pool.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when the object is retrieved from the pool and becomes active.
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// Called when the object is returned to the pool and is no longer in use.
    /// </summary>
    void OnDespawn();
}
