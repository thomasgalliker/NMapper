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

        public object Map(object source, Type requestedTargetType, MappingContext context)
        {
            if (source is TSource[] sourceArray)
            {
                var targetArray = new TTarget[sourceArray.Length];
                context.StoreMappedObject(source, requestedTargetType, targetArray);

                for (var i = 0; i < sourceArray.Length; i++)
                {
                    targetArray[i] = FastCollectionMappingPlan.MapItem(sourceArray[i], this.map, context, this.elementTypePair, this.mappingType)!;
                }

                return targetArray;
            }

            if (source is ICollection collection)
            {
                var targetArray = new TTarget[collection.Count];
                context.StoreMappedObject(source, requestedTargetType, targetArray);

                var index = 0;
                foreach (var item in collection)
                {
                    targetArray[index++] = FastCollectionMappingPlan.MapItem((TSource?)item, this.map, context, this.elementTypePair, this.mappingType)!;
                }

                return targetArray;
            }

            return FastCollectionMappingPlan.MapUnsizedToArray(source, requestedTargetType, context, this.map, this.elementTypePair, this.mappingType);
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

        public object Map(object source, Type requestedTargetType, MappingContext context)
        {
            var targetCollection = this.collectionAdapter.Create(FastCollectionMappingPlan.GetCapacity(source));
            context.StoreMappedObject(source, requestedTargetType, targetCollection);

            if (source is List<TSource> sourceList)
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    this.collectionAdapter.Add(targetCollection, FastCollectionMappingPlan.MapItem(sourceList[i], this.map, context, this.elementTypePair, this.mappingType));
                }

                return targetCollection;
            }

            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                this.collectionAdapter.Add(targetCollection, FastCollectionMappingPlan.MapItem(item, this.map, context, this.elementTypePair, this.mappingType));
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

        public object Map(object source, Type requestedTargetType, MappingContext context)
        {
            if (source is TSource[] sourceArray)
            {
                var targetArray = new TTarget[sourceArray.Length];
                context.StoreMappedObject(source, requestedTargetType, targetArray);

                for (var i = 0; i < sourceArray.Length; i++)
                {
                    targetArray[i] = FastCollectionMappingPlan.MapItem(sourceArray[i], this.map, context, this.elementTypePair, this.mappingType)!;
                }

                return targetArray;
            }

            if (source is ICollection collection)
            {
                var targetArray = new TTarget[collection.Count];
                context.StoreMappedObject(source, requestedTargetType, targetArray);

                var index = 0;
                foreach (var item in collection)
                {
                    targetArray[index++] = FastCollectionMappingPlan.MapItem((TSource?)item, this.map, context, this.elementTypePair, this.mappingType)!;
                }

                return targetArray;
            }

            return FastCollectionMappingPlan.MapUnsizedToArray(source, requestedTargetType, context, this.map, this.elementTypePair, this.mappingType);
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

        public object Map(object source, Type requestedTargetType, MappingContext context)
        {
            var targetCollection = this.collectionAdapter.Create(FastCollectionMappingPlan.GetCapacity(source));
            context.StoreMappedObject(source, requestedTargetType, targetCollection);

            if (source is List<TSource> sourceList)
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    this.collectionAdapter.Add(targetCollection, FastCollectionMappingPlan.MapItem(sourceList[i], this.map, context, this.elementTypePair, this.mappingType));
                }

                return targetCollection;
            }

            foreach (var item in FastCollectionMappingPlan.Enumerate<TSource>(source))
            {
                this.collectionAdapter.Add(targetCollection, FastCollectionMappingPlan.MapItem(item, this.map, context, this.elementTypePair, this.mappingType));
            }

            return targetCollection;
        }
    }

    internal static class FastCollectionMappingPlan
    {
        /// <summary>
        /// Maps a source sequence whose length is not known up front into an array.
        /// The source is buffered first so that the target array can be registered with the
        /// mapping context <i>before</i> its elements are mapped - without that, a circular
        /// reference running through this collection would never find a cache entry and would
        /// recurse until the stack overflows.
        /// </summary>
        internal static object MapUnsizedToArray<TSource, TTarget>(
            object source,
            Type requestedTargetType,
            MappingContext context,
            Func<TSource?, TTarget?> map,
            TypePair elementTypePair,
            Type mappingType)
        {
            var buffer = new List<TSource?>();
            foreach (var item in Enumerate<TSource>(source))
            {
                buffer.Add(item);
            }

            var targetArray = new TTarget[buffer.Count];
            context.StoreMappedObject(source, requestedTargetType, targetArray);

            for (var i = 0; i < buffer.Count; i++)
            {
                targetArray[i] = MapItem(buffer[i], map, context, elementTypePair, mappingType)!;
            }

            return targetArray;
        }

        /// <inheritdoc cref="MapUnsizedToArray{TSource,TTarget}(object,Type,MappingContext,Func{TSource,TTarget},TypePair,Type)"/>
        internal static object MapUnsizedToArray<TSource, TTarget>(
            object source,
            Type requestedTargetType,
            MappingContext context,
            Func<TSource?, MappingContext, TTarget?> map,
            TypePair elementTypePair,
            Type mappingType)
        {
            var buffer = new List<TSource?>();
            foreach (var item in Enumerate<TSource>(source))
            {
                buffer.Add(item);
            }

            var targetArray = new TTarget[buffer.Count];
            context.StoreMappedObject(source, requestedTargetType, targetArray);

            for (var i = 0; i < buffer.Count; i++)
            {
                targetArray[i] = MapItem(buffer[i], map, context, elementTypePair, mappingType)!;
            }

            return targetArray;
        }

        internal static TTarget? MapItem<TSource, TTarget>(
            TSource? source,
            Func<TSource?, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType)
        {
            if (!context.IsUntracked && !typeof(TSource).IsValueType)
            {
                return MapItemTracked(source, map, context, elementTypePair, mappingType);
            }

            try
            {
                return map(source);
            }
            catch (Exception ex)
            {
                if (ex is MappingException or MissingMappingException)
                {
                    throw;
                }

                throw new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex);
            }
        }

        internal static TTarget? MapItem<TSource, TTarget>(
            TSource? source,
            Func<TSource?, MappingContext, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType)
        {
            if (!context.IsUntracked && !typeof(TSource).IsValueType)
            {
                return MapItemTracked(source, map, context, elementTypePair, mappingType);
            }

            try
            {
                return map(source, context);
            }
            catch (Exception ex)
            {
                if (ex is MappingException or MissingMappingException)
                {
                    throw;
                }

                throw new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex);
            }
        }

        /// <summary>
        /// Maps a single element while participating in reference tracking and depth limiting.
        /// Used whenever <see cref="MappingContext.IsUntracked"/> is <c>false</c> and the element
        /// type is a reference type; the untracked path calls the compiled delegate directly and
        /// skips this bookkeeping entirely.
        /// </summary>
        private static TTarget? MapItemTracked<TSource, TTarget>(
            TSource? source,
            Func<TSource?, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType)
        {
            if (context.TryGetMappedObject(source, typeof(TTarget), out var cached))
            {
                return (TTarget?)cached;
            }

            if (!context.TryEnter(source))
            {
                if (context.ThrowIfMaxDepthExceeded)
                {
                    throw new MappingException(
                        elementTypePair.SourceType,
                        elementTypePair.TargetType,
                        mappingType,
                        new InvalidOperationException($"Maximum recursion depth exceeded (MaxDepth: {context.MaxDepth})."));
                }

                return default;
            }

            try
            {
                var result = map(source);
                if (result is not null)
                {
                    context.StoreMappedObject(source, typeof(TTarget), result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex is MappingException or MissingMappingException)
                {
                    throw;
                }

                throw new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex);
            }
            finally
            {
                context.Exit(source);
            }
        }

        /// <inheritdoc cref="MapItemTracked{TSource,TTarget}(TSource,Func{TSource,TTarget},MappingContext,TypePair,Type)"/>
        private static TTarget? MapItemTracked<TSource, TTarget>(
            TSource? source,
            Func<TSource?, MappingContext, TTarget?> map,
            MappingContext context,
            TypePair elementTypePair,
            Type mappingType)
        {
            if (context.TryGetMappedObject(source, typeof(TTarget), out var cached))
            {
                return (TTarget?)cached;
            }

            if (!context.TryEnter(source))
            {
                if (context.ThrowIfMaxDepthExceeded)
                {
                    throw new MappingException(
                        elementTypePair.SourceType,
                        elementTypePair.TargetType,
                        mappingType,
                        new InvalidOperationException($"Maximum recursion depth exceeded (MaxDepth: {context.MaxDepth})."));
                }

                return default;
            }

            try
            {
                var result = map(source, context);
                if (result is not null)
                {
                    context.StoreMappedObject(source, typeof(TTarget), result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex is MappingException or MissingMappingException)
                {
                    throw;
                }

                throw new MappingException(elementTypePair.SourceType, elementTypePair.TargetType, mappingType, ex);
            }
            finally
            {
                context.Exit(source);
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
