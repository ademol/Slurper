using System.Threading.Tasks;
using Slurper.Providers;
using Xunit;

namespace SlurperTests.Providers
{
    public class FileSystemLayerLinuxTests
    {
        [Fact]
        public async Task CreateTargetLocation()
        {
            // Given
            var fsll = new FileSystemLayerLinux();

            // When
            fsll.CreateTargetLocation();

            // Then
        }
        
        public async Task GetMountedPartitionInfo()
        {
            // Given
            
            // When
            
            // Then
        }
    }
}

