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
    <script>
        function downloadPDF(patientCaseRefNo) {
            event.preventDefault();
            $.ajax({
                type: "POST",
                url: "https://localhost:44316/WebService1.asmx/btnDownloadPDF_ServerClick", 
                data: JSON.stringify({ patientCaseRefNo: patientCaseRefNo }), 
                contentType: "application/json; charset=utf-8",
                xhrFields: {
                    responseType: 'blob'  
                },
                success: function (response, status, xhr) {
                    
                    var blob = new Blob([response], { type: 'application/pdf' });

                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'ClinicalNote.pdf'; 

                    document.body.appendChild(link);

                    link.click();

                    document.body.removeChild(link);
                },
                error: function (xhr, status, error) {
                    console.log('Error downloading PDF:', xhr.status, error);
                    alert('Failed to download PDF. Status: ' + xhr.status);
                }
            });
        }

    </script>


</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Clinical Notes : </h1>
            <!--<button id="btnDownloadPDF" runat="server" class="btn btn-primary mt-3" onserverclick="btnDownloadPDF_ServerClick">Download PDF</button>-->
            <button id="Button1" onclick="downloadPDF(18090)" class="btn btn-primary mt-3" >Download PDF</button>


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
                    <!-- Data -->
                </tbody>
            </table>
        
            

            <script src="https://code.jquery.com/jquery-3.5.1.js"></script>
            <script type="text/javascript" src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>

            <script type="text/javascript">
                $(document).ready(function () {

                    $('#patientTable').DataTable({
                        paging: false,
                        searching: false,
                        ordering: false,
                        info: false
                    });

                    fetchPatientInfo();

                    function fetchPatientInfo() {
                        $.ajax({
                            type: "POST",
                            url: "https://localhost:44316/WebService1.asmx/GetPatientInfos",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: function (response) {
                                var patients = response.d; 
                                console.log(patients);
                                populatePatientTable(patients);
                            },
                            error: function (xhr, status, error) {
                                console.log('Error fetching patient data:', xhr.status, error);
                            }
                        });
                    }

                    function populatePatientTable(patients) {
                        var tableBody = $("#patientTableBody");
                        tableBody.empty(); 

                        patients.forEach(function (patient) {
                            
                            var row = `<tr>
                                <td>${patient.CaseRefNo}</td>
                                <td>${patient.Name}</td>
                                <td>${patient.RequestedSession}</td>
                                <td>${new Date(patient.DOB).toLocaleDateString()}</td>
                                <td>${new Date(patient.LoginDate).toLocaleDateString()}</td>
                                <td>${new Date(patient.DOI).toLocaleDateString()}</td>
                                <td>${patient.Address}</td>
                                <td>
                                    <button class="toggleNotes" data-caseref="${patient.CaseRefNo}">Show Notes</button>
                                </td>
                            </tr>`;

                            tableBody.append(row);

                            var clinicalNotesRow = `<tr class="notesRow" id="notes-${patient.CaseRefNo}" style="display:none;">
                                <td colspan="8">
                                    <table class="notesTable">
                                        <thead>
                                            <tr>
                                                <th>Note ID</th>
                                                <th>Date</th>
                                                <th>Subjective</th>
                                                <th>Objective</th>
                                                <th>Assessment</th>
                                                <th>Plan</th>
                                            </tr>
                                        </thead>
                                        <tbody>`;

                            patient.ClinicalNotes.forEach(function (note) {
                                clinicalNotesRow += `<tr>
                                    <td>${note.NoteID}</td>
                                    <td>${new Date(parseInt(note.Date.substr(6))).toLocaleDateString()}</td>
                                    <td>${note.Subjective}</td>
                                    <td>${note.Objective}</td>
                                    <td>${note.Assessment}</td>
                                    <td>${note.Plan}</td>
                                </tr>`;
                            });

                            clinicalNotesRow += `</tbody></table></td></tr>`;

                            tableBody.append(clinicalNotesRow);
                        });

                        $(document).on('click', '.toggleNotes', function (event) {
                            event.preventDefault(); 

                            var caseRefNo = $(this).data('caseref');
                            var notesRow = $('#notes-' + caseRefNo);
                            notesRow.toggle(); 

                            if (notesRow.is(':visible')) {
                                $(this).text('Hide Notes'); 
                            } else {
                                $(this).text('Show Notes'); 
                            }
                        });


                        
                    }
                });
            </script>



        </div>
    </form>
</body>
</html>
