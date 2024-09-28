﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="ClinicalNotes.Home" %>

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
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Clinical Notes : </h1>
            <button id="btnDownloadPDF" runat="server" class="btn btn-primary mt-3" onserverclick="btnDownloadPDF_ServerClick">Download PDF</button>
            <!--- <table id="instructionTable" class="display">
                <tbody>
                    <tr>
                        <td>Case Ref No:</td>
                        <td>@PatientInfo.CaseRefNo</td>
                        <td>Requested Session:</td>
                        <td>Initial Assessment + 6</td>
                    </tr>
                    <tr>
                        <td>Name:</td>
                        <td>@PatientInfo.Name</td>
                        <td>DOB:</td>
                        <td>23/05/2002</td>
                    </tr>
                    <tr>
                        <td>Login Date:</td>
                        <td>11/06/2024</td>
                        <td>DOI:</td>
                        <td>02/05/2024</td>
                    </tr>
                    <tr>
                        <td>Address:</td>
                        <td colspan="3">1 Goldwell Lane, Aldington, Ashford, Kent TN25 7DX</td>
                    </tr>
                </tbody>
            </table> -->

            <table id="instructionTable" class="display">
                <tbody>
                    <tr>
                        <td>Case Ref No:</td>
                        <td id="caseRefNo"></td>
                        <td>Requested Session:</td>
                        <td id="requestedSession"></td>
                    </tr>
                    <tr>
                        <td>Name:</td>
                        <td id="name"></td>
                        <td>DOB:</td>
                        <td id="dob"></td>
                    </tr>
                    <tr>
                        <td>Login Date:</td>
                        <td id="loginDate"></td>
                        <td>DOI:</td>
                        <td id="doi"></td>
                    </tr>
                    <tr>
                        <td>Address:</td>
                        <td id="address" colspan="3"></td>
                    </tr>
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

            <script>
                $(document).ready(function () {
                    
                    $('#instructionTable').DataTable({
                        paging: false,
                        searching: false,
                        ordering: false,
                        info: false
                    });
                    var patientData = <%= new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(patients) %>;

                    populatePatientInfo(patientData);
                    populateClinicalNotes(patientData.ClinicalNotes);

                });

                function populatePatientInfo(data) {
                    
                    $("#caseRefNo").text(data.CaseRefNo);
                    $("#name").text(data.Name);
                    $("#dob").text(new Date(data.DOB).toLocaleDateString());
                    $("#loginDate").text(new Date(data.LoginDate).toLocaleDateString());
                    $("#doi").text(new Date(data.DOI).toLocaleDateString());
                    $("#address").text(data.Address);
                }

                function populateClinicalNotes(clinicalNotes) {
                    
                    clinicalNotes.forEach(function (note) {
                        
                        var noteHtml = `
                            <div class="clinical-note-header">${new Date(note.Date).toLocaleDateString()} ----</div>
                            <div class="section-title">Subjective</div>
                            <ul>
                                <li>${note.Subjective}</li>
                            </ul>
                            <div class="section-title">Objective</div>
                            <ul>
                                <li>${note.Objective}</li>
                            </ul>
                            <div class="section-title">Assessment</div>
                            <ul>
                                <li>${note.Assessment}</li>
                            </ul>
                            <div class="section-title">Plan</div>
                            <ul>
                                <li>${note.Plan}</li>
                            </ul>`;
                        $(".card-body").append(noteHtml);
                    });
                }
            </script>



        </div>
    </form>
</body>
</html>
