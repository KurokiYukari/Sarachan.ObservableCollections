using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Sarachan.ObservableCollections
{
    public interface INotifyCollectionChanged<T>
    {
        event NotifyCollectionChangedEventHandler<T> CollectionChanged;
    }

    public delegate void NotifyCollectionChangedEventHandler<T>(object sender, NotifyCollectionChangedEventArgs<T> e);

    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct NotifyCollectionChangedEventArgs<T>
    {
        public NotifyCollectionChangedAction Action { get; }
        public ReadOnlySpan<T> NewItems { get; }
        public ReadOnlySpan<T> OldItems { get; }
        public int NewStartingIndex { get; }
        public int OldStartingIndex { get; }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, ReadOnlySpan<T> newItems = default, ReadOnlySpan<T> oldItems = default, int newStartingIndex = -1, int oldStartingIndex = -1)
        {
            Action = action;
            NewItems = newItems;
            OldItems = oldItems;
            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
        }

        private readonly static NotifyCollectionChangedEventArgs _resetArgs = new(NotifyCollectionChangedAction.Reset);

        public void RelayStandardEventArgs(bool supportRangeAction, object? sender, NotifyCollectionChangedEventHandler handler)
        {
            switch (Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (supportRangeAction)
                        {
                            handler(sender, new NotifyCollectionChangedEventArgs(Action, NewItems.ToArray(), NewStartingIndex));
                        }
                        else
                        {
                            for (int i = 0; i < NewItems.Length; i++)
                            {
                                handler(sender, new NotifyCollectionChangedEventArgs(Action, NewItems[i], NewStartingIndex + i));
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (supportRangeAction)
                        {
                            handler(sender, new NotifyCollectionChangedEventArgs(Action, OldItems.ToArray(), OldStartingIndex));
                        }
                        else
                        {
                            for (int i = OldItems.Length - 1; i >= 0; i--)
                            {
                                handler(sender, new NotifyCollectionChangedEventArgs(Action, OldItems[i], OldStartingIndex + i));
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        Debug.Assert(NewItems.Length == 1);
                        Debug.Assert(OldItems.Length == 1);
                        handler(sender, new NotifyCollectionChangedEventArgs(Action, NewItems.ToArray(), OldItems.ToArray(), NewStartingIndex));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        Debug.Assert(NewItems.Length == 1);
                        handler(sender, new NotifyCollectionChangedEventArgs(Action, NewItems[0], NewStartingIndex, OldStartingIndex));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        handler(sender, _resetArgs);
                        break;
                    }
                default:
                    break;
            }

            //if (!supportRangeAction)
            //{
            //    if (NewItems.Length > 1 || OldItems.Length > 1)
            //    {
            //        return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            //    }
            //}

            //return Action switch
            //{
            //    NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(Action, NewItems.ToArray(), NewStartingIndex),
            //    NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(Action, OldItems.ToArray(), OldStartingIndex),
            //    NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(Action, NewItems.ToArray(), OldItems.ToArray(), NewStartingIndex),
            //    NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(Action, OldItems[0], NewStartingIndex, OldStartingIndex),
            //    NotifyCollectionChangedAction.Reset => _resetArgs,
            //    _ => throw new NotSupportedException(),
            //};
        }

        public static NotifyCollectionChangedEventArgs<T> Add(in T newItem, int newStartingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, newItems: new(in newItem), newStartingIndex: newStartingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Add(ReadOnlySpan<T> newItems, int newStartingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, newItems: newItems, newStartingIndex: newStartingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Remove(in T oldItem, int oldStartingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, oldItems: new(in oldItem), oldStartingIndex: oldStartingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Remove(ReadOnlySpan<T> oldItems, int oldStartingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, oldItems: oldItems, oldStartingIndex: oldStartingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Replace(in T newItem, in T oldItem, int startingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Replace, new(in newItem), new(in oldItem), startingIndex, startingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Move(in T changedItem, int newStartingIndex, int oldStartingIndex)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Move, new(in changedItem), default, newStartingIndex, oldStartingIndex);
        }

        public static NotifyCollectionChangedEventArgs<T> Reset(ReadOnlySpan<T> newItems, ReadOnlySpan<T> oldItems)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset, newItems, oldItems);
        }
    }
}
