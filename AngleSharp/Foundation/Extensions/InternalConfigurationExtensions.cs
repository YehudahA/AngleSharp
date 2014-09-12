﻿namespace AngleSharp
{
    using AngleSharp.DOM;
    using AngleSharp.Infrastructure;
    using AngleSharp.Network;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a helper to construct objects with externally
    /// defined classes and libraries.
    /// </summary>
    [DebuggerStepThrough]
    static class InternalConfigurationExtensions
    {
        #region Encoding

        /// <summary>
        /// Gets the default encoding for the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use for getting the default encoding.</param>
        /// <returns>The current encoding.</returns>
        public static Encoding DefaultEncoding(this IConfiguration configuration)
        {
            if (configuration == null)
                configuration = Configuration.Default;

            return DocumentEncoding.Suggest(configuration.Language);
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads the given URI by using an asynchronous GET request.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <returns>The task which will eventually return the response.</returns>
        public static Task<IResponse> LoadAsync(this IConfiguration configuration, Url url)
        {
            return configuration.LoadAsync(url, CancellationToken.None);
        }

        /// <summary>
        /// Loads the given URI by using an asynchronous GET request.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <param name="cancel">The token which can be used to cancel the request.</param>
        /// <param name="force">[Optional] True if the request will be considered despite no allowed external request.</param>
        /// <returns>The task which will eventually return the response.</returns>
        public static Task<IResponse> LoadAsync(this IConfiguration configuration, Url url, CancellationToken cancel, Boolean force = false)
        {
            if (!configuration.AllowRequests && !force)
                return Empty<IResponse>();

            var requester = configuration.GetRequester();

            if (requester == null)
                throw new NullReferenceException("No HTTP requester has been set up in the configuration.");

            return requester.RequestAsync(new DefaultRequest
            {
                Address = url,
                Method = HttpMethod.Get
            }, cancel);
        }

        #endregion

        #region Fetching

        /// <summary>
        /// Performs a potentially CORS-enabled fetch from the given URI by using an asynchronous GET request.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <param name="cors">The cross origin settings to use.</param>
        /// <param name="origin">The origin of the page that requests the loading.</param>
        /// <param name="defaultBehavior">The default behavior in case it is undefined.</param>
        /// <returns>The task which will eventually return the stream.</returns>
        public static Task<IResponse> LoadWithCorsAsync(this IConfiguration configuration, Url url, CorsSetting cors, String origin, OriginBehavior defaultBehavior)
        {
            return configuration.LoadWithCorsAsync(url, cors, origin, defaultBehavior, CancellationToken.None);
        }

        /// <summary>
        /// Performs a potentially CORS-enabled fetch from the given URI by using an asynchronous GET request.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <param name="cors">The cross origin settings to use.</param>
        /// <param name="origin">The origin of the page that requests the loading.</param>
        /// <param name="defaultBehavior">The default behavior in case it is undefined.</param>
        /// <param name="cancel">The token which can be used to cancel the request.</param>
        /// <returns>The task which will eventually return the stream.</returns>
        public static Task<IResponse> LoadWithCorsAsync(this IConfiguration configuration, Url url, CorsSetting cors, String origin, OriginBehavior defaultBehavior, CancellationToken cancel)
        {
            if (!configuration.AllowRequests)
                return Empty<IResponse>();

            var requester = configuration.GetRequester();

            if (requester == null)
                throw new NullReferenceException("No HTTP requester has been set up in the configuration.");

            //TODO
            //http://www.w3.org/TR/html5/infrastructure.html#potentially-cors-enabled-fetch
            return requester.RequestAsync(new DefaultRequest
            {
                Address = url,
                Method = HttpMethod.Get
            }, cancel);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Loads the given URI by using an asynchronous request with the given method and body.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <param name="content">The body that should be used in the request.</param>
        /// <param name="mimeType">The mime-type of the request.</param>
        /// <param name="method">The method that is used for sending the request asynchronously.</param>
        /// <returns>The task which will eventually return the response.</returns>
        public static Task<IResponse> SendAsync(this IConfiguration configuration, Url url, Stream content = null, String mimeType = null, HttpMethod method = HttpMethod.Post)
        {
            return configuration.SendAsync(url, content, mimeType, method, CancellationToken.None);
        }

        /// <summary>
        /// Loads the given URI by using an asynchronous request with the given method and body.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="url">The url that yields the path to the desired action.</param>
        /// <param name="content">The body that should be used in the request.</param>
        /// <param name="mimeType">The mime-type of the request.</param>
        /// <param name="method">The method that is used for sending the request asynchronously.</param>
        /// <param name="cancel">The token which can be used to cancel the request.</param>
        /// <param name="force">[Optional] True if the request will be considered despite no allowed external request.</param>
        /// <returns>The task which will eventually return the response.</returns>
        public static Task<IResponse> SendAsync(this IConfiguration configuration, Url url, Stream content, String mimeType, HttpMethod method, CancellationToken cancel, Boolean force = false)
        {
            if (!configuration.AllowRequests && !force)
                return Empty<IResponse>();

            var requester = configuration.GetRequester();

            if (requester == null)
                throw new NullReferenceException("No HTTP requester has been set up in the configuration.");

            var request = new DefaultRequest
            {
                Address = url,
                Content = content,
                Method = method
            };

            if (mimeType != null)
                request.Headers[HeaderNames.ContentType] = mimeType;

            return requester.RequestAsync(request, cancel);
        }

        #endregion

        #region Services

        /// <summary>
        /// Gets a service with a specific type from the configuration, if it has been registered.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="configuration">The configuration instance to use.</param>
        /// <returns>The service, if any.</returns>
        public static TService GetService<TService>(this IConfiguration configuration)
            where TService : IService
        {
            foreach (var service in configuration.Services)
            {
                if (service is TService)
                    return (TService)service;
            }

            return default(TService);
        }

        /// <summary>
        /// Gets services with a specific type from the configuration, if it has been registered.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="configuration">The configuration instance to use.</param>
        /// <returns>An enumerable over all services.</returns>
        public static IEnumerable<TService> GetServices<TService>(this IConfiguration configuration)
            where TService : IService
        {
            foreach (var service in configuration.Services)
            {
                if (service is TService)
                    yield return (TService)service;
            }
        }

        #endregion

        #region Cookies

        /// <summary>
        /// Gets the cookie from the HTTP response.
        /// </summary>
        /// <param name="options">The configurations to use.</param>
        /// <param name="response">The response to the request.</param>
        /// <returns>The value of the cookie.</returns>
        public static String GetCookie(this IConfiguration options, IResponse response)
        {
            var service = options.GetService<ICookieService>();
            var cookie = String.Empty;

            if (service != null)
                cookie = service.GetCookie(response);
            else if (!response.Headers.TryGetValue(HeaderNames.SetCookie, out cookie))
                cookie = String.Empty;

            return cookie;
        }

        #endregion

        #region Parsing Styles

        /// <summary>
        /// Tries to resolve a style engine for the given type name.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="type">The mime-type of the source code.</param>
        /// <returns>The style engine or null, if the type if unknown.</returns>
        public static IStyleEngine GetStyleEngine(this IConfiguration configuration, String type)
        {
            foreach (var styleEngine in configuration.StyleEngines)
            {
                if (styleEngine.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                    return styleEngine;
            }

            return null;
        }
        
        /// <summary>
        /// Parses the given source code by using the supplied type name (otherwise it is text/css) and
        /// returns the created stylesheet.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="source">The source code of the style sheet.</param>
        /// <param name="owner">The optional owner of the stylesheet, if any.</param>
        /// <param name="type">The optional mime-type of the source code.</param>
        /// <returns>A freshly created stylesheet, if any.</returns>
        public static IStyleSheet ParseStyling(this IConfiguration configuration, String source, IElement owner = null, String type = null)
        {
            if (configuration.IsStyling)
            {
                var engine = configuration.GetStyleEngine(type ?? MimeTypes.Css);

                if (engine != null)
                    return engine.CreateStyleSheetFor(source, owner);
            }

            return null;
        }

        /// <summary>
        /// Parses the given source code by using the supplied type name (otherwise it is text/css) and
        /// returns the created stylesheet.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="source">The source code of the style sheet.</param>
        /// <param name="owner">The optional owner of the stylesheet, if any.</param>
        /// <param name="type">The optional mime-type of the source code.</param>
        /// <returns>A freshly created stylesheet, if any.</returns>
        public static IStyleSheet ParseStyling(this IConfiguration configuration, Stream source, IElement owner = null, String type = null)
        {
            if (configuration.IsStyling)
            {
                var engine = configuration.GetStyleEngine(type ?? MimeTypes.Css);

                if (engine != null)
                    return engine.CreateStyleSheetFor(source, owner);
            }

            return null;
        }

        #endregion

        #region Parsing Scripts

        /// <summary>
        /// Tries to resolve a script engine for the given type name.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="type">The mime-type of the source code.</param>
        /// <returns>The script engine or null, if the type if unknown.</returns>
        public static IScriptEngine GetScriptEngine(this IConfiguration configuration, String type)
        {
            foreach (var scriptEngine in configuration.ScriptEngines)
            {
                if (scriptEngine.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                    return scriptEngine;
            }

            return null;
        }

        /// <summary>
        /// Parses the given source code by using the supplied type name (otherwise it is text/css) and
        /// returns the created stylesheet.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="source">The source code of the style sheet.</param>
        /// <param name="options">The options for running the script.</param>
        /// <param name="type">The optional mime-type of the source code.</param>
        public static void RunScript(this IConfiguration configuration, String source, ScriptOptions options, String type = null)
        {
            if (configuration.IsScripting)
            {
                var engine = configuration.GetScriptEngine(type ?? MimeTypes.DefaultJavaScript);

                if (engine != null)
                    engine.Evaluate(source, options);
            }
        }

        /// <summary>
        /// Parses the given source code by using the supplied type name (otherwise it is text/css) and
        /// returns the created stylesheet.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="source">The source code of the style sheet.</param>
        /// <param name="options">The options for running the script.</param>
        /// <param name="type">The optional mime-type of the source code.</param>
        public static void RunScript(this IConfiguration configuration, Stream source, ScriptOptions options, String type = null)
        {
            if (configuration.IsScripting)
            {
                var engine = configuration.GetScriptEngine(type ?? MimeTypes.DefaultJavaScript);

                if (engine != null)
                    engine.Evaluate(source, options);
            }
        }

        #endregion

        #region Helpers

        static Task<TResult> Empty<TResult>()
            where TResult : class
        {
#if LEGACY
            var task = new TaskCompletionSource<TResult>();
            task.SetResult(null);
            return task.Task;
#else
            return Task.FromResult<TResult>(null);
#endif
        }

        #endregion
    }
}
