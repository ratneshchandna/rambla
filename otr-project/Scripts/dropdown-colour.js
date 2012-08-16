function CategoryColour() {
    var selected_category = $('#CategoryId').val();
    if (selected_category == "") {
        $('#CategoryId').attr('state', 'selected_none');
    }
    else {
        $('#CategoryId').attr('state', 'selected_option');
    }
}

function AgreementColour() {
    var selected_agreement = $('#AgreementId').val();
    if (selected_agreement == "") {
        $('#AgreementId').attr('state', 'selected_none');
    }
    else {
        $('#AgreementId').attr('state', 'selected_option');
    }
}

function RegionColour() {
    var selected_region = $('#RegionId').val();
    if (selected_region == "") {
        $('#RegionId').attr('state', 'selected_none');
    }
    else {
        $('#RegionId').attr('state', 'selected_option');
    }
}