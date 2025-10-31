using AngleSharp.Html.Parser;
using Dotbot.Infrastructure.Entities.Reports;

namespace Dotbot.Infrastructure.SeedWork;

public static class SeedMotInspectionDefectDefinitions
{
    private static readonly string MotManualDirectory =
        Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.Parent!.FullName +
        "/mot-manuals";

    public static IEnumerable<VehicleMotInspectionDefectDefinition> GenerateMotInspectionDefectDefinitions()
    {
        var manualSections = Directory.GetFiles(MotManualDirectory).ToList();
        List<VehicleMotInspectionDefectDefinition> motInspectionDefectDefinitions = [];
        foreach (var manualSection in manualSections)
        {
            var htmlDocument = new HtmlParser().ParseDocument(File.OpenRead(manualSection));
            var htmlElements = htmlDocument.All.ToList();
            var sectionTitle = htmlDocument.QuerySelector("div[id=section-title]");
            var sectionTitleHeading = sectionTitle?.QuerySelector("h1");
            var topLevelCategory = sectionTitleHeading?.TextContent.Trim();

            var categoryArea = string.Empty;
            string? subCategoryName = null;

            foreach (var htmlElement in htmlElements)
                switch (htmlElement.LocalName)
                {
                    case "span" when
                        htmlElement.ClassName == "govuk-accordion__section-heading-text-focus":
                        categoryArea = htmlElement.TextContent.Trim();
                        break;
                    case "table":
                        {
                            for (var index = htmlElements.IndexOf(htmlElement) - 1; index > 0; index--)
                            {
                                var element = htmlElements.ElementAtOrDefault(index);
                                if (element?.LocalName == "table")
                                    break;

                                if (element?.LocalName == "h3" && !string.IsNullOrWhiteSpace(element.Id) &&
                                    element.Id.StartsWith("section"))
                                {
                                    subCategoryName = element.TextContent.Trim();
                                    break;
                                }
                            }

                            var tableBodyElements = htmlElement.QuerySelector("tbody");
                            if (tableBodyElements != null && htmlElement.InnerHtml.Contains("Defect"))
                            {
                                var rows = tableBodyElements.QuerySelectorAll("tr");
                                foreach (var row in rows)
                                {
                                    var columns = row.QuerySelectorAll("td");
                                    var referenceCode = columns.ElementAtOrDefault(0)?.TextContent;
                                    var defectText = columns.ElementAtOrDefault(1)?.TextContent;

                                    if (!string.IsNullOrWhiteSpace(topLevelCategory) &&
                                        !string.IsNullOrWhiteSpace(defectText))
                                        motInspectionDefectDefinitions.Add(new VehicleMotInspectionDefectDefinition
                                        {
                                            TopLevelCategory = topLevelCategory,
                                            CategoryArea = categoryArea,
                                            SubCategoryName = subCategoryName,
                                            DefectName = defectText,
                                            DefectReferenceCode = referenceCode
                                        });
                                }
                            }

                            break;
                        }
                }
        }

        return motInspectionDefectDefinitions;
    }
}