using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace P42.Uno;

/// <summary>
/// Pdf Generation Options from https://ekoopmans.github.io/html2pdf.js/
/// </summary>
/// <param name="Margin">PDF margin (in jsPDF units). Can be a single number, [vMargin, hMargin], or [top, left, bottom, right].</param>
/// <param name="Filename">The default filename of the exported PDF.</param>
/// <param name="PageBreak">Controls the pagebreak behaviour on the page.</param>
/// <param name="Image">The image type and quality used to generate the PDF.</param>
/// <param name="EnableLinks">If enabled, PDF hyperlinks are automatically added ontop of all anchor tags.</param>
/// <param name="Html2canvas">Configuration options sent directly to html2canvas</param>
/// <param name="JsPDF">Configuration options sent directly to jsPDF</param>
public record PdfOptions(
    [ValidMargin]
    double[]? Margin = null, 
    string Filename = "document.pdf",
    PdfPageBreakMode? PageBreak = null, 
    PdfImageSettings? Image = null, 
    bool? EnableLinks = null, 
    Html2CanvasOptions? Html2canvas = null,
    JsPdfOptions? JsPDF = null
    );

/// <summary>
/// Html rendering options from https://html2canvas.hertzen.com/configuration
/// </summary>
/// <param name="AllowTaint">Whether to allow cross-origin images to taint the canvas</param>
/// <param name="BackgroundColor">Canvas background color, if none is specified in DOM. Set null for transparent</param>
/// <param name="ForeignObjectRendering">Whether to use ForeignObject rendering if the browser supports it</param>
/// <param name="ImageTimeout">Timeout for loading an image (in milliseconds). Set to 0 to disable timeout.</param>
/// <param name="IngoreElements">Predicate function which removes the matching elements from the render.</param>
/// <param name="Logging">Enable logging for debug purposes</param>
/// <param name="Proxy">Url to the proxy which is to be used for loading cross-origin images. If left empty, cross-origin images won’t be loaded.</param>
/// <param name="Scale">The scale to use for rendering. Defaults to the browsers device pixel ratio.</param>
/// <param name="UseCORS">Whether to attempt to load images from a server using CORS</param>
/// <param name="Width">The width of the canvas</param>
/// <param name="Height">The height of the canvas</param>
/// <param name="X">Crop canvas x-coordinate</param>
/// <param name="Y">Crop canvas y-coordinate</param>
/// <param name="ScrollX">The x-scroll position to used when rendering element</param>
/// <param name="ScrollY">The y-scroll position to used when rendering element</param>
/// <param name="WindowWidth">Window width to use when rendering Element, which may affect things like Media queries</param>
/// <param name="WindowHeight">Window height to use when rendering Element, which may affect things like Media queries</param>
public record Html2CanvasOptions(
    bool? AllowTaint = null,
    string? BackgroundColor = null,
    bool? ForeignObjectRendering = null,
    int? ImageTimeout = null,
    string? IngoreElements = null,
    bool? Logging = null,
    string? Proxy = null,
    double? Scale = null,
    bool? UseCORS = null,
    double? Width = null,
    double? Height = null,
    double? X = null,
    double? Y = null,
    double? ScrollX = null,
    double? ScrollY = null,
    double? WindowWidth = null,
    double? WindowHeight = null
    );

/// <summary>
/// PDF generation options from https://rawgit.com/MrRio/jsPDF/master/docs/jsPDF.html
/// </summary>
/// <param name="Orientation">Orientation of the first page.</param>
/// <param name="Unit">Measurement unit (base unit) to be used when coordinates are specified</param>
/// <param name="Format">The format of the first page</param>
/// <param name="Compress">Compress the generated PDF</param>
/// <param name="Encryption">Encryption options</param>
public record JsPdfOptions(
    PdfPageOrientation? Orientation = null, 
    PdfUnits? Unit = null,
    PdfPageSize? Format = null,
    bool? Compress = null,
    PdfEncryption? Encryption = null
    );

public class ValidMarginAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is null or ICollection<double> { Count: 0 or 1 or 2 or 4 }
            ? ValidationResult.Success
            : new ValidationResult("The property is not a valid collection type.");
}


/// <summary>
/// PDF page break handling 
/// </summary>
[Flags]
public enum PdfPageBreakMode
{
    /// <summary>
    /// Automatically adds page-breaks to avoid splitting any elements across pages.
    /// </summary>
    [JsonStringEnumMemberName("avoid-all")]
    AvoidAll = 0,
    /// <summary>
    /// Adds page-breaks according to the CSS break-before, break-after, and break-inside properties. Only recognizes always/left/right for before/after, and avoid for inside.
    /// </summary>
    [JsonStringEnumMemberName("css")]
    Css = 1,
    /// <summary>
    /// Adds page-breaks after elements with class html2pdf__page-break. This feature may be removed in the future.
    /// </summary>
    [JsonStringEnumMemberName("legacy")]
    Legacy = 2
}

/// <summary>
/// The image type used to generate the PDF
/// </summary>
public enum PdfImageType
{
    [JsonStringEnumMemberName("jpeg")]
    Jpeg,
    [JsonStringEnumMemberName("png")]
    Png
}

