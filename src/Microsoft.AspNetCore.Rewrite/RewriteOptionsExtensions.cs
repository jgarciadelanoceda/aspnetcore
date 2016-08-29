﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.AspNetCore.Rewrite.Internal.CodeRules;
using Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite;

namespace Microsoft.AspNetCore.Rewrite
{
    /// <summary>
    /// The builder to a list of rules for <see cref="RewriteOptions"/> and <see cref="RewriteMiddleware"/> 
    /// </summary>
    public static class RewriteOptionsExtensions
    {
        /// <summary>
        /// Adds a rule to the current rules.
        /// </summary>
        /// <param name="options">The UrlRewrite options.</param>
        /// <param name="rule">A rule to be added to the current rules.</param>
        /// <returns>The Rewrite options.</returns>
        public static RewriteOptions Add(this RewriteOptions options, Rule rule)
        {
            options.Rules.Add(rule);
            return options;
        }

        /// <summary>
        /// Adds a rule to the current rules.
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="applyRule">A Func that checks and applies the rule.</param>
        /// <returns></returns>
        public static RewriteOptions Add(this RewriteOptions options, Action<RewriteContext> applyRule)
        {
            options.Rules.Add(new DelegateRule(applyRule));
            return options;
        }

        /// <summary>
        /// Rewrites the path if the regex matches the HttpContext's PathString
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="regex">The regex string to compare with.</param>
        /// <param name="replacement">If the regex matches, what to replace HttpContext with.</param>
        /// <returns>The Rewrite options.</returns>
        public static RewriteOptions Rewrite(this RewriteOptions options, string regex, string replacement)
        {
            return Rewrite(options, regex, replacement, stopProcessing: false);
        }

        /// <summary>
        /// Rewrites the path if the regex matches the HttpContext's PathString
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="regex">The regex string to compare with.</param>
        /// <param name="replacement">If the regex matches, what to replace the uri with.</param>
        /// <param name="stopProcessing">If the regex matches, conditionally stop processing other rules.</param>
        /// <returns>The Rewrite options.</returns>
        public static RewriteOptions Rewrite(this RewriteOptions options, string regex, string replacement, bool stopProcessing)
        {
            options.Rules.Add(new RewriteRule(regex, replacement, stopProcessing));
            return options;
        }

        /// <summary>
        /// Redirect the request if the regex matches the HttpContext's PathString
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="regex">The regex string to compare with.</param>
        /// <param name="replacement">If the regex matches, what to replace the uri with.</param>
        /// <returns>The Rewrite options.</returns>
        public static RewriteOptions Redirect(this RewriteOptions options, string regex, string replacement)
        {
            return Redirect(options, regex, replacement, statusCode: 302);
        }

        /// <summary>
        /// Redirect the request if the regex matches the HttpContext's PathString
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="regex">The regex string to compare with.</param>
        /// <param name="replacement">If the regex matches, what to replace the uri with.</param>
        /// <param name="statusCode">The status code to add to the response.</param>
        /// <returns>The Rewrite options.</returns>
        public static RewriteOptions Redirect(this RewriteOptions options, string regex, string replacement, int statusCode)
        {
            options.Rules.Add(new RedirectRule(regex, replacement, statusCode));
            return options;
        }

        public static RewriteOptions RedirectToHttpsPermanent(this RewriteOptions options)
        {
            return RedirectToHttps(options, statusCode: 301, sslPort: null);
        }

        /// <summary>
        /// Redirect a request to https if the incoming request is http
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        public static RewriteOptions RedirectToHttps(this RewriteOptions options)
        {
            return RedirectToHttps(options, statusCode: 302, sslPort: null);
        }

        /// <summary>
        /// Redirect a request to https if the incoming request is http
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="statusCode">The status code to add to the response.</param>
        public static RewriteOptions RedirectToHttps(this RewriteOptions options, int statusCode)
        {
            return RedirectToHttps(options, statusCode, sslPort: null);
        }

        /// <summary>
        /// Redirect a request to https if the incoming request is http
        /// </summary>
        /// <param name="options">The Rewrite options.</param>
        /// <param name="statusCode">The status code to add to the response.</param>
        /// <param name="sslPort">The SSL port to add to the response.</param>
        public static RewriteOptions RedirectToHttps(this RewriteOptions options, int statusCode, int? sslPort)
        {
            options.Rules.Add(new RedirectToHttpsRule { StatusCode = statusCode, SSLPort = sslPort });
            return options;
        }
    }
}
