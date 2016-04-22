using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqToHtml
{
    public class ParseHtml
    {
        private readonly object mSyncRoot = new object();

        private void GetHtmlNode(ref List<string> nodes, string context, ref string text, string elementName = null)
        {
            context = context.Trim();
            if (String.IsNullOrEmpty(context))
            {
                return;
            }
            if (elementName == null)
            {
                if (context.StartsWith("<"))
                {
                    Regex nodeRegex = new Regex(HtmlRegex.NODETEXT);
                    Match match = nodeRegex.Match(context);
                    if (match.Value.Trim().EndsWith("/>"))
                    {
                        nodes.Add(match.Value);
                        context = context.Remove(0, match.Value.Length);
                        this.GetHtmlNode(ref nodes, context, ref text);
                    }
                    else
                    {
                        Regex nodenameRegex = new Regex(HtmlRegex.NODENAME);
                        Match nodenameMatch = nodenameRegex.Match(match.Value);
                        string nodeHtml = new Regex(String.Format(HtmlRegex.NODEHTML, nodenameMatch.Value)).Match(context).Value;
                        if (!String.IsNullOrEmpty(nodeHtml))
                        {
                            Regex nodeStart = new Regex(String.Format(HtmlRegex.NODESTART, nodenameMatch.Value));
                            Regex nodeEnd = new Regex(String.Format(HtmlRegex.NODEEND, nodenameMatch.Value));
                            context = context.Remove(0, nodeHtml.Length);
                            while (nodeStart.Matches(nodeHtml).Count != nodeEnd.Matches(nodeHtml).Count)
                            {

                                int number = nodeStart.Matches(nodeHtml).Count - nodeEnd.Matches(nodeHtml).Count;
                                for (int i = 0; i < number; i++)
                                {
                                    string temp = new Regex(String.Format(HtmlRegex.NODESTARTEND, nodenameMatch.Value)).Match(context).Value;
                                    nodeHtml += temp;
                                    context = context.Remove(0, temp.Length);
                                }
                            }
                            nodes.Add(nodeHtml);
                            this.GetHtmlNode(ref nodes, context, ref text);
                        }
                        else
                        {
                            string temp = match.Value.Substring(0, match.Value.Length - 1) + "/>";
                            nodes.Add(temp);
                            context = context.Remove(0, match.Value.Length);
                            this.GetHtmlNode(ref nodes, context, ref text);
                        }
                    }
                }
                else
                {
                    context += "<";
                    text += context.Substring(0, context.IndexOf('<'));
                    context = context.Remove(0, context.IndexOf('<'));
                    context = context.Remove(context.Length - 1, 1);
                    this.GetHtmlNode(ref nodes, context, ref text);
                }
            }
            else if (context.StartsWith("<"))
            {
                if (!context.EndsWith("/>"))
                {
                    context = new Regex(String.Format(HtmlRegex.NODEINNERHTML, elementName)).Match(context).Value;
                    this.GetHtmlNode(ref nodes, context, ref text);
                }
            }
        }

        private HtmlElement BuildElement(string ndeoText)
        {
            HtmlElement element = new HtmlElement();
            Regex attributeRegex = new Regex(HtmlRegex.ATTRIBULT);
            foreach (Match matchAttribute in attributeRegex.Matches(ndeoText))
            {
                string attrStr = matchAttribute.Value.Trim();
                string[] attrStrArr = attrStr.Split('=');
                HtmlAttribute attr = new HtmlAttribute();
                attr.Name = String.IsNullOrEmpty(attrStrArr[0]) ? "" : attrStrArr[0];
                attr.Value = "";
                if (attrStrArr.Count() > 1)
                {
                    attr.Value = String.IsNullOrEmpty(attrStrArr[1]) ? "" : attrStrArr[1].Remove(0, 1);
                    if (!String.IsNullOrEmpty(attr.Value))
                    {
                        attr.Value = attr.Value.Remove(attrStrArr[1].Length - 2, 1);
                    }
                }
                element.Attributes.Add(attr);
            }
            Regex nodenameRegex = new Regex(HtmlRegex.NODENAME);
            Match nodenameMatch = nodenameRegex.Match(ndeoText);
            element.Name = nodenameMatch.Value;
            return element;
        }

        private string Initialize(string context)
        {
            context = context.Trim();
            context = context.Replace("\r", "");
            context = context.Replace("\n", "");
            Regex documentRegex = new Regex(HtmlRegex.DOCUMENT);
            context = documentRegex.Replace(context, "");
            return context;
        }

        public void Parse(string mContext, HtmlElement parentElement)
        {
            mContext = this.Initialize(mContext);
            Regex nodeRegex = new Regex(HtmlRegex.NODETEXT);
            Match match = nodeRegex.Match(mContext);
            if (match != null && !String.IsNullOrEmpty(match.Value))
            {
                HtmlElement element = this.BuildElement(match.Value);
                string text = String.Empty;
                List<string> contextList = new List<string>();
                this.GetHtmlNode(ref contextList, mContext, ref text, element.Name);
                element.Text = text;
                lock (mSyncRoot)
                {
                    parentElement.Elements.Add(element);
                }
                Parallel.ForEach(contextList, context =>
                {
                    this.Parse(context, element);
                });
            }
        }
    }

    public static class HtmlRegex
    {
        public const string NODELABEL = @"<[^>]>";
        public const string SELFSEAL = @"<.*?(?=/>)";
        public const string OUTERSEAL = @"<.*?(?=>)>";
        public const string ATTRIBULT = @"(\s).*?=.*?(?=\s)[^>]";
        public const string NODENAME = @"(?<=<).*?(?=[\s>])";
        public const string NODETEXT = @"[^>][^<]*";
        public const string NODEINNERHTML = @"(?<=\<{0}.*\>).*(?=\</{0}\>)";
        public const string NODEHTML = @"(\<{0}.*?\>).*?(</{0}\>)";
        public const string DOCUMENT = @"\<\!DOCTYPE\shtml.*?\>";
        public const string NODESTARTEND = @".*?(\</{0}\>)";
        public const string NODESTART = @"\<{0}.*?\>";
        public const string NODEEND = @"\</{0}\>";
    }
}
