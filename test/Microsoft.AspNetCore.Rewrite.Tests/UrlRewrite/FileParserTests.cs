﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.AspNetCore.Rewrite.Internal.ModRewrite;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;
using Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class FileParserTests
    {
        [Fact]
        public void RuleParse_ParseTypicalRule()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(new List<Condition>(),
                url: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                actionType: ActionType.Rewrite,
                pattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = new UrlRewriteFileParser().Parse(new StringReader(xml));

            // assert
            AssertUrlRewriteRuleEquality(res, expected);
        }

        [Fact]
        public void RuleParse_ParseSingleRuleWithSingleCondition()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var condList = new List<Condition>();
            condList.Add(new Condition
            {
                Input = new InputParser().ParseInputString("{HTTPS}"),
                Match = new RegexMatch(new Regex("^OFF$"), false)
            });

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(condList,
                url: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                actionType: ActionType.Rewrite,
                pattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = new UrlRewriteFileParser().Parse(new StringReader(xml));

            // assert
            AssertUrlRewriteRuleEquality(res, expected);
        }

        [Fact]
        public void RuleParse_ParseMultipleRules()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                                <rule name=""Rewrite to another article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var condList = new List<Condition>();
            condList.Add(new Condition
            {
                Input = new InputParser().ParseInputString("{HTTPS}"),
                Match = new RegexMatch(new Regex("^OFF$"), false)
            });

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(condList,
                url: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                actionType: ActionType.Rewrite,
                pattern: "article.aspx?id={R:1}&amp;title={R:2}"));
            expected.Add(CreateTestRule(condList,
                url: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to another article.aspx",
                actionType: ActionType.Rewrite,
                pattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = new UrlRewriteFileParser().Parse(new StringReader(xml));

            // assert
            AssertUrlRewriteRuleEquality(res, expected);
        }

        // Creates a rule with appropriate default values of the url rewrite rule.
        private UrlRewriteRule CreateTestRule(List<Condition> conditions,
            LogicalGrouping condGrouping = LogicalGrouping.MatchAll,
            bool condTracking = false,
            string name = "",
            bool enabled = true,
            PatternSyntax patternSyntax = PatternSyntax.ECMAScript,
            bool stopProcessing = false,
            string url = "",
            bool ignoreCase = true,
            bool negate = false,
            ActionType actionType = ActionType.None,
            string pattern = "",
            bool appendQueryString = false,
            bool rewrittenUrl = false,
            RedirectType redirectType = RedirectType.Permanent
            )
        {
            return new UrlRewriteRule(name, new RegexMatch(new Regex("^OFF$"), false), conditions,
                new RewriteAction(RuleTermination.Continue, new InputParser().ParseInputString(url), queryStringAppend: false));
        }

        // TODO make rules comparable?
        private void AssertUrlRewriteRuleEquality(IList<UrlRewriteRule> actual, IList<UrlRewriteRule> expected)
        {
            Assert.Equal(actual.Count, expected.Count);
            for (var i = 0; i < actual.Count; i++)
            {
                var r1 = actual[i];
                var r2 = expected[i];

                Assert.Equal(r1.Name, r2.Name);

                if (r1.Conditions == null)
                {
                    Assert.Equal(r2.Conditions.Count, 0);
                }
                else if (r2.Conditions == null)
                {
                    Assert.Equal(r1.Conditions.Count, 0);
                }
                else
                {
                    Assert.Equal(r1.Conditions.Count, r2.Conditions.Count);
                    for (var j = 0; j < r1.Conditions.Count; j++)
                    {
                        var c1 = r1.Conditions[j];
                        var c2 = r2.Conditions[j];
                        Assert.Equal(c1.Input.PatternSegments.Count, c2.Input.PatternSegments.Count);
                    }
                }

                Assert.Equal(r1.Action.GetType(), r2.Action.GetType());
                Assert.Equal(r1.InitialMatch.GetType(), r2.InitialMatch.GetType());
            }
        }
    }
}