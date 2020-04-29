using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;
using Slurper.Contracts;
using Slurper.Providers;
using TypeMock.ArrangeActAssert;
using Xunit;

namespace SlurperTests
{
    public class ProgramTests
    {
        [Theory]

        [InlineData(PlatformID.Win32NT, typeof(FileSystemLayerWindows))]
        [InlineData(PlatformID.Unix, typeof(FileSystemLayerLinux))]
        [InlineData(PlatformID.MacOSX, typeof(FileSystemLayerLinux))]
        public void ChoseFileSystemLayer(PlatformID platformId, Type type)
        {
            // Given 
            var sub = Substitute.For<EnvironmentService>();
            sub.GetOsPlatform().Returns(platformId);

            // When
            var actual = Slurper.Program.ChoseFileSystemLayer();
            
            // Then
            Assert.Equal(type, actual.GetType());

        }
    }
}