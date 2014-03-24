using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkerRole.Processors;

namespace DocprocTest
{
    [TestClass]
    public class TemplateTest
    {
        [TestMethod]
        public void Template_TestFill()
        {
            string template = "${name}<br />" +
                              "${street}<br />" +
                              "${zipcode} ${city}<br />" +
                              "${email}";
            TemplateProcessor processor = new TemplateProcessor();
            
        }
    }
}
