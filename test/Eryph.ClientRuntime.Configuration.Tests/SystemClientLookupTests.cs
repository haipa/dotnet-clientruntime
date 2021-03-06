using System;
using System.IO;
using System.Runtime.InteropServices;
using Eryph.IdentityModel.Clients;
using Moq;
using Xunit;

namespace Eryph.ClientRuntime.Configuration.Tests
{
    public class SystemClientLookupTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSystemClient_returns_null_if_process_not_running(bool forEryphZero)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);

            var moduleName = forEryphZero ? "zero" : "identity";
            filesystemMock.Setup(x =>
                    x.OpenText(It.Is<string>(p => p.EndsWith($"{moduleName}{Path.DirectorySeparatorChar}.run_info"))))
                .Returns(new StringReader("{\"process_id\" : 100, \"url\" : \"http://eryph.io\"}"));


            environmentMock.Setup(x => x.IsProcessRunning("", 100)).Returns(false);

            var lookup = new ClientCredentialsLookup(environmentMock.Object);

            Assert.Null(lookup.GetSystemClientCredentials());
        }

        [Theory]
        [InlineData("default")]
        [InlineData("other")]
        [InlineData("local")]
        [InlineData("zero")]
        public void GetSystemClient_considers_configuration(string configurationName)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);

            var lockPath = configurationName == "local" ? "identity" : "zero";
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(true);
            environmentMock.Setup(x => x.IsWindowsAdminUser).Returns(true);

            filesystemMock.Setup(x =>
                    x.OpenText(It.Is<string>(p => p.EndsWith($"{lockPath}{Path.DirectorySeparatorChar}.lock"))))
                .Returns(() => new StringReader($"{{\"processName\":\"TestingEryph\",\"processId\":100,\"endpoints\":{{\"identity\":\"http://localhost\"}}}}"));

            environmentMock.Setup(x => x.IsProcessRunning("TestingEryph", 100)).Returns(true);

            var lookup = new ClientCredentialsLookup(environmentMock.Object);

            if (configurationName!="zero" && configurationName != "local")
                Assert.Throws<InvalidOperationException>(() => lookup.GetSystemClientCredentials(configurationName));
            else
            {
                var systemClient = lookup.GetSystemClientCredentials(configurationName);
                Assert.NotNull(systemClient);
            }

            filesystemMock.Verify();
        }

        [Theory]
        [InlineData("http://eryph.io/identity", "http://eryph.io/identity")]
        [InlineData("http://eryph.io", "http://eryph.io/")]
        [InlineData("http://localhost:4711", "http://localhost:4711/")]
        public void GetSystemClient_reads_process_info(string baseUrl, string identityEndpoint)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);
            

            filesystemMock.Setup(x =>
                    x.OpenText(It.Is<string>(p => p.EndsWith(".lock"))))
                .Returns( () => new StringReader($"{{\"processName\":\"TestingEryph\",\"processId\":100,\"endpoints\":{{\"identity\":\"{baseUrl}\"}}}}"));

            environmentMock.Setup(x => x.IsProcessRunning("TestingEryph", 100)).Returns(true);

            var lookup = new ClientCredentialsLookup(environmentMock.Object);
            var response = lookup.GetSystemClientCredentials();

            Assert.NotNull(response);
            Assert.Equal(identityEndpoint, response.IdentityProvider.ToString());
            Assert.NotNull(response.KeyPairData);
            Assert.Equal("system-client", response.Id);

            environmentMock.Verify();
            filesystemMock.Verify();
        }

        private Mock<IFileSystem> SetupEnvironmentAndFileSystemWithClientKey(Mock<IEnvironment> environmentMock)
        {
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(false);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux))).Returns(true);

            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(x => x.OpenText(It.IsAny<string>())).Throws<FileNotFoundException>();

            fileSystemMock.Setup(x => x.OpenText(It.Is<string>(x => x.EndsWith("system-client.key"))))
                .Returns(new StringReader(TestData.PrivateKeyFileString));

            environmentMock.Setup(x => x.FileSystem).Returns(fileSystemMock.Object);
            return fileSystemMock;
        }

        [Fact]
        public void GetSystemClient_Throws_if_not_AdminOn_Windows()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(true);
            environmentMock.Setup(x => x.IsWindowsAdminUser).Returns(false);

            var lookup = new ClientCredentialsLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() => lookup.GetSystemClientCredentials());
        }

        [Fact]
        public void GetSystemClient_Throws_if_OSX()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux || p == OSPlatform.Windows)))
                .Returns(false);

            var lookup = new ClientCredentialsLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() => lookup.GetSystemClientCredentials());
            environmentMock.Verify();
        }

        [Fact]
        public void GetSystemClient_Throws_if_EryphZero_and_not_Windows()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows)))
                .Returns(false);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux)))
                .Returns(true);


            var lookup = new ClientCredentialsLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() => lookup.GetSystemClientCredentials("zero"));
            environmentMock.Verify();
        }
    }
}