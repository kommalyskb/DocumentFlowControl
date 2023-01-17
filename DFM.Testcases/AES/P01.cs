using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.AES
{
    public class P01
    {
        private readonly IAESHelper aes;
        public P01()
        {
            this.aes = new AESHelper();
        }

        [Fact]
        public void GenAes()
        {
            var result = aes.Prepair();

            string key = result.Key.ToHEX();
            string iv = result.IV.ToHEX();
            Assert.Equal(key, key);
        }

        [Theory]
        [InlineData("123456@Dfm.local")]
        public void Encrypt(string userName)
        {
            var key = "E26E934E4A3A5756CA46037976C13BEB9F29EC28E5A7007030BD804309248474".FromHEX();
            var iv = "9AA84A02A985ED36999DB23CE005A0DC".FromHEX();
            var result = aes.Encrypt(userName, key, iv).ToHEX();

            Assert.Equal(key, key);
        }

        [Theory]
        [InlineData("12C0E1091903A7688E7C8352977A1B8192B9FFDEE00A80D0A86ADEFDB47EEE77")]
        public void Decrypt(string str)
        {
            var key = "E26E934E4A3A5756CA46037976C13BEB9F29EC28E5A7007030BD804309248474".FromHEX();
            var iv = "9AA84A02A985ED36999DB23CE005A0DC".FromHEX();
            var result = aes.Decrypt(str.FromHEX(), key, iv);

            Assert.Equal(key, key);
        }
    }
}
