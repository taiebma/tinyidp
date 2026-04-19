using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using tinyidp.Business.Certificate;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.tests;

public class TrustStoreServiceTest
{
    private Mock<ILogger<TrustStoreService>> _logger;
    private Mock<ICertificateRepository> _certificateRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private ITrustStoreService _TrustStoreService;
    private IMemoryCache _TrustedStore;
    private delegate void ServiceMemoryCache(object inputValue, out object outputValue);

    private string _rootCa = File.ReadAllText("../../../../tinyidp/ssl/root_ca/root_ca.pem");
    private string _intCa = File.ReadAllText("../../../../tinyidp/ssl/core_ca/certs/core_ca.pem");
    private string _validCert = "../../../../tinyidp/ssl/core_ca/client-test-ca.pem";
    private string _selfCert = "../../../../tinyidp/ssl/core_ca/test-self.pem";

    public TrustStoreServiceTest()
    {
        _logger = new Mock<ILogger<TrustStoreService>>();
        _certificateRepositoryMock = new Mock<ICertificateRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _TrustedStore = new MemoryCache(new MemoryCacheOptions
        {
        });
        _TrustStoreService = new TrustStoreService(
            _logger.Object, 
            _certificateRepositoryMock.Object, 
            _TrustedStore);
    }

    [Fact]
    public async Task Test_VGetAllCaTrusted_ReturnsOk()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<TrustStore>() { new TrustStore() { Certificate = _rootCa } , new TrustStore() { Certificate = _intCa } };

        _certificateRepositoryMock.Setup(x => x.GetTrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        var result = await _TrustStoreService.GetAllCaTrusted();

        // Assert
        Assert.NotNull(result);
        _certificateRepositoryMock.Verify(x => x.GetTrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_ValidChain_ReturnsTrue()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<TrustStore>() { new TrustStore() { Certificate = _rootCa } , new TrustStore() { Certificate = _intCa } };

        _certificateRepositoryMock.Setup(x => x.GetTrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        var result = await _TrustStoreService.VerifyWithChain(certificate);

        // Assert
        Assert.True(result);
        _certificateRepositoryMock.Verify(x => x.GetTrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_EmptyTrustedStore_ThrowsTinyidpCertificateException()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        _certificateRepositoryMock.Setup(x => x.GetTrustStore()).Returns(Task.FromResult(new List<TrustStore>()));

        // Act
        await Assert.ThrowsAsync<TinyidpCertificateException>(async () => await _TrustStoreService.VerifyWithChain(certificate));

        // Assert
        _certificateRepositoryMock.Verify(x => x.GetTrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_InvalidChain_ThrowsTinyidpCertificateException()
    {
        // Arrange
        var certificate = new X509Certificate2(_selfCert);
        var trustedStore = new List<TrustStore>() { new TrustStore() { Certificate = _rootCa } , new TrustStore() { Certificate = _intCa } };
        _certificateRepositoryMock.Setup(x => x.GetTrustStore()).Returns(Task.FromResult(new List<TrustStore>()));

        // Act
        await Assert.ThrowsAsync<TinyidpCertificateException>(async () => await _TrustStoreService.VerifyWithChain(certificate));

        // Assert
        _certificateRepositoryMock.Verify(x => x.GetTrustStore(), Times.Once);
    }

    [Fact]
    public async Task Test_VerifyWithChain_TrustedStorePopulated_UsesCachedValue()
    {
        // Arrange
        var certificate = new X509Certificate2(_validCert);
        var trustedStore = new List<TrustStore>() { new TrustStore() { Certificate = _rootCa } , new TrustStore() { Certificate = _intCa } };
        _certificateRepositoryMock.Setup(x => x.GetTrustStore()).Returns(Task.FromResult(trustedStore));

        // Act
        await _TrustStoreService.VerifyWithChain(certificate);
        var result2 = await _TrustStoreService.VerifyWithChain(certificate);

        // Assert
        Assert.True(result2);
        _certificateRepositoryMock.Verify(x => x.GetTrustStore(), Times.Once);
    }
}    
