using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToHtml
{
    public class HtmlAttribute
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class HtmlAttributes : List<HtmlAttribute>
    {
        public string this[string name]
        {
            get
            {
                IEnumerable<HtmlAttribute> attributes = this.Where(d => d.Name.Equals(name)).Select(d => d);
                if (attributes != null && attributes.Count() > 0)
                {
                    return attributes.First().Value;
                }
                return null;
            }
            set
            {
                IEnumerable<HtmlAttribute> attributes = this.Where(d => d.Name.Equals(name)).Select(d => d);
                if (attributes != null && attributes.Count() > 0)
                {
                    attributes.First().Value = value;
                }
                throw new Exception("");
            }
        }
    }
}
