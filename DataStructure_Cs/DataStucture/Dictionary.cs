﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    Dictionary.cs
 Author:      Zeng Zhiwei
 Time:        2020\11\12 星期四 22:37:09
=====================================================
*/
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyNamespace
{
    public class Dictionary<TKey, TValue>: IEnumerable
    {
        private struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public TKey key;        // Key of entry
            public TValue value;    // Value of entry
        }

        private int[] buckets; //用来进行Hash碰撞
        private Entry[] entries;//用来存储字典的内容，并且标识下一个元素的位置。
        private int count;
        private int version;// 当前版本，防止迭代过程中集合被更改
        private int freeList;// 被删除Entry在entries中的下标index，这个位置是空闲的
        private int freeCount;// 有多少个被删除的Entry，有多少个空闲的位置
        //private KeyCollection keys;
        //private ValueCollection values;
        private IEqualityComparer<TKey> comparer;// 比较器

        public Dictionary() : this(0) { }

        public Dictionary(int capacity)
        {
            if(capacity < 0) throw new ArgumentOutOfRangeException();
            if(capacity > 0) Initialize(capacity);

            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        #region private
        private void Initialize(int capacity)
        {
            //大于字典容量的一个最小的质数
            int size = HashHelpers.GetPrime(capacity);
            buckets = new int[size];
            for(int i = 0; i < buckets.Length; i++)
                buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            //key不能为空，value可以为空
            if(key == null)
                throw new ArgumentNullException();
            if(buckets == null) 
                Initialize(0);

            int collisionCount = 0;

            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            //将HashCode的返回值转化为数组索引
            int targetBucket = hashCode % buckets.Length;
            // 处理hash碰撞冲突
            // 如果转换出的bucketIndex大于等于0，判断buckets数组中有没有相等的，如果相等，需要处理冲突
            for(int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
            {
                //如果转换的hash值与之前已经添加的hash值相等，同时插入的key与之前的相同，处理冲突，key是唯一的，不能重复
                if(entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if(add)
                        throw new ArgumentException("Argument_AddingDuplicate");

                    entries[i].value = value;
                    version++;
                    return;
                }
            }
            //数组索引
            int index;
            //如果空链表的长度大于0，FreeList链表不为空，
            //因此字典会优先把新增元素添加到FreeList链表所指向的位置，添加后FreeCount减1
            if(freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else
            {
                //如果数组已满，需扩容
                if(count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }


            entries[index].hashCode = hashCode;
            //新增元素的next指向上一个元素的索引
            entries[index].next = buckets[targetBucket];
            entries[index].key = key;
            entries[index].value = value;
            //记录新增元素的索引
            buckets[targetBucket] = index;
            version++;

            //如果碰撞次数（单链表长度）大于设置的最大碰撞阈值，需要扩容
            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(comparer))
            {
                //comparer = (IEqualityComparer<TKey>)HashHelpers.GetRandomizedEqualityComparer(comparer);
                Resize(entries.Length, true);
            }
        }


        /// <summary>
        /// 扩容数组
        /// </summary>
        private void Resize()
        {
            Resize(HashHelpers.ExpandPrime(count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            Debug.Assert(newSize >= entries.Length);
            int[] newBuckets = new int[newSize];
            for(int i = 0; i < newBuckets.Length; i++) 
                newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            //拷贝数组到新的地址
            Array.Copy(entries, 0, newEntries, 0, count);

            if(forceNewHashCodes)
            {
                for(int i = 0; i < count; i++)
                {
                    if(newEntries[i].hashCode != -1)
                    {
                        newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
                    }
                }
            }

            // 重新组织entry和bucket的关系
            for(int i = 0; i < count; i++)
            {
                if(newEntries[i].hashCode >= 0)
                {
                    int bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;
        }

        /// <summary>
        /// 找到指定key在entries数组中的下标
        /// </summary>
        private int FindEntry(TKey key)
        {
            if(key == null)
                throw new ArgumentNullException();

            if(buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for(int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                {
                    if(entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) 
                        return i;
                }
            }
            return -1;
        }

        #endregion

        #region public

        public TValue this[TKey key] { 
            get 
            {
                int index = FindEntry(key);
                if(index!=-1)
                {
                    return entries[index].value;
                }
                throw new KeyNotFoundException();
            }
            set 
            {
                Insert(key, value, false);
            } 
        }
        //public ValueCollection Values { get; }
        //public KeyCollection Keys { get; }
        public int Count { get { return count; } }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        //public void Clear();
        //public bool ContainsKey(TKey key);
        //public bool ContainsValue(TValue value);
        //public bool Remove(TKey key);
        public bool TryGetValue(TKey key, out TValue value) 
        {
            int index = FindEntry(key);
            if(index != -1)
            {
                value = entries[index].value;
                return true;
            }
            value = default(TValue);
            return false;
        }
        #endregion

        #region Enumerator

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        //IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        //{
        //    return new Enumerator(this, Enumerator.KeyValuePair);
        //}

        public struct Enumerator: IEnumerator<KeyValuePair<TKey, TValue>>
        //, IDictionaryEnumerator
        {
            private Dictionary<TKey, TValue> dictionary;
            private int index;
            private int version;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;
            internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                index = 0;
                version = dictionary.version;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<TKey, TValue>();
            }

            public void Dispose(){}

            public bool MoveNext()
            {
                if(version != dictionary.version)
                    throw new Exception("version not matching!");
                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while((uint)index < (uint)dictionary.count)
                {
                    if(dictionary.entries[index].hashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(dictionary.entries[index].key, dictionary.entries[index].value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = dictionary.count + 1;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return current; }
            }

            object IEnumerator.Current
            {
                get
                {
                    return current;
                }
            }

            public void Reset()
            {
                if(version != dictionary.version)
                    throw new Exception("version not matching!");
                index = 0;
                current = new KeyValuePair<TKey, TValue>();
            }
        }

        #endregion

        // 测试代码
        public static void main()
        {
            Dictionary<int,int> testList = new Dictionary<int, int>();
            testList.Add(1, 1);
            testList.Add(2, 2);
            testList.Add(3, 3);
            testList.Add(4, 4);
            foreach(var item in testList)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }
    }
}