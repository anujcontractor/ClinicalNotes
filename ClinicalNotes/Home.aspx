<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="ClinicalNotes.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="https://cdn.datatables.net/2.1.7/css/dataTables.dataTables.css" />
    <script src="https://cdn.datatables.net/2.1.7/js/dataTables.js"></script>
    <style>
        .section-title {
            font-weight: bold;
            margin-top: 10px;
        }
        .clinical-note-header {
            font-weight: bold;
            font-size: 18px;
            margin-bottom: 10px;
        }
    </style>
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <link rel="stylesheet" href="https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css" />
    <script src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Clinical Notes : </h1>
            <button id="btnDownloadPDF" runat="server" class="btn btn-primary mt-3" onserverclick="btnDownloadPDF_ServerClick">Download PDF</button>

           <table id="patientTable" class="display">
                <thead>
                    <tr>
                        <th>Case Ref No</th>
                        <th>Name</th>
                        <th>Requested Session</th>
                        <th>DOB</th>
                        <th>Login Date</th>
                        <th>DOI</th>
                        <th>Address</th>
                    </tr>
                </thead>
                <tbody id="patientTableBody">
                    <!-- Data will be dynamically inserted here -->
                </tbody>
            </table>
        
            <div class="row">
                <div class="col-md-12">
                    <div class="card">
                        <div class="card-body">
                            <div class="clinical-note-header">11/09/2024 ----</div>
                        
                       
                            <div class="section-title">Subjective</div>
                            <ul>
                                <li>Consented to review and treat virtually</li>
                                <li>Patient reports major pain and discomfort at shoulder, neck, and knee; moderate pain at lower back.</li>
                                <li>ADL like bending, lifting, carrying, driving and sleeping improving on daily basis</li>
                                <li>VAS same as last</li>
                                <li>Client feels slightly better than last week.</li>
                            </ul>

                        
                            <div class="section-title">Objective</div>
                            <ol>
                                <li>All SDs, red flags absent. No P+Ns, bruises, deformity reported.</li>
                                <li>AROM- lower back- moderate pain and stiffness</li>
                                <li>AROM – Cervical spine- 70% EOR major pain</li>
                                <li>AROM- right shoulder-80% EOR major pain</li>
                                <li>AROM- left shoulder-80% EOR major pain</li>
                                <li>AROM- Right knee- 80% EOR major pain</li>
                                <li>AROM- left knee- 80% EOR major pain</li>
                            </ol>

                           
                            <div class="section-title">Assessment</div>
                            <ol>
                                <li>Education about the condition</li>
                                <li>Postural correction</li>
                                <li>Advice,</li>
                                <li>Home exercises,</li>
                                <li>Hot and cold packs,</li>
                            </ol>

                            <div class="section-title">Plan</div>
                            <p>HEP + FU</p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <div class="card">
                        <div class="card-body">
                            <div class="clinical-note-header">18/09/2024 ----</div>
                        
                            <div class="section-title">Subjective</div>
                            <ul>
                                <li>Consented to review and treat virtually</li>
                                <li>Patient reports moderate pain and discomfort at lower back and minor pain at both knee.</li>
                                <li>ADL like bending, lifting, carrying, driving and sleeping improving on daily basis</li>
                                <li>Better, most pain and discomfort in neck.</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div> 

            <!-- <div id="clinicalNotesSection" class="row">
                < foreach (var note in patients.ClinicalNotes) { %>
                    <div class="col-md-12">
                        <div class="card">
                            <div class="card-body">
                                <div class="clinical-note-header"><= note.Date.ToShortDateString() %> ----</div>

                        
                                <div class="section-title">Subjective</div>
                                <ul>
                                    <li><= note.Subjective %></li>
                                </ul>

                 
                                <div class="section-title">Objective</div>
                                <ul>
                                    <li><= note.Objective %></li>
                                </ul>

                                <div class="section-title">Assessment</div>
                                <ul>
                                    <li><= note.Assessment %></li>
                                </ul>

                         
                                <div class="section-title">Plan</div>
                                <ul>
                                    <li><= note.Plan %></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                < } %>
            </div> -->

            <script src="https://code.jquery.com/jquery-3.5.1.js"></script>
            <script type="text/javascript" src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>

            <script type="text/javascript">
                $(document).ready(function () {

                    // Re-initialize the DataTable after inserting the rows
                    $('#patientTable').DataTable({
                        paging: false,
                        searching: false,
                        ordering: false,
                        info: false
                    });

                    // Call web service on page load
                    fetchPatientInfo();

                    // Function to call web service and fetch patient data
                    function fetchPatientInfo() {
                        $.ajax({
                            type: "POST",
                            url: "http://localhost:44316/WebService1.asmx/GetPatientInfos",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: function (response) {
                                var patients = response.d; // The patient data
                                console.log(patients);
                                populatePatientTable(patients);
                            },
                            error: function (xhr, status, error) {
                                console.log('Error fetching patient data:', xhr.status, error);
                            }
                        });
                    }

                    // Function to dynamically populate the patient table
                    function populatePatientTable(patients) {
                        var tableBody = $("#patientTableBody");
                        tableBody.empty(); // Clear any previous data

                        // Loop through each patient and create a table row
                        patients.forEach(function (patient) {
                            var row = `<tr>
                                <td>${patient.CaseRefNo}</td>
                                <td>${patient.Name}</td>
                                <td>${patient.RequestedSession}</td>
                                <td>${new Date(patient.DOB).toLocaleDateString()}</td>
                                <td>${new Date(patient.LoginDate).toLocaleDateString()}</td>
                                <td>${new Date(patient.DOI).toLocaleDateString()}</td>
                                <td>${patient.Address}</td>
                            </tr>`;
                            tableBody.append(row);
                        });

                        
                    }
                });
            </script>



        </div>
    </form>
</body>
</html>
