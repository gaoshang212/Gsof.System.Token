using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gsof.System.Token.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task GetToken()
        {
            var token = await Token.GetSystemToken();
            Console.WriteLine(token);

            Assert.IsNotNull(token);
        }
    }
}
