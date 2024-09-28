using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec.wmf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

namespace ClinicalNotes
{

    
    public class PatientInfo
    {
        public int CaseRefNo { get; set; }
        public string Name { get; set; }
        public string RequestedSession { get; set; }
        public DateTime DOB { get; set; }
        public DateTime LoginDate { get; set; }
        public DateTime DOI { get; set; }
        public string Address { get; set; }
        public List<ClinicalNotes> ClinicalNotes { get; set; }

        public PatientInfo()
        {
            ClinicalNotes = new List<ClinicalNotes>();
        }
    }

    public class ClinicalNotes
    {
        public int NoteID { get; set; }
        public int CaseRefNo { get; set; } 
        public DateTime Date { get; set; }
        public string Subjective { get; set; }
        public string Objective { get; set; }
        public string Assessment { get; set; }
        public string Plan { get; set; }

    }

    public class TwoLineFooter : PdfPageEventHelper
    {
        private string line1;
        private string line2;

        public TwoLineFooter(string line1, string line2)
        {
            this.line1 = line1;
            this.line2 = line2;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetFontAndSize(bf, 10);

            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line1,
                (document.Right + document.Left) / 2, document.Bottom - 5, 0);
            cb.EndText();

            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line2,
                (document.Right + document.Left) / 2, document.Bottom - 20, 0);
            cb.EndText();
        }
    }


    public partial class Home : System.Web.UI.Page
    {
        public List<PatientInfo> patients = new List<PatientInfo>();
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                String connect = "";
                using (SqlConnection connection = new SqlConnection(connect))
                {
                    connection.Open();
                    String sql = "SELECT * FROM patients";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PatientInfo patient = new PatientInfo();
                                patient.CaseRefNo = reader.GetInt32(0);
                                patient.Name = reader.GetString(1);
                                patient.RequestedSession = reader.GetString(2);
                                patient.DOB = reader.GetDateTime(3);
                                patient.LoginDate = reader.GetDateTime(4);
                                patient.DOI = reader.GetDateTime(5);
                                patient.Address = reader.GetString(6);

                                patient.ClinicalNotes = GetClinicalNotesForPatient(patient.CaseRefNo, connection);
                                patients.Add(patient);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.ToString());
            }
        }

        private List<ClinicalNotes> GetClinicalNotesForPatient(int caseRefNo, SqlConnection connection)
        {
            List<ClinicalNotes> clinicalNotesList = new List<ClinicalNotes>();

            String notesSql = "SELECT * FROM ClinicalNotes WHERE CaseRefNo = @CaseRefNo";
            using (SqlCommand notesCommand = new SqlCommand(notesSql, connection))
            {
                notesCommand.Parameters.AddWithValue("@CaseRefNo", caseRefNo);

                using (SqlDataReader notesReader = notesCommand.ExecuteReader())
                {
                    while (notesReader.Read())
                    {
                        ClinicalNotes notes = new ClinicalNotes();
                        notes.NoteID = notesReader.GetInt32(0);
                        notes.CaseRefNo = notesReader.GetInt32(1);
                        notes.Date = notesReader.GetDateTime(2);
                        notes.Subjective = notesReader.GetString(3);
                        notes.Objective = notesReader.GetString(4);
                        notes.Assessment = notesReader.GetString(5);
                        notes.Plan = notesReader.GetString(6);

                        clinicalNotesList.Add(notes);
                    }
                }
            }

            return clinicalNotesList;
        }
        protected void btnDownloadPDF_ServerClick(object sender, EventArgs e)
        {
            string fileName = "ClinicalNote.pdf";

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            string footerLine1 = "15-19 Cavendish Place, 2nd Floor, London, England, W1G 0DD";
            string footerLine2 = "Email: enquiry@londonphysiotherapy.com";

            Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);

            TwoLineFooter eventFooter = new TwoLineFooter(footerLine1, footerLine2);
            writer.PageEvent = eventFooter;

            Font headerFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.BLACK);
            Font dataFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK);

            pdfDoc.Open();

            string logoPath = Server.MapPath("~/Assets/logo.jpg");
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
            logo.ScaleToFit(100f, 50f);
            logo.SetAbsolutePosition(25, pdfDoc.PageSize.Height - 80);
            pdfDoc.Add(logo);

            Font addressFont = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL, BaseColor.BLACK);
            Paragraph address = new Paragraph("15-19 Cavandish Place 2nd Floor\nLondon, England W1G 0DD\nenquiry@londonphysiotherapy.com", addressFont);
            address.Alignment = Element.ALIGN_RIGHT;
            pdfDoc.Add(address);

            pdfDoc.Add(new Paragraph("\n\n\n"));

            Font titleFont = new Font(Font.FontFamily.HELVETICA, 20, Font.BOLD | Font.UNDERLINE, BaseColor.BLACK);
            Paragraph title = new Paragraph("Clinical Notes", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(title);

            pdfDoc.Add(new Paragraph("\n\n"));

            // Fetch data from the database
            string connect = "";  
            int patientCaseRefNo = 18090;  

            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

                string patientQuery = "SELECT CaseRefNo, Name, RequestedSession, DOB, LoginDate, DOI, Address FROM Patients WHERE CaseRefNo = @CaseRefNo";
                SqlCommand patientCommand = new SqlCommand(patientQuery, connection);
                patientCommand.Parameters.AddWithValue("@CaseRefNo", patientCaseRefNo);

                SqlDataReader reader = patientCommand.ExecuteReader();

                if (reader.Read())
                {
                    PdfPTable table = new PdfPTable(4);
                    table.WidthPercentage = 100;
                    float[] columnWidths = { 1.5f, 3f, 1.5f, 3f };
                    table.SetWidths(columnWidths);

                    BaseColor headerColor = new BaseColor(192, 192, 192);  // Light gray

                    AddCellToTable(table, "Case Ref No:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, reader["CaseRefNo"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
                    AddCellToTable(table, "Requested Session:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, reader["RequestedSession"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

                    AddCellToTable(table, "Name:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, reader["Name"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
                    AddCellToTable(table, "DOB:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, Convert.ToDateTime(reader["DOB"]).ToString("dd/MM/yyyy"), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

                    AddCellToTable(table, "Login Date:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, Convert.ToDateTime(reader["LoginDate"]).ToString("dd/MM/yyyy"), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
                    AddCellToTable(table, "DOI:", headerFont, headerColor, Element.ALIGN_LEFT);
                    AddCellToTable(table, Convert.ToDateTime(reader["DOI"]).ToString("dd/MM/yyyy"), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

                    AddCellToTable(table, "Address:", headerFont, headerColor, Element.ALIGN_LEFT);
                    PdfPCell addressCell = new PdfPCell(new Phrase(reader["Address"].ToString(), dataFont));
                    addressCell.Colspan = 3;
                    addressCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    addressCell.Border = PdfPCell.BOX;
                    table.AddCell(addressCell);

                    pdfDoc.Add(table);
                }

                reader.Close();

                string notesQuery = "SELECT NoteDate, Subjective, Objective, Assessment, Plan FROM ClinicalNotes WHERE CaseRefNo = @CaseRefNo";
                SqlCommand notesCommand = new SqlCommand(notesQuery, connection);
                notesCommand.Parameters.AddWithValue("@CaseRefNo", patientCaseRefNo);

                SqlDataReader notesReader = notesCommand.ExecuteReader();

                Font sectionFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("\nClinical Notes:", sectionFont));

                while (notesReader.Read())
                {
                    PdfPTable notesTable = new PdfPTable(2);
                    notesTable.WidthPercentage = 100;
                    notesTable.SetWidths(new float[] { 0.5f, 2f });

                    Paragraph date = new Paragraph(Convert.ToDateTime(notesReader["NoteDate"]).ToString("dd/MM/yyyy"), dataFont);
                    pdfDoc.Add(date);

                    AddCellToTable(notesTable, "Subjective:", headerFont, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT);
                    AddCellToTable(notesTable, notesReader["Subjective"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
 
                    AddCellToTable(notesTable, "Objective:", headerFont, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT);
                    AddCellToTable(notesTable, notesReader["Objective"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
 
                    AddCellToTable(notesTable, "Assessment:", headerFont, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT);
                    AddCellToTable(notesTable, notesReader["Assessment"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

                    AddCellToTable(notesTable, "Plan:", headerFont, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT);
                    AddCellToTable(notesTable, notesReader["Plan"].ToString(), dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

                    pdfDoc.Add(notesTable);
                    pdfDoc.Add(new Paragraph("\n"));
                }

                notesReader.Close();
            }

            pdfDoc.Close();

            Response.Write(pdfDoc);
            Response.End();
        }

        private void AddCellToTable(PdfPTable table, string value, Font font, BaseColor backgroundColor, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(value, font));
            cell.HorizontalAlignment = alignment;
            cell.BackgroundColor = backgroundColor;
            table.AddCell(cell);
        }





        //protected void btnDownloadPDF_ServerClick(object sender, EventArgs e)
        //{
        //    // Define the file name
        //    string fileName = "ClinicalNote.pdf";

        //    // Set the HTTP headers to force download
        //    Response.ContentType = "application/pdf";
        //    Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);

        //    string footerLine1 = "15-19 Cavendish Place, 2nd Floor, London, England, W1G 0DD";
        //    string footerLine2 = "Email: enquiry@londonphysiotherapy.com";

        //    // Create a PDF document
        //    Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
        //    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);

        //    TwoLineFooter eventFooter = new TwoLineFooter(footerLine1, footerLine2);
        //    writer.PageEvent = eventFooter;

        //    pdfDoc.Open();

        //    string logoPath = Server.MapPath("~/Assets/logo.jpg"); // Update the path accordingly
        //    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
        //    logo.ScaleToFit(100f, 50f); // Adjust size
        //    logo.SetAbsolutePosition(25, pdfDoc.PageSize.Height - 80); // Adjust position for upper-left corner
        //    pdfDoc.Add(logo);

        //    // Add the Hospital Address to the upper right corner
        //    Font addressFont = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL, BaseColor.BLACK);
        //    Paragraph address = new Paragraph("15-19 Cavandish Place 2nd Floor\nLondon, England W1G 0DD\nenquiry@londonphysiotherapy.com", addressFont);
        //    address.Alignment = Element.ALIGN_RIGHT;
        //    pdfDoc.Add(address);

        //    pdfDoc.Add(new Paragraph("\n"));
        //    pdfDoc.Add(new Paragraph("\n"));
        //    pdfDoc.Add(new Paragraph("\n"));

        //    // Create a PdfContentByte to place the address at an absolute position
        //    // PdfContentByte cb = writer.DirectContent;
        //    // ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(address), pdfDoc.PageSize.Width - 25, pdfDoc.PageSize.Height - 50, 0);

        //    // Add the Title with style
        //    Font titleFont = new Font(Font.FontFamily.HELVETICA, 20, Font.BOLD | Font.UNDERLINE, BaseColor.BLACK);
        //    Paragraph title = new Paragraph("Clinical Notes", titleFont);
        //    title.Alignment = Element.ALIGN_CENTER;

        //    pdfDoc.Add(title);

        //    // Add a new line
        //    pdfDoc.Add(new Paragraph("\n"));
        //    pdfDoc.Add(new Paragraph("\n"));

        //    PdfPTable haedtable = new PdfPTable(4);
        //    haedtable.WidthPercentage = 100;

        //    // Set column widths
        //    float[] columnWidths = { 1.5f, 3f, 1.5f, 3f }; // Adjust column widths
        //    haedtable.SetWidths(columnWidths);

        //    // Define fonts
        //    Font headerFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.BLACK);
        //    Font dataFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK);

        //    // Define cell background color for headers
        //    BaseColor headerColor = new BaseColor(192, 192, 192); // Light gray

        //    // Add "Case Ref No:" and "Requested Session:"
        //    AddCellToTable(haedtable, "Case Ref No:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "18090", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "Requested Session:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "Initial Assessment + 6", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

        //    // Add "Name:" and "DOB:"
        //    AddCellToTable(haedtable, "Name:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "Mr. Owen Tobitt", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "DOB:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "23/05/2002", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

        //    // Add "Login Date:" and "DOI:"
        //    AddCellToTable(haedtable, "Login Date:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "11/06/2024", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "DOI:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    AddCellToTable(haedtable, "02/05/2024", dataFont, BaseColor.WHITE, Element.ALIGN_LEFT);

        //    // Add "Address:"
        //    AddCellToTable(haedtable, "Address:", headerFont, headerColor, Element.ALIGN_LEFT);
        //    PdfPCell addressCell = new PdfPCell(new Phrase("1 Goldwell Lane, Aldington, Ashford, Kent TN25 7DX", dataFont));
        //    addressCell.Colspan = 3;
        //    addressCell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    addressCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    addressCell.Border = PdfPCell.BOX;
        //    haedtable.AddCell(addressCell);

        //    // Add the table to the document
        //    pdfDoc.Add(haedtable);

        //    pdfDoc.Add(new Paragraph("\n"));

        //    // Define general font for the rest of the 
        //    Font textFont = new Font(Font.FontFamily.HELVETICA, 11, Font.NORMAL, BaseColor.BLACK);

        //    Paragraph date = new Paragraph("29/07/2024", textFont);
        //    pdfDoc.Add(date);

        //    pdfDoc.Add(new Paragraph("\n"));

        //    // Create a table with 2 columns for the structured data
        //    PdfPTable table = new PdfPTable(2);
        //    table.WidthPercentage = 100;
        //    table.SetWidths(new float[] { 0.5f, 2f });

        //    // Add Subjective Section
        //    PdfPCell cell = new PdfPCell(new Phrase("Subjective:", headerFont));
        //    cell.Border = PdfPCell.BOX;
        //    cell.BorderWidth = 1f;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    cell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(cell);

        //    List subjectiveList = new List(List.ORDERED);
        //    subjectiveList.IndentationLeft = 20f;
        //    subjectiveList.Add(new ListItem("Consented to review and treat virtually", textFont));
        //    subjectiveList.Add(new ListItem("Patient reports major pain and discomfort at shoulder, neck, and knee; moderate pain at lower back.", textFont));
        //    subjectiveList.Add(new ListItem("ADL like bending, lifting, carrying, driving, and sleeping improving on daily basis.", textFont));
        //    subjectiveList.Add(new ListItem("VAS same as last", textFont));
        //    subjectiveList.Add(new ListItem("Client feels slightly better than last week.", textFont));
        //    PdfPCell subjectiveCell = new PdfPCell();
        //    subjectiveCell.AddElement(subjectiveList);
        //    subjectiveCell.Border = PdfPCell.BOX;
        //    subjectiveCell.BorderWidth = 1f;
        //    subjectiveCell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(subjectiveCell);

        //    // Add Objective Section
        //    cell = new PdfPCell(new Phrase("Objective:", headerFont));
        //    cell.Border = PdfPCell.BOX;
        //    cell.BorderWidth = 1f;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    cell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(cell);

        //    List objectiveList = new List(List.ORDERED);
        //    objectiveList.IndentationLeft = 20f;
        //    objectiveList.Add(new ListItem("All SDs, red flags absent. No P+Ns, bruises, deformity reported.", textFont));
        //    objectiveList.Add(new ListItem("AROM - lower back - moderate pain and stiffness.", textFont));
        //    objectiveList.Add(new ListItem("AROM - Cervical spine - 70% EOR major pain.", textFont));
        //    objectiveList.Add(new ListItem("AROM - right shoulder - 80% EOR major pain.", textFont));
        //    objectiveList.Add(new ListItem("AROM - left shoulder - 80% EOR major pain.", textFont));
        //    objectiveList.Add(new ListItem("AROM - Right knee - 80% EOR major pain.", textFont));
        //    objectiveList.Add(new ListItem("AROM - left knee - 80% EOR major pain.", textFont));
        //    PdfPCell objectiveCell = new PdfPCell();
        //    objectiveCell.AddElement(objectiveList);
        //    objectiveCell.Border = PdfPCell.BOX;
        //    objectiveCell.BorderWidth = 1f;
        //    objectiveCell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(objectiveCell);

        //    // Add Assessment Section
        //    cell = new PdfPCell(new Phrase("Assessment:", headerFont));
        //    cell.Border = PdfPCell.BOX;
        //    cell.BorderWidth = 1f;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    cell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(cell);

        //    List assessmentList = new List(List.ORDERED);
        //    assessmentList.IndentationLeft = 20f;
        //    assessmentList.Add(new ListItem("Education about the condition", textFont));
        //    assessmentList.Add(new ListItem("Postural correction", textFont));
        //    assessmentList.Add(new ListItem("Advice", textFont));
        //    assessmentList.Add(new ListItem("Home exercises", textFont));
        //    assessmentList.Add(new ListItem("Hot and cold packs", textFont));
        //    PdfPCell assessmentCell = new PdfPCell();
        //    assessmentCell.AddElement(assessmentList);
        //    assessmentCell.Border = PdfPCell.BOX;
        //    assessmentCell.BorderWidth = 1f;
        //    assessmentCell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(assessmentCell);

        //    // Add Plan Section
        //    cell = new PdfPCell(new Phrase("Plan:", headerFont));
        //    cell.Border = PdfPCell.BOX;
        //    cell.BorderWidth = 1f;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    cell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(cell);

        //    List planlist = new List(List.ORDERED);
        //    planlist.IndentationLeft = 20f;
        //    planlist.Add(new ListItem("HEP + FU", textFont));
        //    PdfPCell planCell = new PdfPCell();
        //    planCell.AddElement(planlist);
        //    planCell.Border = PdfPCell.BOX;
        //    planCell.BorderWidth = 1f;
        //    planCell.BorderColor = BaseColor.BLACK;
        //    table.AddCell(planCell);
        //    pdfDoc.Add(table);

        //    Paragraph caption = new Paragraph("I believe the content of this report is accurate and completed to the best of my knowledge and belief.", textFont);
        //    pdfDoc.Add(caption);

        //    pdfDoc.Add(new Paragraph("\n"));


        //    Paragraph t1 = new Paragraph("Physiotherapist:", textFont);
        //    pdfDoc.Add(t1);

        //    Paragraph t2 = new Paragraph("HCPC Number:", textFont);
        //    pdfDoc.Add(t2);

        //    Paragraph t3 = new Paragraph("CSP Number:", textFont);
        //    pdfDoc.Add(t3);

        //    Paragraph t4 = new Paragraph("Signature:", textFont);
        //    pdfDoc.Add(t4);


        //    pdfDoc.Close();

        //    // Write the document to the output stream
        //    Response.Write(pdfDoc);
        //    Response.End();
        //}

        //private void AddCellToTable(PdfPTable haedtable, string value, Font dataFont, BaseColor backgroundColor, int alignment)
        //{
        //    PdfPCell cell = new PdfPCell(new Phrase(value, dataFont));

        //    // Set the alignment for the cell
        //    cell.HorizontalAlignment = alignment;

        //    // Set the background color for the cell
        //    cell.BackgroundColor = backgroundColor;

        //    // Add the cell to the table
        //    haedtable.AddCell(cell);
        //}

    }
}