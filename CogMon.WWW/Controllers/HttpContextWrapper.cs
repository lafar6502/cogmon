﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NGinnBPM.MessageBus.Impl.HttpService;
using System.IO;
using System.Web;
using System.Text;
using System.Collections.Specialized;

namespace CogMon.WWW.Controllers
{
    public class HttpContextWrapper : IRequestContext
    {
        private HttpContextBase _ctx;
        private TextReader _input;
        

        public HttpContextWrapper(HttpContextBase ctx)
        {
            _ctx = ctx;
            _input = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
            DispatchUrl = _ctx.Request.RawUrl;
        }

        public void AddResponseHeader(string name, string value)
        {
            _ctx.Response.AddHeader(name, value);
        }

        public void ClearResponse()
        {
            _ctx.Response.Clear();
        }

        public string HttpMethod
        {
            get { return _ctx.Request.HttpMethod; }
        }

        public TextReader Input
        {
            get { return _input;  }
        }

        public Stream InputStream
        {
            get { return _ctx.Request.InputStream; }
        }

        public TextWriter Output
        {
            get { return _ctx.Response.Output; }
        }

        public Stream OutputStream
        {
            get { return  _ctx.Response.OutputStream; }
        }

        public NameValueCollection QueryString
        {
            get { return _ctx.Request.QueryString; }
        }

        public string RawUrl
        {
            get { return _ctx.Request.RawUrl; }
        }

        public int RequestContentLength
        {
            get { return _ctx.Request.ContentLength; }
        }

        public string RequestContentType
        {
            get { return _ctx.Request.ContentType; }
        }

        public Encoding RequestEncoding
        {
            get { return _ctx.Request.ContentEncoding; }
        }

        public int ResponseContentLength
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ResponseContentType
        {
            get
            {
                return _ctx.Response.ContentType;
            }
            set
            {
                _ctx.Response.ContentType = value;
            }
        }

        public Encoding ResponseEncoding
        {
            get
            {
                return _ctx.Response.ContentEncoding;
            }
            set
            {
                _ctx.Response.ContentEncoding = value;
            }
        }

        public int ResponseStatus
        {
            get
            {
                return _ctx.Response.StatusCode;
            }
            set
            {
                _ctx.Response.StatusCode = value;
            }
        }

        public Uri Url
        {
            get { return _ctx.Request.Url; }
        }

        public IDictionary<string, string> UrlVariables { get;set;}
        public System.Security.Principal.IPrincipal User
        {
            get { return _ctx.User; }
        }

        public string DispatchUrl { get;set;}


        public NameValueCollection Headers
        {
            get { return _ctx.Request.Headers; }
        }
    }
}