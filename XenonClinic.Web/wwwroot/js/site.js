function readEid() {
    fetch('/Patients/ReadEid', { method: 'POST' })
        .then(r => r.json())
        .then(data => {
            document.getElementById('EmiratesId').value = data.emiratesId;
            document.getElementById('FullNameEn').value = data.fullNameEn;
            document.getElementById('FullNameAr').value = data.fullNameAr;
            document.getElementById('DateOfBirth').value = data.dateOfBirth;
            document.getElementById('Gender').value = data.gender;
        });
}
