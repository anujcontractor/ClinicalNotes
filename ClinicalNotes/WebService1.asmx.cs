using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ClinicalNotes
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://clinicalnotes.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public List<PatientInfo> GetPatientInfos()
        {
            List<PatientInfo> patients = new List<PatientInfo>();
            try
            {
                String connect = "Data Source=.\\sqlexpress;Initial Catalog=mydatabase;Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True";
                using (SqlConnection connection = new SqlConnection(connect))
                {
                    connection.Open();
                    String sql = "SELECT * FROM PatientInfo WHERE CaseRefNo = 18090";
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
                throw;
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
    }
}
