//using UnityEngine;
//using System.Collections.Generic;
//using System;
//using System.Timers;
//using System.Linq;

//// ReSharper disable once CheckNamespace
//namespace AkilliMum
//{
//    public class ExecuteOnMainThread : MonoBehaviour
//    {

//        private List<ThreadAction> _actions = new List<ThreadAction>();

//        public static object Lock = new object();

//        protected virtual void Awake()
//        {
//        }

//        protected virtual void Start()
//        {
//        }

//        public void Execute(Action action, string groupName = "", float delay = 0f, bool stopOnTimeScale = true, Int64 repeat = 1)
//        {
//            //Debug.Log("Added item to thread with delay: " + delay);
//            lock (Lock)
//            {
//                var neww = new ThreadAction
//                {
//                    Action = action,
//                    IsReady = false,
//                    StopOnTimeScale = stopOnTimeScale,
//                    GroupName = groupName,
//                    Repeat = repeat
//                };

//                if (delay <= 0f)
//                {
//                    neww.IsReady = true;
//                }
//                else
//                {
//                    neww.Start(delay);
//                }

//                _actions.Add(neww);
//            }
//        }

//        public void CancelExecute(string groupName)
//        {
//            lock (Lock)
//            {
//                var copy = _actions.ToList();
//                for (int i = 0; i < copy.Count; i++)
//                {
//                    if (copy[i].GroupName == groupName)
//                    {
//                        _actions.Remove(copy[i]);
//                        //Debug.Log("removed action: " + copy [i].GroupName);
//                    }
//                }
//            }
//        }

//        protected virtual void FixedUpdate()
//        {
//        }

//        protected virtual void Update()
//        {
//            lock (Lock)
//            {
//                var copy = _actions.ToList();
//                // ReSharper disable once CompareOfFloatsByEqualityOperator
//                if (Time.timeScale == 1f)
//                {
//                    for (int i = 0; i < copy.Count; i++)
//                    {
//                        copy[i].Continue();
//                    }
//                }
//                else
//                {
//                    for (int i = 0; i < copy.Count; i++)
//                    {
//                        if (copy[i].StopOnTimeScale)
//                            copy[i].Pause();
//                    }
//                }

//                for (int i = 0; i < copy.Count; i++)
//                {
//                    if (copy[i].IsReady)
//                    {
//                        lock (Lock)
//                        {
//                            copy[i].IsReady = false;
//                        }
//                        copy[i].Action.Invoke();
//                        copy[i].Repeat--;
//                        if (copy[i].Repeat <= 0)
//                            _actions.Remove(copy[i]);
//                    }
//                }
//            }
//        }
//    }

//    internal class ThreadAction
//    {
//        public Action Action
//        {
//            get;
//            set;
//        }
//        public bool IsReady
//        {
//            get;
//            set;
//        }
//        public bool StopOnTimeScale
//        {
//            get;
//            set;
//        }
//        public string GroupName
//        {
//            get;
//            set;
//        }

//        public Int64 Repeat
//        {
//            get;
//            set;
//        }
//        private Timer _timer;

//        public void Start(float delay)
//        {
//            _timer = new Timer(delay);
//            _timer.Elapsed += (o, e) =>
//            {
//                _timer.Stop();
//                lock (ExecuteOnMainThread.Lock)
//                {
//                    IsReady = true;
//                }
//            };
//            _timer.Start();
//        }
//        public void Pause()
//        {
//            if (_timer != null && _timer.Enabled)
//            {
//                _timer.Enabled = false;
//            }
//        }
//        public void Continue()
//        {
//            if (_timer != null && !_timer.Enabled)
//            {
//                _timer.Enabled = true;
//            }
//        }
//    }
//}