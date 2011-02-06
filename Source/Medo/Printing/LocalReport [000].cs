//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2009-10-27: Initial version.


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using System.Xml;


namespace Medo.Printing {

    /// <summary>
    /// Helper class for mananing printing tasks of Microsoft's local report.
    /// </summary>
    public class LocalReport : IDisposable {

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="reportPath">File system path of the local report.</param>
        /// <param name="reportDataSources">Data sources used by the report.</param>
        /// <param name="reportParameters">Report parameter properties for the local report.</param>
        public LocalReport(string reportPath, IList<ReportDataSource> dataSources, IList<ReportParameter> parameters) {
            this.ReportPath = reportPath;
            this.DataSources = dataSources;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="reportPath">File system path of the local report.</param>
        /// <param name="reportDataSource">Data source used by the report.</param>
        /// <param name="reportParameters">Report parameter properties for the local report.</param>
        public LocalReport(string reportPath, ReportDataSource dataSource, params ReportParameter[] parameters)
            : this(reportPath, new ReportDataSource[] { dataSource }, parameters) {
        }


        /// <summary>
        /// Indicates whether the Export button is visible on the control.
        /// </summary>
        public bool ShowExportButton { get; set; }

        /// <summary>
        /// Gets or sets the document name to display (for example, in a print status dialog box or printer queue) while printing the document.
        /// </summary>
        public string DocumentName { get; set; }

        /// <summary>
        /// Gets file system path of the local report.
        /// </summary>
        public string ReportPath { get; private set; }

        /// <summary>
        /// Gets data sources used by the report.
        /// </summary>
        public IList<ReportDataSource> DataSources { get; private set; }

        /// <summary>
        /// Gets report parameter properties for the local report.
        /// </summary>
        public IList<ReportParameter> Parameters { get; private set; }


        /// <summary>
        /// Shows modal dialog.
        /// </summary>
        /// <param name="owner">Any object that implements System.Windows.Forms.IWin32Window that represents the top-level window that will own the modal dialog box.</param>
        public DialogResult ShowPrintPreviewDialog(IWin32Window owner) {
            using (var form = new Form()) {
                Form ownerForm = owner as Form;
                if (ownerForm != null) {
                    form.Font = ownerForm.Font;
                    form.Icon = ownerForm.Icon;
                } else {
                    form.Font = SystemFonts.MessageBoxFont;
                    form.Icon = null;
                }
                form.MinimizeBox = false;
                form.ShowIcon = true;
                form.ShowInTaskbar = false;
                form.Text = this.DocumentName ?? Translations.DefaultTitle;
                if (owner != null) {
                    form.StartPosition = FormStartPosition.CenterParent;
                } else {
                    form.StartPosition = FormStartPosition.CenterScreen;
                }
                form.WindowState = FormWindowState.Maximized;

                var view = new ReportViewer();
                view.BorderStyle = BorderStyle.None;
                view.Dock = DockStyle.Fill;

                view.ShowDocumentMapButton = false;
                view.ShowBackButton = false;
                view.ShowStopButton = false;
                view.ShowRefreshButton = false;
                view.ShowExportButton = this.ShowExportButton;
                view.ZoomMode = ZoomMode.FullPage;
                view.ShowFindControls = false;

                view.ProcessingMode = ProcessingMode.Local;
                view.LocalReport.ReportPath = this.ReportPath;
                view.LocalReport.DisplayName = this.DocumentName;

                if (this.DataSources != null) {
                    foreach (var iDataSource in this.DataSources) {
                        view.LocalReport.DataSources.Add(iDataSource);
                    }
                }
                if (this.Parameters != null) {
                    view.LocalReport.SetParameters(this.Parameters);
                }

                view.Messages = Translations.Instance;

                view.SetDisplayMode(DisplayMode.PrintLayout);

                form.Controls.Add(view);

                return form.ShowDialog(owner);
            }
        }


        /// <summary>
        /// Exports document to PDF.
        /// </summary>
        /// <param name="fileName">File name for exported document.</param>
        public void ExportToPortableDocumentFormat(string fileName) {
            var report = CreateLocalReportForRendering();

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string extension;
            byte[] bytes = report.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None)) {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }

        /// <summary>
        /// Exports document to Microsoft Excel file format.
        /// </summary>
        /// <param name="fileName">File name for exported document.</param>
        public void ExportToExcel(string fileName) {
            var report = CreateLocalReportForRendering();

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string extension;
            byte[] bytes = report.Render("Excel", null, out mimeType, out encoding, out extension, out streamids, out warnings);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None)) {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }

