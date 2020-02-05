using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Metrics.UnitTests
{
    class TestObserver : IObserver<DiagnosticListener>
    {
        private readonly string _eventName;
        private readonly IObserver<KeyValuePair<string, object>> _observer;

        public TestObserver(string eventName, IObserver<KeyValuePair<string, object>> observer)
        {
            _eventName = eventName;
            _observer = observer;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == _eventName)
            {
                value.Subscribe(_observer);
            }
        }
    }
}