/// <summary>
/// The image type and quality used to generate the PDF
/// </summary>
/// <param name="Type"></param>
/// <param name="Quality"></param>
public record PdfImageSettings(PdfImageType Type = PdfImageType.Jpeg, float Quality = 0.95f)
{
    public static PdfImageSettings Default = new();

}

/// <summary>
/// Page orientation
/// </summary>
public enum PdfPageOrientation
{
    [JsonStringEnumMemberName("portrait")]
    Portrait,
    [JsonStringEnumMemberName("landscape")]
    Landscape
}

/// <summary>
/// Units for PDF
/// </summary>
public enum PdfUnits
{
    /// <summary>
    /// Points
    /// </summary>
    [JsonStringEnumMemberName("pt")]
    Pt,
    /// <summary>
    /// millimeters
    /// </summary>
    [JsonStringEnumMemberName("mm")]
    Mm,
    /// <summary>
    /// centimeters
    /// </summary>
    [JsonStringEnumMemberName("cm")]
    Cm,
    /// <summary>
    /// inches
    /// </summary>
    [JsonStringEnumMemberName("in")]
    In,
    /// <summary>
    /// pixels
    /// </summary>
    [JsonStringEnumMemberName("px")]
    Px,
    /// <summary>
    /// pica
    /// </summary>
    [JsonStringEnumMemberName("pc")]
    Pc,
    /// <summary>
    /// emphasis
    /// </summary>
    [JsonStringEnumMemberName("em")]
    Em,
    /// <summary>
    /// ex unit is a relative unit in CSS that is based on the "x-height" of a font
    /// </summary>
    [JsonStringEnumMemberName("ex")]
    Ex
}

/// <summary>
/// Size of PDF Page
/// </summary>
public enum PdfPageSize
{
    [JsonStringEnumMemberName("a0")]
    A0,
    [JsonStringEnumMemberName("a1")]
    A1,
    [JsonStringEnumMemberName("a2")]
    A2,
    [JsonStringEnumMemberName("a3")]
    A3,
    [JsonStringEnumMemberName("a4")]
    A4,
    [JsonStringEnumMemberName("a5")]
    A5,
    [JsonStringEnumMemberName("a6")]
    A6,
    [JsonStringEnumMemberName("a7")]
    A7,
    [JsonStringEnumMemberName("a8")]
    A8,
    [JsonStringEnumMemberName("a9")]
    A9,
    [JsonStringEnumMemberName("a10")]
    A10,
    [JsonStringEnumMemberName("b0")]
    B0,
    [JsonStringEnumMemberName("b1")]
    B1,
    [JsonStringEnumMemberName("b2")]
    B2,
    [JsonStringEnumMemberName("b3")]
    B3,
    [JsonStringEnumMemberName("b4")]
    B4,
    [JsonStringEnumMemberName("b5")]
    B5,
    [JsonStringEnumMemberName("b6")]
    B6,
    [JsonStringEnumMemberName("b7")]
    B7,
    [JsonStringEnumMemberName("b8")]
    B8,
    [JsonStringEnumMemberName("b9")]
    B9,
    [JsonStringEnumMemberName("b10")]
    B10,
    [JsonStringEnumMemberName("c0")]
    C0,
    [JsonStringEnumMemberName("c1")]
    C1,
    [JsonStringEnumMemberName("c2")]
    C2,
    [JsonStringEnumMemberName("c3")]
    C3,
    [JsonStringEnumMemberName("c4")]
    C4,
    [JsonStringEnumMemberName("c5")]
    C5,
    [JsonStringEnumMemberName("c6")]
    C6,
    [JsonStringEnumMemberName("c7")]
    C7,
    [JsonStringEnumMemberName("c8")]
    C8,
    [JsonStringEnumMemberName("c9")]
    C9,
    [JsonStringEnumMemberName("c10")]
    C10,
    [JsonStringEnumMemberName("dl")]
    Dl,
    [JsonStringEnumMemberName("letter")]
    Letter,
    [JsonStringEnumMemberName("government-letter")]
    GovernmentLetter,
    [JsonStringEnumMemberName("legal")]
    Legal,
    [JsonStringEnumMemberName("junior-legal")]
    JuniorLegal,
    [JsonStringEnumMemberName("ledger")]
    Ledger,
    [JsonStringEnumMemberName("tabloid")]
    Tabloid,
    [JsonStringEnumMemberName("credit-card")]
    CreditCard
}

/// <summary>
/// User permissions enabled in PDF
/// </summary>
public enum PdfUserPermissions
{
    [JsonStringEnumMemberName("print")]
    Print,
    [JsonStringEnumMemberName("modify")]
    Modify,
    [JsonStringEnumMemberName("copy")]
    Copy,
    [JsonStringEnumMemberName("annot-forms")]
    AnnotForms
}

/// <summary>
/// PDF Encryption Options
/// </summary>
/// <param name="OwnerPassword"></param>
/// <param name="UserPassword"></param>
/// <param name="UserPermissions"></param>
public record PdfEncryption(string OwnerPassword, string? UserPassword = null, PdfUserPermissions? UserPermissions = null);

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true)]
[JsonSerializable(typeof(PdfOptions))]
internal partial class PdfOptionsSourceGenerationContext : JsonSerializerContext;
