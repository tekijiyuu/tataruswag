﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices;

namespace HttpUtilities
{
    public class HttpReader
    {
        #region Properties
        public string Host { get => _GlobalHost; }

        public string UserAgent { get; set; } = "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
        public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";

        public bool ThrowExceptions { get => _ThrowExceptions; set => _ThrowExceptions = value; }

        public IDictionary<string, string> OptionalHeaders { get => _OptionalHeaders; set => _OptionalHeaders = value; }

        #endregion

        #region Protected fileds

        protected string _GlobalHost;

        protected CookieContainer _GlobalCookie;

        protected string _AuthorizationString = null;

        protected ILog _Logger;

        protected bool _ThrowExceptions = false;

        protected IDictionary<string, string> _OptionalHeaders;

        #endregion

        public HttpReader(CookieContainer cookieIn, ILog logger = null)
        {
            _Logger = logger;
            Init(null, cookieIn, null);
        }

        public HttpReader(ILog logger = null)
        {
            _Logger = logger;
            Init(null, null, null);
        }

        public HttpReader(string host, ILog logger = null)
        {
            _Logger = logger;
            Init(host, null, null);
        }

        public HttpReader(string host, string authorizationString, ILog logger = null)
        {
            _Logger = logger;
            Init(host, null, authorizationString);
        }

        public HttpReader(string host, CookieContainer cookieIn, string authorizationString = null, ILog logger = null)
        {
            _Logger = logger;
            Init(host, cookieIn, authorizationString);
        }

        public HttpReader(HttpReader reader)
        {
            this._Logger = reader._Logger;
            Init(reader._GlobalHost, reader._GlobalCookie, reader._AuthorizationString);
        }

        private void Init(string host, CookieContainer cookieIn, string authorizationString)
        {
            _GlobalHost = host;

            if (cookieIn != null)
                _GlobalCookie = cookieIn;
            else
                _GlobalCookie = new CookieContainer();

            _AuthorizationString = authorizationString;

            _OptionalHeaders = new Dictionary<string, string>();
        }

        #region AsyncMethods

        public virtual async Task<HttpResponse> RequestWebDataAsync(string url, HttpMethods method, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();

            await Task.Run(() => result = RequestWebData(url, method, acceptCookie));

            if (_ThrowExceptions)
                if (result.InnerException != null)
                    throw result.InnerException;

            return result;
        }

        public virtual async Task<HttpResponse> RequestWebDataAsync(string url, HttpMethods method, string dataIn, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();

            await Task.Run(() => result = RequestWebData(url, method, dataIn, acceptCookie));

            if (_ThrowExceptions)
                if (result.InnerException != null)
                    throw result.InnerException;

            return result;
        }

        #endregion

        #region SyncMethods

        public virtual HttpResponse RequestWebData(string url, HttpMethods method, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();

            try
            {
                WebRequest localRequest = PrepRequest(url, method, _GlobalCookie, _AuthorizationString);

                string content = string.Empty;

                content = ReadWebData(localRequest, null, acceptCookie);

                result = new HttpResponse(true, content);
            }
            catch (Exception e)
            {
                _Logger?.WriteLog(e?.ToString() ?? "Exception is null");

                if (_ThrowExceptions)
                    throw e;

                result = new HttpResponse(false, null, e);
            }

            return result;
        }

        public virtual HttpResponse RequestWebData(string url, HttpMethods method, string dataIn, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();

            try
            {
                WebRequest localRequest = PrepRequest(url, method, _GlobalCookie, _AuthorizationString);

                string content = string.Empty;

                content = ReadWebData(localRequest, dataIn, acceptCookie);

                result = new HttpResponse(true, content);
            }
            catch (Exception e)
            {
                _Logger?.WriteLog(e?.ToString() ?? "Exception is null");

                if (_ThrowExceptions)
                    throw e;

                result = new HttpResponse(false, null, e);
            }

            return result;
        }

        #endregion

        #region ProtecteMethods

        protected virtual WebRequest PrepRequest(string url, HttpMethods method, CookieContainer cookie = null, string authorizationString = null)
        {
            var uri = new Uri(url);
            WebRequest localRequest = WebRequest.Create(uri);
            localRequest.Method = method.ToString();

            if (UserAgent != null)
                ((HttpWebRequest)localRequest).UserAgent = UserAgent;
            if (Accept != null)
                ((HttpWebRequest)localRequest).Accept = Accept;

            if (_GlobalHost != null)
                ((HttpWebRequest)localRequest).Host = _GlobalHost;
            else
                ((HttpWebRequest)localRequest).Host = uri.Host;

            if (ContentType != null)
                localRequest.ContentType = ContentType;

            if (cookie != null)
                ((HttpWebRequest)localRequest).CookieContainer = cookie;

            if (authorizationString != null)
                ((HttpWebRequest)localRequest).Headers.Add("Authorization", authorizationString);

            if (_OptionalHeaders != null)
            {
                foreach (KeyValuePair<string, string> headerPair in _OptionalHeaders)
                {
                    if (headerPair.Key != null && headerPair.Value != null)
                        ((HttpWebRequest)localRequest).Headers.Add(headerPair.Key, headerPair.Value);
                }
            }

            return localRequest;
        }

        protected virtual string ReadWebData(WebRequest request, string dataIn = null, bool acceptCookie = false)
        {
            string content = string.Empty;

            if (dataIn != null)
            {
                var dataBytes = Encoding.UTF8.GetBytes(dataIn);

                request.ContentLength = dataBytes.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(dataBytes, 0, dataBytes.Length);
                    dataStream.Close();
                }
            }

            using (WebResponse localResponse = request.GetResponse())
            {
                using (Stream ReceiveStream = localResponse.GetResponseStream())
                {
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                    using (StreamReader readStream = new StreamReader(ReceiveStream, encode))
                    {
                        content = readStream.ReadToEnd();
                        readStream.Close();
                    }

                    if (acceptCookie)
                    {
                        try
                        {
                            var cookies = (localResponse as HttpWebResponse)?.Cookies;

                            if (cookies != null)
                            {
                                if (_GlobalCookie == null)
                                    _GlobalCookie = new CookieContainer();

                                foreach (Cookie newCookie in cookies)
                                {
                                    _GlobalCookie.Add(request.RequestUri, newCookie);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _Logger?.WriteLog(ex?.ToString() ?? "Exception is null");
                        }
                    }

                    ReceiveStream.Close();
                }

                localResponse.Close();
            }

            return content;
        }
        #endregion

    }
}
