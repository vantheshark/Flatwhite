//using System;
//using System.Collections.Generic;
//using System.Runtime.Caching;
//using System.Threading;

//namespace Flatwhite
//{
//    internal class TimerWrapper : IDisposable
//    {
//        public Timer Timer { get; set; }
//        public void Dispose()
//        {
//            Timer?.Dispose();
//        }
//    }

//    public class Phoenix : IDisposable
//    {
//        private readonly TimeSpan _lifeTime;
//        private readonly Func<object> _reborn;
//        public Action RebornCallBack { get; set; }

//        public Phoenix(TimeSpan lifeTime, Func<object> reborn)
//        {
//            _lifeTime = lifeTime;
//            _reborn = reborn;

//            var t = new TimerWrapper();
//            t.Timer = new Timer(_ =>
//            {
//                var method = methodExecutedContext.Invocation.Method;
//                var parameters = methodExecutedContext.Invocation.Arguments;
//                var instance = Global.ServiceActivator.CreateInstance(methodExecutedContext.MethodInfo.DeclaringType);
//                var result = method.Invoke(instance, parameters);
//                var newPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddMilliseconds(Duration) };
//                cacheStore.Set(key, result, newPolicy);


//                var w = (TimerWrapper)_;
//                _timers.Remove(w.Timer);
//                w.Dispose();

//            }, t, TimeSpan.FromMilliseconds(Duration), TimeSpan.Zero);
//            _timers.Add(t.Timer);


//            _rebornCallBack = rebornCallBack;
//        }
//        private static readonly List<Timer> _timers = new List<Timer>();

//        public Phoenix Reborn()
//        {

//            _rebornCallBack();
//        }

//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}