import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

function addHeader(doc, title, subtitle) {
  doc.setFontSize(18);
  doc.setFont('helvetica', 'bold');
  doc.text(title, 14, 22);
  doc.setFontSize(11);
  doc.setFont('helvetica', 'normal');
  doc.setTextColor(100);
  doc.text(subtitle, 14, 30);
  doc.setTextColor(0);
  doc.setDrawColor(200);
  doc.line(14, 34, 196, 34);
  return 40;
}

function addFooter(doc) {
  const pageHeight = doc.internal.pageSize.height;
  doc.setFontSize(8);
  doc.setTextColor(150);
  doc.text(
    `Page ${doc.internal.getCurrentPageInfo().pageNumber}`,
    doc.internal.pageSize.width / 2,
    pageHeight - 10,
    { align: 'center' }
  );
}

export function generateExamResultsPDF(studentName, admissionNumber, className, sectionName, results) {
  const doc = new jsPDF();
  let y = addHeader(doc, 'Exam Results', `Student: ${studentName} | Adm: ${admissionNumber} | ${className} - ${sectionName}`);

  if (!results || results.length === 0) {
    doc.setFontSize(12);
    doc.text('No exam results available.', 14, y + 10);
    doc.save('exam-results.pdf');
    return;
  }

  results.forEach((result) => {
    if (y > 240) {
      doc.addPage();
      y = 20;
    }

    doc.setFontSize(12);
    doc.setFont('helvetica', 'bold');
    doc.text(result.examName, 14, y);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.text(result.isPassed ? 'Passed' : 'Failed', 180, y);
    y += 6;

    const rows = (result.subjectResults || []).map((s) => [
      s.subjectName,
      `${s.marksObtained}`,
      `${s.maxMarks}`,
      `${s.passingMarks}`,
      s.isPass ? 'Pass' : 'Fail',
    ]);

    autoTable(doc, {
      startY: y,
      head: [['Subject', 'Marks', 'Max', 'Pass', 'Result']],
      body: rows,
      theme: 'grid',
      styles: { fontSize: 9, cellPadding: 3 },
      headStyles: { fillColor: [25, 118, 210] },
      margin: { left: 14, right: 14 },
    });

    y = doc.lastAutoTable.finalY + 4;

    doc.setFontSize(10);
    doc.setFont('helvetica', 'bold');
    doc.text(
      `Total: ${result.totalMarksObtained} / ${result.totalMaxMarks}  |  Percentage: ${result.percentage}%${result.grade ? '  |  Grade: ' + result.grade : ''}`,
      14,
      y
    );
    y += 12;
  });

  addFooter(doc);
  doc.save('exam-results.pdf');
}

export function generateFeeReceiptPDF(receipt) {
  const doc = new jsPDF();
  let y = addHeader(doc, 'Payment Receipt', `Receipt #: ${receipt.receiptNumber}`);

  doc.setFontSize(11);
  doc.setFont('helvetica', 'normal');

  const rows = [
    ['Receipt Number', receipt.receiptNumber],
    ['Fee Structure', receipt.feeStructureName || receipt.feeType || 'N/A'],
    ['Amount', `$${Number(receipt.amount || 0).toFixed(2)}`],
    ['Fine', `$${Number(receipt.fineAmount || 0).toFixed(2)}`],
    ['Total Paid', `$${Number(receipt.totalPaid || 0).toFixed(2)}`],
    ['Payment Method', receipt.paymentMethod || 'N/A'],
    ['Payment Date', receipt.paymentDate ? new Date(receipt.paymentDate).toLocaleDateString() : 'N/A'],
  ];

  if (receipt.transactionReference) {
    rows.push(['Transaction Ref', receipt.transactionReference]);
  }
  if (receipt.remarks) {
    rows.push(['Remarks', receipt.remarks]);
  }

  autoTable(doc, {
    startY: y,
    body: rows,
    theme: 'plain',
    styles: { fontSize: 11, cellPadding: 5 },
    columnStyles: {
      0: { fontStyle: 'bold', cellWidth: 60 },
      1: { cellWidth: 120 },
    },
    margin: { left: 14, right: 14 },
  });

  addFooter(doc);
  doc.save(`receipt-${receipt.receiptNumber}.pdf`);
}

export function generateAttendanceReportPDF(studentName, admissionNumber, className, sectionName, attendance, summary) {
  const doc = new jsPDF();
  let y = addHeader(doc, 'Attendance Report', `Student: ${studentName} | Adm: ${admissionNumber} | ${className} - ${sectionName}`);

  if (summary) {
    doc.setFontSize(11);
    doc.setFont('helvetica', 'bold');
    doc.text(`Attendance Percentage: ${summary.percentage}%`, 14, y);
    doc.setFont('helvetica', 'normal');
    doc.text(
      `  |  Present: ${summary.present}  |  Absent: ${summary.absent}  |  Late: ${summary.late}  |  Excused: ${summary.excused}`,
      14,
      y + 6
    );
    y += 14;
  }

  if (!attendance || attendance.length === 0) {
    doc.setFontSize(12);
    doc.text('No attendance records available.', 14, y + 10);
    doc.save('attendance-report.pdf');
    return;
  }

  const rows = attendance.map((a) => [
    new Date(a.date).toLocaleDateString(),
    a.status,
    a.remarks || '-',
  ]);

  autoTable(doc, {
    startY: y,
    head: [['Date', 'Status', 'Remarks']],
    body: rows,
    theme: 'grid',
    styles: { fontSize: 9, cellPadding: 3 },
    headStyles: { fillColor: [25, 118, 210] },
    margin: { left: 14, right: 14 },
  });

  addFooter(doc);
  doc.save('attendance-report.pdf');
}
