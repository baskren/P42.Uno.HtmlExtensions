using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Printing;

namespace P42.Uno.HtmlExtensions
{
    public class PrintHelper
    {
        const string CanvasName = "P42.Uno.HtmlExtensions.PrintCanvas";
        /// <summary>
        /// The percent of app's margin width, content is set at 85% (0.85) of the area's width
        /// </summary>
        protected double ApplicationContentMarginLeft = 0.075;

        /// <summary>
        /// The percent of app's margin height, content is set at 94% (0.94) of tha area's height
        /// </summary>
        protected double ApplicationContentMarginTop = 0.03;
        
        /// <summary>
        /// PrintDocument is used to prepare the pages for printing.
        /// Prepare the pages to print in the handlers for the Paginate, GetPreviewPage, and AddPages events.
        /// </summary>
        protected PrintDocument printDocument;

        /// <summary>
        /// Marker interface for document source
        /// </summary>
        protected IPrintDocumentSource printDocumentSource;

        /// <summary>
        /// A list of UIElements used to store the print preview pages.  This gives easy access
        /// to any desired preview page.
        /// </summary>
        internal List<UIElement> printPreviewPages;

        // Event callback which is called after print preview pages are generated.  Photos scenario uses this to do filtering of preview pages
        //protected event EventHandler PreviewPagesCreated;

        Task<IRandomAccessStreamWithContentType> wideMarginsIconTask;
        Task<IRandomAccessStreamWithContentType> moderateMarginsIconTask;
        Task<IRandomAccessStreamWithContentType> narrowMarginsIconTask;


        /// <summary>
        ///  A hidden canvas used to hold pages we wish to print
        /// </summary>
        protected Canvas PrintCanvas
        {
            get
            {
                if (RootPage.FindName(CanvasName) is Canvas canvas)
                    return canvas;
                RootPanel?.Children.Insert(0, new Canvas { Opacity = 0.0, Name = CanvasName });
                return RootPage.FindName(CanvasName) as Canvas;
            }
        }

       
        internal static Page RootPage
        {
            get
            {
                var rootFrame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                var page = rootFrame?.Content as Windows.UI.Xaml.Controls.Page;
                //var panel = page?.Content as Panel;
                //var children = panel.Children.ToList();
                return page;
            }
        }

        internal static Panel RootPanel => RootPage?.Content as Panel;

        protected UIElement PrintContent;

        protected Grid PrintSpinner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scenarioPage">The scenario page constructing us</param>
        public PrintHelper(string jobName)
        {
            JobName = jobName;

            printPreviewPages = new List<UIElement>();

            /*
            // Start these tasks early because we know we're going to need the
            // streams in PrintTaskRequested.
            //var wideMarginsIconReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///P42.Uno.HtmlExtensions/Assets/wideMargins.svg"));
            var wideMarginsIconStream = GetType().Assembly.GetManifestResourceStream("P42.Uno.HtmlExtensions.Resources.wideMargins.svg");
            var wideMarginsIconRandomStream = wideMarginsIconStream.AsRandomAccessStream();
            var wideMarginsIconReference = RandomAccessStreamReference.CreateFromStream(wideMarginsIconRandomStream);
            wideMarginsIconTask = wideMarginsIconReference.OpenReadAsync().AsTask();

            var moderateMarginsIconReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///P42.Uno.HtmlExtensions/Assets/moderateMargins.svg"));
            moderateMarginsIconTask = moderateMarginsIconReference.OpenReadAsync().AsTask();

            var narrowMarginsIconReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///P42.Uno.HtmlExtensions/Assets/narrowMargins.svg"));
            narrowMarginsIconTask = narrowMarginsIconReference.OpenReadAsync().AsTask();
            */
        }