        /// <summary>
        /// Starts the document's printing process.
        /// </summary>
        public void Print() {
            var report = CreateLocalReportForRendering();

            string pageWidth = "8.27in";
            string pageHeight = "11.69in";
            string marginLeft = "0.5in";
            string marginRight = "0.5in";
            string marginTop = "0.5in";
            string marginBottom = "0.5in";

            FillPageLayoutElements(ref pageWidth, ref pageHeight, ref marginLeft, ref marginRight, ref marginTop, ref marginBottom);

            string deviceInfo = string.Format(CultureInfo.InvariantCulture,
                "<DeviceInfo>" +
                    "<OutputFormat>EMF</OutputFormat>" +
                    "<DpiX>600</DpiX>" +
                    "<DpiY>600</DpiY>" +
                    "<PageWidth>{0}</PageWidth>" +
                    "<PageHeight>{1}</PageHeight>" +
                    "<MarginLeft>{2}</MarginLeft>" +
                    "<MarginRight>{3}</MarginRight>" +
                    "<MarginTop>{4}</MarginTop>" +
                    "<MarginBottom>{5}</MarginBottom>" +
                "</DeviceInfo>", pageWidth, pageHeight, marginLeft, marginRight, marginTop, marginBottom);

            Warning[] warnings;
            this._reportDocumentStreams = new List<Stream>();
            report.Render("IMAGE", deviceInfo, CreatePrinterStream, out warnings);

            if (this._reportDocumentStreams.Count > 0) {
                foreach (Stream iStream in this._reportDocumentStreams) {
                    iStream.Position = 0;
                }

                var reportDocument = new PrintDocument();
                reportDocument.DocumentName = this.DocumentName;
                reportDocument.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50); //TODO
                reportDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169); //TODO
                reportDocument.OriginAtMargins = false;
                reportDocument.BeginPrint += new PrintEventHandler(reportDocument_BeginPrint);
                reportDocument.PrintPage += new PrintPageEventHandler(reportDocument_PrintPage);
                reportDocument.PrintController = new StandardPrintController(); //to remove dialog with page counting
                reportDocument.Print();
            }
        }

        private void FillPageLayoutElements(ref string pageWidth, ref string pageHeight, ref string marginLeft, ref string marginRight, ref string marginTop, ref string marginBottom) {
            using (XmlTextReader xr = new XmlTextReader(this.ReportPath)) {
                Stack<string> walk = new Stack<string>();

                while (xr.Read()) {
                    if (xr.NodeType == XmlNodeType.Element) {
                        switch (xr.Name) {
                            case "Report": {
                                    if (walk.Count > 0) { break; }
                                    if (!xr.IsEmptyElement) { walk.Push(xr.Name); }
                                }
                                break;
                            case "PageWidth": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    pageWidth = xr.ReadString();
                                }
                                break;
                            case "PageHeight": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    pageHeight = xr.ReadString();
                                }
                                break;
                            case "LeftMargin": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    marginLeft = xr.ReadString();
                                }
                                break;
                            case "RightMargin": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    marginRight = xr.ReadString();
                                }
                                break;
                            case "TopMargin": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    marginTop = xr.ReadString();
                                }
                                break;
                            case "BottomMargin": {
                                    if (walk.Peek() != "Report") { break; }
                                    xr.MoveToContent();
                                    marginBottom = xr.ReadString();
                                }
                                break;
                        }

                    } else if (xr.NodeType == XmlNodeType.EndElement) {

                        switch (xr.Name) {

                            case "Report": {
                                    walk.Pop();
                                    break;
                                }

                        }

                    }

                } //while
            }
        }

        private Stream CreatePrinterStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek) {
            Stream stream = new FileStream(name + "." + fileNameExtension, FileMode.Create);
            this._reportDocumentStreams.Add(stream);
            return stream;
        }

        private IList<Stream> _reportDocumentStreams;
        private int _reportDocumentStreamIndex;

        private void reportDocument_BeginPrint(object sender, PrintEventArgs e) {
            this._reportDocumentStreamIndex = 0;
        }

        private void reportDocument_PrintPage(object sender, PrintPageEventArgs e) {
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.None;

            Metafile pageImage = new Metafile(this._reportDocumentStreams[this._reportDocumentStreamIndex]);
            e.Graphics.DrawImage(pageImage, e.PageBounds);

            this._reportDocumentStreamIndex += 1;
            e.HasMorePages = (this._reportDocumentStreamIndex < this._reportDocumentStreams.Count);
        }


        private Microsoft.Reporting.WinForms.LocalReport CreateLocalReportForRendering() {
            var report = new Microsoft.Reporting.WinForms.LocalReport();
            report.ReportPath = this.ReportPath;
            report.DisplayName = this.DocumentName;
            if (this.DataSources != null) {
                foreach (var iDataSource in this.DataSources) {
                    report.DataSources.Add(iDataSource);
                }
            }
            if (this.Parameters != null) {
                report.SetParameters(this.Parameters);
            }
            return report;
        }



        #region IDisposable Members

        /// <summary>
        /// Releases used resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (this._reportDocumentStreams != null) {
                    this._reportDocumentStreams.Clear();
                    this._reportDocumentStreams = null;
                }
            }
        }

        /// <summary>
        /// Releases used resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        #endregion




        private class Translations : IReportViewerMessages {

            private static Translations _instance = new Translations();
            public static Translations Instance {
                get { return _instance; }
            }


            private Translations() {
            }


            public static string DefaultTitle {
                get { return GetInCurrentLanguage("Print preview", "Pregled izvještaja"); }
            }


            #region IReportViewerMessages Members

            public string BackButtonToolTip { get { return ""; } }

            public string BackMenuItemText { get { return ""; } }

            public string ChangeCredentialsText { get { return ""; } }

            public string CurrentPageTextBoxToolTip {
                get { return GetInCurrentLanguage("Current page", "Trenutna stranica"); }
            }

            public string DocumentMapButtonToolTip { get { return ""; } }

            public string DocumentMapMenuItemText { get { return ""; } }

            public string ExportButtonToolTip {
                get { return GetInCurrentLanguage("Export", "Izvoz"); }
            }

            public string ExportMenuItemText {
                get { return GetInCurrentLanguage("&Export", "I&zvoz"); }
            }

            public string FalseValueText { get { return ""; } }

            public string FindButtonText { get { return ""; } }

            public string FindButtonToolTip { get { return ""; } }

            public string FindNextButtonText { get { return ""; } }

            public string FindNextButtonToolTip { get { return ""; } }

            public string FirstPageButtonToolTip {
                get { return GetInCurrentLanguage("First page", "Prva stranica"); }
            }

            public string LastPageButtonToolTip {
                get { return GetInCurrentLanguage("Last page", "Posljednja stranica"); }
            }

            public string NextPageButtonToolTip {
                get { return GetInCurrentLanguage("Next page", "Slijedeća stranica"); }
            }

            public string NoMoreMatches { get { return ""; } }

            public string NullCheckBoxText { get { return ""; } }

            public string NullCheckBoxToolTip { get { return ""; } }

            public string NullValueText { get { return ""; } }

            public string PageOf {
                get { return GetInCurrentLanguage("of", "od"); }
            }

            public string PageSetupButtonToolTip {
                get { return GetInCurrentLanguage("Page setup", "Postavke stranice"); }
            }

            public string PageSetupMenuItemText {
                get { return GetInCurrentLanguage("Page &setup", "Postavke &stranice"); }
            }

            public string ParameterAreaButtonToolTip { get { return ""; } }

            public string PasswordPrompt { get { return ""; } }

            public string PreviousPageButtonToolTip {
                get { return GetInCurrentLanguage("Previous page", "Prethodna stranica"); }
            }

            public string PrintButtonToolTip {
                get { return GetInCurrentLanguage("Print", "Ispis"); }
            }

            public string PrintLayoutButtonToolTip {
                get { return GetInCurrentLanguage("Print layout", "Postavke ispisa"); }
            }

            public string PrintLayoutMenuItemText {
                get { return GetInCurrentLanguage("Print &layout", "&Postavke ispisa"); }
            }

            public string PrintMenuItemText {
                get { return GetInCurrentLanguage("&Print", "&Ispis"); }
            }

            public string ProgressText {
                get { return GetInCurrentLanguage("Report is being generated", "Izvještaj se trenutno priprema"); }
            }

            public string RefreshButtonToolTip { get { return ""; } }

            public string RefreshMenuItemText { get { return ""; } }

            public string SearchTextBoxToolTip { get { return ""; } }

            public string SelectAValue { get { return ""; } }

            public string SelectAll { get { return ""; } }

            public string StopButtonToolTip { get { return ""; } }

            public string StopMenuItemText { get { return ""; } }

            public string TextNotFound { get { return ""; } }

            public string TotalPagesToolTip {
                get { return GetInCurrentLanguage("Total pages", "Ukupno stranica"); }
            }

            public string TrueValueText { get { return ""; } }

            public string UserNamePrompt { get { return ""; } }

            public string ViewReportButtonText { get { return ""; } }

            public string ViewReportButtonToolTip { get { return ""; } }

            public string ZoomControlToolTip {
                get { return GetInCurrentLanguage("Zoom", "Povećanje"); }
            }

            public string ZoomMenuItemText {
                get { return GetInCurrentLanguage("&Zoom", "Po&većanje"); }
            }

            public string ZoomToPageWidth {
                get { return GetInCurrentLanguage("Page width", "Širina stranice"); }
            }

            public string ZoomToWholePage {
                get { return GetInCurrentLanguage("Whole page", "Cijela stranica"); }
            }


            private static string GetInCurrentLanguage(string en_US, string hr_HR) {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToUpperInvariant()) {
                    case "EN":
                    case "EN-US":
                    case "EN-GB":
                        return en_US;

                    case "HR":
                    case "HR-HR":
                    case "HR-BA":
                        return hr_HR;

                    default:
                        return en_US;
                }

            #endregion

            }

        }

    }
}