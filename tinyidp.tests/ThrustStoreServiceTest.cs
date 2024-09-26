using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.Certificate;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.tests;

public class ThrustStoreServiceTest
{
    private Mock<ILogger<ThrustStoreService>> _logger;
    private Mock<ICertificateRepository> _certificateRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private IThrustStoreService _thrustStoreService;
    private IMemoryCache _thrustedStore;
    private delegate void ServiceMemoryCache(object inputValue, out object outputValue);

    private string _rootCa = File.ReadAllText("../../../../tinyidp/ssl/root_ca/root_ca.pem");
    private string _intCa = File.ReadAllText("../../../../tinyidp/ssl/core_ca/certs/core_ca.pem");
    private string _validCert = "../../../../tinyidp/ssl/core_ca/client-test-ca.pem";
    private string _selfCert = "../../../../tinyidp/ssl/core_ca/test-self.pem";

    public ThrustStoreServiceTest()
    {
        _logger = new Mock<ILogger<ThrustStoreService>>();
        _certificateRepositoryMock = new Mock<ICertificateRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _thrustedStore = new MemoryCache(new MemoryCacheOptions
        {
        });
        _thrustStoreService = new ThrustStoreService(
            _logger.Object, 
            _certificateRepositoryMock.Object, 
            _thrustedStore);
    }

    [Fact]
    public async Task Test_VGetAllCaThrusted_ReturnsOk()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<ThrustStore>() { new ThrustStore() { Certificate = _rootCa } , new ThrustStore() { Certificate = _intCa } };

        _certificateRepositoryMock.Setup(x => x.GetThrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        var result = await _thrustStoreService.GetAllCaThrusted();

        // Assert
        Assert.NotNull(result);
        _certificateRepositoryMock.Verify(x => x.GetThrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_ValidChain_ReturnsTrue()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<ThrustStore>() { new ThrustStore() { Certificate = _rootCa } , new ThrustStore() { Certificate = _intCa } };

        _certificateRepositoryMock.Setup(x => x.GetThrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        var result = await _thrustStoreService.VerifyWithChain(certificate);

        // Assert
        Assert.True(result);
        _certificateRepositoryMock.Verify(x => x.GetThrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_EmptyTrustedStore_ThrowsTinyidpCertificateException()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        _certificateRepositoryMock.Setup(x => x.GetThrustStore()).Returns(Task.FromResult(new List<ThrustStore>()));

        // Act
        await Assert.ThrowsAsync<TinyidpCertificateException>(async () => await _thrustStoreService.VerifyWithChain(certificate));

        // Assert
        _certificateRepositoryMock.Verify(x => x.GetThrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_InvalidChain_ThrowsTinyidpCertificateException()
    {
        // Arrange
        var certificate = new X509Certificate2(_selfCert);
        var trustedStore = new List<ThrustStore>() { new ThrustStore() { Certificate = _rootCa } , new ThrustStore() { Certificate = _intCa } };
        _certificateRepositoryMock.Setup(x => x.GetThrustStore()).Returns(Task.FromResult(new List<ThrustStore>()));

        // Act
        await Assert.ThrowsAsync<TinyidpCertificateException>(async () => await _thrustStoreService.VerifyWithChain(certificate));

        // Assert
        _certificateRepositoryMock.Verify(x => x.GetThrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_TrustedStorePopulated_UsesCachedValue()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<ThrustStore>() { new ThrustStore() { Certificate = _rootCa } , new ThrustStore() { Certificate = _intCa } };
        _certificateRepositoryMock.Setup(x => x.GetThrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        await _thrustStoreService.VerifyWithChain(certificate);
        var result2 = await _thrustStoreService.VerifyWithChain(certificate);

        // Assert
        Assert.True(result2);
        _certificateRepositoryMock.Verify(x => x.GetThrustStore(), Times.Once);
    }
}    