        /// <summary>
        /// This function unregisters the app for printing with Windows.
        /// </summary>
        public virtual void UnregisterForPrinting()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (printDocument != null)
                {
                    printDocument.Paginate -= CreatePrintPreviewPages;
                    printDocument.GetPreviewPage -= GetPrintPreviewPage;
                    printDocument.AddPages -= AddPrintPages;

                    // Remove the handler for printing initialization.
                    var printMan = PrintManager.GetForCurrentView();
                    printMan.PrintTaskRequested -= PrintTaskRequested;
                }

                if (PrintCanvas != null)
                {
                    PrintCanvas.Children.Clear();
                    RootPanel.Children.Remove(PrintCanvas);
                }
                if (PrintContent != null)
                {
                    RootPanel.Children.Remove(PrintContent);
                }
                if (PrintSpinner != null)
                    RootPanel.Children.Remove(PrintSpinner);

                printDocument = null;
                printDocumentSource = null;
                printPreviewPages?.Clear();
                printPreviewPages = null;

                var total = GC.GetTotalMemory(true);
                GC.Collect();
            });

            GC.Collect();
        }

        public string JobName { get; private set; }


        /// <summary>
        /// Method that will generate print content for the scenario
        /// For scenarios 1-4: it will create the first page from which content will flow
        /// Scenario 5 uses a different approach
        /// </summary>
        /// <param name="page">The page to print</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task InitAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            GC.Collect();

            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            var printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTaskRequested;
        }


        /// <summary>
        /// This is the event handler for PrintManager.PrintTaskRequested.
        /// </summary>
        /// <param name="sender">PrintManager</param>
        /// <param name="e">PrintTaskRequestedEventArgs </param>
        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;
            printTask = e.Request.CreatePrintTask(JobName, sourceRequestedArgs =>
            {
                var deferral = sourceRequestedArgs.GetDeferral();

                var printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(printTask.Options);
                var displayedOptions = printTask.Options.DisplayedOptions;
                displayedOptions.Clear();
                displayedOptions.Add(StandardPrintTaskOptions.Copies);
                displayedOptions.Add(StandardPrintTaskOptions.Orientation);
                displayedOptions.Add(StandardPrintTaskOptions.PrintQuality);
                displayedOptions.Add(StandardPrintTaskOptions.MediaSize);
                displayedOptions.Add(StandardPrintTaskOptions.Collation);
                displayedOptions.Add(StandardPrintTaskOptions.Duplex);

                // Preset the default value of the printer option
                printTask.Options.MediaSize = PrintMediaSize.NorthAmericaLetter;

                // Create a new list option
                //var margins = printDetailedOptions.CreateItemListOption("Margins", "Margins");
                /*
                if (Forms9Patch.OsInfoService.Version >= new Version(10, 0, 17134, 0))
                {
                    margins.AddItem("WideMargins", "Wide", "Each margin is 20% of the paper size", await wideMarginsIconTask);
                    margins.AddItem("ModerateMargins", "Moderate", "Each margin is 10% of the paper size", await moderateMarginsIconTask);
                    margins.AddItem("NarrowMargins", "Narrow", "Each margin is 5% of the paper size", await narrowMarginsIconTask);
                    // App tells the user some more information about what the feature means.
                    margins.Description = "The space between the content of your document and the edge of the paper";
                }
                else
                */
                /*
                {
                    margins.AddItem("WideMargins", "Wide");
                    margins.AddItem("ModerateMargins", "Moderate");
                    margins.AddItem("NarrowMargins", "Narrow");
                }
                // The default is ModerateMargins
                */

                ApplicationContentMarginTop = 0.1;
                ApplicationContentMarginLeft = 0.1;
                //margins.TrySetValue("ModerateMargins");
                //displayedOptions.Add("Margins");


                // Add the custom option to the option list

                printDetailedOptions.OptionChanged += OnPrintDetailOptionChangedAsync;

                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += PrintTask_Completed;

                void PrintTask_Completed(PrintTask task, PrintTaskCompletedEventArgs args)
                {
                    UnregisterForPrinting();
                    task.Completed -= PrintTask_Completed;
                };

                sourceRequestedArgs.SetSource(printDocumentSource);
                
                deferral.Complete();
            });
        }




