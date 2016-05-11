using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace Swartz.WarmupStarter
{
    public class WarmupHttpModule : IHttpModule
    {
        private static readonly object SynLock = new object();
        private static IList<Action> _awaiting = new List<Action>();
        private HttpApplication _context;

        public void Init(HttpApplication context)
        {
            _context = context;
            context.AddOnBeginRequestAsync(BeginBeginRequest, EndBeginRequest, null);
        }

        public void Dispose()
        {
        }

        private static bool InWarmup()
        {
            lock (SynLock)
            {
                return _awaiting != null;
            }
        }

        /// <summary>
        ///     Warmup code is about to start: Any new incoming request is queued
        ///     until "SignalWarmupDone" is called.
        /// </summary>
        public static void SignalWarmupStart()
        {
            lock (SynLock)
            {
                if (_awaiting == null)
                {
                    _awaiting = new List<Action>();
                }
            }
        }

        /// <summary>
        ///     Warmup code just completed: All pending requests in the "_await" queue are processed,
        ///     and any new incoming request is now processed immediately.
        /// </summary>
        public static void SignalWarmupDone()
        {
            IList<Action> temp;

            lock (SynLock)
            {
                temp = _awaiting;
                _awaiting = null;
            }

            if (temp != null)
            {
                foreach (var action in temp)
                {
                    action();
                }
            }
        }

        /// <summary>
        ///     Enqueue or directly process action depending on current mode.
        /// </summary>
        private void Await(Action action)
        {
            var temp = action;

            lock (SynLock)
            {
                if (_awaiting != null)
                {
                    temp = null;
                    _awaiting.Add(action);
                }
            }

            temp?.Invoke();
        }

        private IAsyncResult BeginBeginRequest(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            // host is available, process every requests, or file is processed
            if (!InWarmup() || WarmupUtility.DoBeginRequest(_context))
            {
                var asyncResult = new DoneAsyncResult(extradata);
                cb(asyncResult);
                return asyncResult;
            }
            else
            {
                // this is the "on hold" execution path
                var asyncResult = new WarmupAsyncResult(cb, extradata);
                Await(asyncResult.Completed);
                return asyncResult;
            }
        }

        private static void EndBeginRequest(IAsyncResult ar)
        {
        }

        private class WarmupAsyncResult : IAsyncResult
        {
            private readonly object _asyncState;
            private readonly AsyncCallback _cb;
            private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
            private bool _isCompleted;

            public WarmupAsyncResult(AsyncCallback cb, object asyncState)
            {
                _cb = cb;
                _asyncState = asyncState;
                _isCompleted = false;
            }

            object IAsyncResult.AsyncState => _asyncState;

            WaitHandle IAsyncResult.AsyncWaitHandle => _eventWaitHandle;

            bool IAsyncResult.CompletedSynchronously => false;

            bool IAsyncResult.IsCompleted => _isCompleted;

            public void Completed()
            {
                _isCompleted = true;
                _eventWaitHandle.Set();
                _cb(this);
            }
        }

        /// <summary>
        ///     Async result for "ok to process now" requests
        /// </summary>
        private class DoneAsyncResult : IAsyncResult
        {
            private static readonly WaitHandle WaitHandle = new ManualResetEvent(true);
            private readonly object _asyncState;

            public DoneAsyncResult(object asyncState)
            {
                _asyncState = asyncState;
            }

            bool IAsyncResult.IsCompleted => true;

            WaitHandle IAsyncResult.AsyncWaitHandle => WaitHandle;

            object IAsyncResult.AsyncState => _asyncState;

            bool IAsyncResult.CompletedSynchronously => true;
        }
    }
}