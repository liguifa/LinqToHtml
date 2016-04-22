using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinqToHtml
{
    public class HtmlElement
    {
        public HtmlAttributes Attributes { get; set; }

        public HtmlElements Elements { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        private Expression<Func<HtmlElement, bool>> mWhere;

        public HtmlElement()
        {
            this.Attributes = new HtmlAttributes();
            this.Elements = new HtmlElements();
        }

        public HtmlElement Parse(string html)
        {
            ParseHtml parse = new ParseHtml();
            parse.Parse(html, this);
            return this;
        }

        public HtmlElement Load(Uri httpAddress)
        {
            return this.Load(httpAddress, Encoding.Default);
        }

        public HtmlElement Load(Uri httpAddress, Encoding encoding)
        {
            WebClient wc = new WebClient();
            Stream stream = wc.OpenRead(httpAddress);
            StreamReader streamReader = new StreamReader(stream, encoding);
            string html = streamReader.ReadToEnd();
            Parse(html);
            return this;
        }

        public HtmlElement Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Not found file {0}.", fileName);
            }
            return this.Load(fileName, Encoding.Default);
        }

        public HtmlElement Load(string fileName, Encoding encoding)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Not found file {0}.", fileName);
            }
            byte[] buffer = null;
            using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)buffer.Length);
            }
            string html = encoding.GetString(buffer);
            this.Parse(html);
            return this;
        }

        public HtmlElement Where(Expression<Func<HtmlElement, bool>> where)
        {
            this.mWhere = where;
            return this;
        }

        public List<HtmlElement> Find()
        {
            Func<HtmlElement, bool> func = this.mWhere.Compile();
            return this.Find(this, func);
        }

        private List<HtmlElement> Find(HtmlElement element, Func<HtmlElement, bool> func)
        {
            List<HtmlElement> innerElement = element.Elements;
            List<HtmlElement> outElement = new List<HtmlElement>();
            object syncRoot = new object();
            Parallel.ForEach(innerElement, e =>
            {
                if (func.Invoke(e))
                {
                    lock (syncRoot)
                    {
                        outElement.Add(e);
                    }
                }
                lock (syncRoot)
                {
                    outElement = outElement.Concat(this.Find(e, func)).ToList();
                }
            });
            return outElement;
        }
    }

    public class HtmlElements : List<HtmlElement>
    {

    }
}