#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async void OnPrintDetailOptionChangedAsync(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var invalidatePreview = false;

            var optionId = args.OptionId as string;
            if (string.IsNullOrEmpty(optionId))
            {
                return;
            }

            if (optionId == "Margins")
            {
                var marginsOption = (PrintCustomItemListOptionDetails)sender.Options["Margins"];
                var marginsValue = marginsOption.Value.ToString();

                switch (marginsValue)
                {
                    case "WideMargins":
                        ApplicationContentMarginTop = 0.2;
                        ApplicationContentMarginLeft = 0.2;
                        break;
                    case "ModerateMargins":
                        ApplicationContentMarginTop = 0.1;
                        ApplicationContentMarginLeft = 0.1;
                        break;
                    case "NarrowMargins":
                        ApplicationContentMarginTop = 0.05;
                        ApplicationContentMarginLeft = 0.05;
                        break;
                    default:
                        break;
                }
                /*
                if (Forms9Patch.OsInfoService.Version >= new Version(10, 0, 17134, 0))
                {
                    if (marginsValue == "NarrowMargins")
                    {
                        marginsOption.WarningText = "Narrow margins may not be supported by some printers";
                    }
                    else
                    {
                        marginsOption.WarningText = "";
                    }
                }
                */
                invalidatePreview = true;
            }

            if (invalidatePreview)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    printDocument.InvalidatePreview();
                });
            }
        }

        bool _generatingPreviewPages;

        /// <summary>
        /// This is the event handler for PrintDocument.Paginate. It creates print preview pages for the app.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Paginate Event  </param>
        public virtual async void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            _generatingPreviewPages = true;
            var paperSize = e.PrintTaskOptions.GetPageDescription(0).PageSize;
            System.Diagnostics.Debug.WriteLine("CreatePrintPreviewPages: {" + paperSize.Width + "," + paperSize.Height + "}");

            // Clear the cache of preview pages
            printPreviewPages.Clear();
            // Clear the print canvas of preview pages
            PrintCanvas.Children.Clear();

            // Get the PrintTaskOptions
            var printingOptions = e.PrintTaskOptions;
            // Get the page description to deterimine how big the page is
            var pageDescription = printingOptions.GetPageDescription(0);

            if (await GeneratePagesAsync(pageDescription) is List<UIElement> pages)
            {
                PrintCanvas.InvalidateMeasure();
                PrintCanvas.UpdateLayout();

                await Task.Delay(1000);
            }

            // Report the number of preview pages created
            var printDoc = (PrintDocument)sender;
            printDoc.SetPreviewPageCount(printPreviewPages.Count, PreviewPageCountType.Intermediate);

            _generatingPreviewPages = false;
        }





        /// <summary>
        /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print preview page,
        /// in the form of an UIElement, to an instance of PrintDocument. PrintDocument subsequently converts the UIElement
        /// into a page that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Arguments containing the preview requested page</param>
        protected virtual async void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("GetPrintPreviewPage: " + e.PageNumber);

            while (_generatingPreviewPages)
                await Task.Delay(100);

            GC.Collect();

            var printDoc = (PrintDocument)sender;
            printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);
        }

        /// <summary>
        /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
        /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
        /// into a pages that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Add page event arguments containing a print task options reference</param>
        protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            var paperSize = e.PrintTaskOptions.GetPageDescription(0).PageSize;
            System.Diagnostics.Debug.WriteLine("AddPrintPages: {" + paperSize.Width + "," + paperSize.Height + "}");

            // Loop over all of the preview pages and add each one to  add each page to be printied
            for (int i = 0; i < printPreviewPages.Count; i++)
            {
                // We should have all pages ready at this point...
                printDocument.AddPage(printPreviewPages[i]);
            }
            
            var printDoc = (PrintDocument)sender;
            
            // Indicate that all of the print pages have been provided
            printDoc.AddPagesComplete();
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task<IEnumerable<UIElement>> GeneratePagesAsync(PrintPageDescription pageDescription)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }
 
    }
}
