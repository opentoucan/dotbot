using System.Text.RegularExpressions;
using Dotbot.Infrastructure.Entities.Reports;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

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
            var document = PdfDocument.Open(manualSection);

            var category1 = string.Empty;
            var currentSubcategory2 = string.Empty;
            var currentSubcategory3 = string.Empty;

            foreach (var page in document.GetPages())
            {
                var letters = page.Letters;
                var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
                var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
                var orderedTextBlocks = UnsupervisedReadingOrderDetector.Instance.Get(textBlocks).ToList();


                foreach (var block in orderedTextBlocks)
                    if (new Regex("^(\\d+\\.){1}\\s(.|\\n)+$").Match(block.Text).Success)
                    {
                        category1 = block.Text.Split('\n')[0];
                    }

                    else if (new Regex("^(\\d+\\.){2}\\s(.|\\n)+$").Match(block.Text).Success)
                    {
                        currentSubcategory2 = block.Text.Split('\n')[0];
                    }

                    else if (new Regex("^(\\d+\\.){3}\\s(.|\\n)+$").Match(block.Text).Success)
                    {
                        currentSubcategory3 = block.Text.Split('\n')[0];
                    }
                    else if (new Regex("^(\\(\\w+\\))+$").Match(block.Text).Success)
                    {
                        var currentIndex = orderedTextBlocks.IndexOf(block);
                        motInspectionDefectDefinitions.Add(new VehicleMotInspectionDefectDefinition
                        {
                            TopLevelCategory = category1,
                            CategoryArea = currentSubcategory2,
                            SubCategoryName = currentSubcategory3,
                            DefectName = orderedTextBlocks.ElementAtOrDefault(currentIndex + 1)!.Text,
                            DefectReferenceCode = block.Text
                        });
                    }
            }
        }

        return motInspectionDefectDefinitions;
    }
}