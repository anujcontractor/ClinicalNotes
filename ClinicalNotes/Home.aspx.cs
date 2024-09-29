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
        
        protected void Page_Load(object sender, EventArgs e)
        {
            
                List<PatientInfo> patients = GetPatientInfos(); 
            
        }

        public List<PatientInfo> GetPatientInfos()
        {
            List<PatientInfo> patients = new List<PatientInfo>();
            try
            {
                String connect = "Data Source=.\\sqlexpress;Initial Catalog=mydatabase;Integrated Security=True;Encrypt=False";
                using (SqlConnection connection = new SqlConnection(connect))
                {
                    connection.Open();
                    String sql = "SELECT * FROM PatientInfo";
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
            return patients;
        }

        public List<ClinicalNotes> GetClinicalNotesForPatient(int caseRefNo, SqlConnection connection)
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
            string connect = "Data Source=.\\sqlexpress;Initial Catalog=mydatabase;Integrated Security=True;Encrypt=False";  
            int patientCaseRefNo = 18090;  

            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

                string patientQuery = "SELECT CaseRefNo, Name, RequestedSession, DOB, LoginDate, DOI, Address FROM PatientInfo WHERE CaseRefNo = @CaseRefNo";
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

                string notesQuery = "SELECT * FROM ClinicalNotes WHERE CaseRefNo = @CaseRefNo";
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

                    Paragraph date = new Paragraph(Convert.ToDateTime(notesReader["Date"]).ToString("dd/MM/yyyy"), dataFont);
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
    }
}