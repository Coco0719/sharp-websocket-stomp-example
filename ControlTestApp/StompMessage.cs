using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlTestApp
{
    class StompMessage
    {
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        public StompMessage(string command)
            : this(command, string.Empty)
        {
        }
        public StompMessage(string command, string body)
            : this(command, body, new Dictionary<string, string>())
        {
        }
        internal StompMessage(string command, string body, Dictionary<string, string> headers)
        {
            Command = command;
            Body = body;
            _headers = headers;
            this["content-length"] = body.Length.ToString();
        }
        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }
        public string Body { get; private set; }
        public string Command { get; private set; }
        public string this[string header]
        {
            get { return _headers.ContainsKey(header) ? _headers[header] : string.Empty; }
            set { _headers[header] = value; }
        }
    }
}
