using System.Xml.Linq;
namespace Cognexa.Architecture.Tests;

public class DependenciasTests
{
    [Fact]
    public void ReferenciasRespeitamCamadas()
    {
        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (raiz is not null && !File.Exists(Path.Combine(raiz.FullName, "Directory.Build.props")))
            raiz = raiz.Parent;
        Assert.NotNull(raiz);
        var permitidas = new Dictionary<string, string[]> { ["Domain"] = [], ["Application"] = ["Domain"], ["Infrastructure"] = ["Domain", "Application"], ["WebApi"] = ["Application", "Infrastructure"], ["Worker"] = ["Application", "Infrastructure"] };
        foreach (var (camada, referencias) in permitidas)
        {
            var doc = XDocument.Load(Path.Combine(raiz.FullName, "src", $"Cognexa.{camada}", $"Cognexa.{camada}.csproj"));
            foreach (var item in doc.Descendants("ProjectReference"))
                Assert.Contains(Path.GetFileNameWithoutExtension(item.Attribute("Include")!.Value).Replace("Cognexa.", ""), referencias);
            if (camada == "Domain")
                Assert.Empty(doc.Descendants("PackageReference"));
            if (camada == "Application")
                Assert.DoesNotContain(doc.Descendants("PackageReference"), x => x.Attribute("Include")!.Value.Contains("EntityFramework") || x.Attribute("Include")!.Value.Contains("Npgsql"));
        }
    }
}
