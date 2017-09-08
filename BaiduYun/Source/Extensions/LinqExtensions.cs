using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BaiduYun.Extensions {

    public static class LinqExtensions {

        public static void Each<T>(this IEnumerable<T> list, Action<T> action) {
            foreach (var item in list)
                action(item);
        }

        public static void Each<T>(this IEnumerable<T> list, Func<T, bool> action) {
            foreach (var item in list) {
                if (!action(item))
                    break;
            }
        }

        public static void RemoveAll<T>(this ICollection<T> list, Func<T, bool> pred) {
            list.Where(item => pred(item)).ToArray().Each(item => list.Remove(item));
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> list) {
            return new ObservableCollection<T>(list);
        }
    }
}
