﻿using System;
using System.Collections.Generic;

namespace HttpTestWin.ViewModel
{
    public class ClientSpan
    {
        public ClientSpan()
        {
            Bags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string TracerId { get; set; }
        public string TraceId { get; set; }
        public string SpanId { get; set; }
        public string ParentSpanId { get; set; }
        public string OpName { get; set; }
        
        //for extensions
        public IDictionary<string, string> Bags { get; set; }
        
        public static ClientSpan Create(string tracerId, string traceId, string parentSpanId, string spanId, string opName, IDictionary<string, string> bags = null)
        {
            var clientSpan = new ClientSpan();
            if (bags != null)
            {
                clientSpan.Bags = bags;
            }

            //todo validate
            clientSpan.TracerId = tracerId;
            clientSpan.TraceId = traceId;
            clientSpan.SpanId = spanId;
            clientSpan.ParentSpanId = parentSpanId;
            clientSpan.OpName = opName;

            return clientSpan;
        }
    }

    public static class HttpTestConfigExtensions
    {
        public static string GetStartSpanApiUri(this HttpTestConfig config)
        {
            return GetRequestUri(config, "StartSpan");
        }

        public static string GetFinishSpanApiUri(this HttpTestConfig config)
        {
            return GetRequestUri(config, "FinishSpan");
        }

        private static string _fixedTraceApiEndPoint = null;
        public static string GetRequestUri(HttpTestConfig config, string method)
        {
            if (_fixedTraceApiEndPoint == null)
            {
                _fixedTraceApiEndPoint = config.TraceApiEndPoint;
                _fixedTraceApiEndPoint = _fixedTraceApiEndPoint.TrimEnd('/') + "/";
            }

            return _fixedTraceApiEndPoint + method;
        }
    }
}
