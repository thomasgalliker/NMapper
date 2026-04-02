using System.Collections;
using System.Collections.Generic;

namespace NMapper.Internals
{
    internal sealed class FastArrayCollectionMappingPlan<TSource, TTarget> : IFastCollectionMappingPlan
    {
        private readonly Func<TSource?, TTarget?> map;
        private readonly TypePair elementTypePair;
        private readonly Type mappingType;

        public FastArrayCollectionMappingPlan(Func<TSource?, TTarget?> map, TypePair elementTypePair, Type mappingType)
        {
            this.map = map;
            this.elementTypePair = elementTypePair;
            this.mappingType = mappingType;
        }

        public object Map(object source, MappingContext context)
        {
            if (source is TSource[] sourceArray)
            {
                var targetArray = new TTarget[sourceArray.Length];
                context.StoreMappedObject(source, targetArray);

                for (var i = 0; i < sourceArray.Length; i++)
                {
                    if (FastCollectionMappingPlan.TryMap(sourceArray[i], this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        targetArray[i] = mapped!;
                    }
                }

                return targetArray;
            }

            if (source is ICollection collection)
            {
                var targetArray = new TTarget[collection.Count];
                context.StoreMappedObject(source, targetArray);

                var index = 0;
                foreach (var item in collection)
                {
                    if (FastCollectionMappingPlan.TryMap((TSource?)item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        targetArray[index++] = mapped!;
                    }
                }

                return targetArray;
            }

            var temp = new List<TTarget>();
            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                if (FastCollectionMappingPlan.TryMap(item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                {
                    temp.Add(mapped!);
                }
            }

            return temp.ToArray();
        }
    }

    internal sealed class FastEnumerableCollectionMappingPlan<TSource, TTarget> : IFastCollectionMappingPlan
    {
        private readonly Func<TSource?, TTarget?> map;
        private readonly TypePair elementTypePair;
        private readonly Type mappingType;
        private readonly ICompiledCollectionAdapter collectionAdapter;

        public FastEnumerableCollectionMappingPlan(Func<TSource?, TTarget?> map, TypePair elementTypePair, Type mappingType, ICompiledCollectionAdapter collectionAdapter)
        {
            this.map = map;
            this.elementTypePair = elementTypePair;
            this.mappingType = mappingType;
            this.collectionAdapter = collectionAdapter;
        }

        public object Map(object source, MappingContext context)
        {
            var targetCollection = this.collectionAdapter.Create(FastCollectionMappingPlan.GetCapacity(source));
            context.StoreMappedObject(source, targetCollection);

            if (source is List<TSource> sourceList)
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    if (FastCollectionMappingPlan.TryMap(sourceList[i], this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        this.collectionAdapter.Add(targetCollection, mapped);
                    }
                }

                return targetCollection;
            }

            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                if (FastCollectionMappingPlan.TryMap(item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                {
                    this.collectionAdapter.Add(targetCollection, mapped);
                }
            }

            return targetCollection;
        }
    }

    internal sealed class FastContextArrayCollectionMappingPlan<TSource, TTarget> : IFastCollectionMappingPlan
    {
        private readonly Func<TSource?, MappingContext, TTarget?> map;
        private readonly TypePair elementTypePair;
        private readonly Type mappingType;

        public FastContextArrayCollectionMappingPlan(Func<TSource?, MappingContext, TTarget?> map, TypePair elementTypePair, Type mappingType)
        {
            this.map = map;
            this.elementTypePair = elementTypePair;
            this.mappingType = mappingType;
        }

        public object Map(object source, MappingContext context)
        {
            if (source is TSource[] sourceArray)
            {
                var targetArray = new TTarget[sourceArray.Length];
                context.StoreMappedObject(source, targetArray);

                for (var i = 0; i < sourceArray.Length; i++)
                {
                    if (FastCollectionMappingPlan.TryMap(sourceArray[i], this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        targetArray[i] = mapped!;
                    }
                }

                return targetArray;
            }

            if (source is ICollection collection)
            {
                var targetArray = new TTarget[collection.Count];
                context.StoreMappedObject(source, targetArray);

                var index = 0;
                foreach (var item in collection)
                {
                    if (FastCollectionMappingPlan.TryMap((TSource?)item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        targetArray[index++] = mapped!;
                    }
                }

                return targetArray;
            }

            var temp = new List<TTarget>();
            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                if (FastCollectionMappingPlan.TryMap(item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                {
                    temp.Add(mapped!);
                }
            }

            return temp.ToArray();
        }
    }

    internal sealed class FastContextEnumerableCollectionMappingPlan<TSource, TTarget> : IFastCollectionMappingPlan
    {
        private readonly Func<TSource?, MappingContext, TTarget?> map;
        private readonly TypePair elementTypePair;
        private readonly Type mappingType;
        private readonly ICompiledCollectionAdapter collectionAdapter;

        public FastContextEnumerableCollectionMappingPlan(Func<TSource?, MappingContext, TTarget?> map, TypePair elementTypePair, Type mappingType, ICompiledCollectionAdapter collectionAdapter)
        {
            this.map = map;
            this.elementTypePair = elementTypePair;
            this.mappingType = mappingType;
            this.collectionAdapter = collectionAdapter;
        }

        public object Map(object source, MappingContext context)
        {
            var targetCollection = this.collectionAdapter.Create(FastCollectionMappingPlan.GetCapacity(source));
            context.StoreMappedObject(source, targetCollection);

            if (source is List<TSource> sourceList)
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    if (FastCollectionMappingPlan.TryMap(sourceList[i], this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                    {
                        this.collectionAdapter.Add(targetCollection, mapped);
                    }
                }

                return targetCollection;
            }

            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                if (FastCollectionMappingPlan.TryMap(item, this.map, context, this.elementTypePair, this.mappingType, out var mapped))
                {
                    this.collectionAdapter.Add(targetCollection, mapped);
                }
            }

            return targetCollection;
        }
    }

    internal static class FastCollectionMappingPlan
    {
        internal static bool TryMap<TSource, TTarget>(
            TSource? source,
            Func<TSource?, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType,
            out TTarget? result)
        {
            try
            {
                result = map(source);
                return true;
            }
            catch (Exception ex)
            {
                context.AddException(new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex));
                result = default;
                return false;
            }
        }

        internal static bool TryMap<TSource, TTarget>(
            TSource? source,
            Func<TSource?, MappingContext, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType,
            out TTarget? result)
        {
            try
            {
                result = map(source, context);
                return true;
            }
            catch (Exception ex)
            {
                context.AddException(new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex));
                result = default;
                return false;
            }
        }

        internal static IEnumerable<TSource?> Enumerate<TSource>(object source)
        {
            if (source is IEnumerable<TSource> typedEnumerable)
            {
                foreach (var item in typedEnumerable)
                {
                    yield return item;
                }

                yield break;
            }

            foreach (var item in (IEnumerable)source)
            {
                yield return (TSource?)item;
            }
        }

        internal static int? GetCapacity(object source)
        {
            if (source is ICollection collection)
            {
                return collection.Count;
            }

            return null;
        }
    }
}
