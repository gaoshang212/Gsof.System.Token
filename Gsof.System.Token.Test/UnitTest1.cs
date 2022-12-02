using Gsof.Extensions;

namespace Gsof.System.Token.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task VerifyToken()
        {
            var result = await ProcessEx.Exec(@"dmidecode.exe", "-s system-uuid", 1000);

            var token = await Token.GetSystemToken(false);
            Assert.AreEqual(token, result.Trim());
        }

        [TestMethod]
        public async Task HasToken()
        {
            var token = await Token.GetSystemToken();
            Assert.IsNotNull(token);
        }

        [TestMethod]
        public void SMBiosUUID()
        {
            var token = SMBios.UUID();

            Assert.AreNotEqual(token.Trim(), "");
        }
    }
}