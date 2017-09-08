﻿namespace BaiduYun.Misc {

    public sealed class Singleton<T> where T : new() {

        private static object locker = new object();
        private static T instance;

        private Singleton() { }

        public static T Instance {
            get {
                if(instance == null) {
                    lock (locker) {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
            set { instance = value; }
        }
    }
}
