using LinqToHtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlElement root = new HtmlElement();
           List<HtmlElement> elements = root.Load(new Uri("http://210.30.62.8:8080/jsxsd/")).Elements;
           var query = from d in elements where d.Name == "html" select d.Name;
            
            foreach (HtmlElement element in root.Load(new Uri("http://210.30.62.8:8080/jsxsd/")).Where(d => d.Name == "a").Find())
            {
                Console.WriteLine(element.Attributes["href"]);
                using (FileStream fs = File.Open(@"d:\test\2.txt",FileMode.Append,FileAccess.Write))
                {
                    fs.Write(Encoding.Default.GetBytes(element.Text), 0, Encoding.Default.GetBytes(element.Text).Length);
                }
            }
            Console.WriteLine("Finish!");
            Console.Read();
        }
    }
}
