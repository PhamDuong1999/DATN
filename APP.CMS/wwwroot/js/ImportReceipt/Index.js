$(function () {
    $("select").select2();
    getData("",-1);
    $('#frmFilter').find('#btnSearch').on('click', function () {
        var time = $('#frmFilter').find('#txtDate').val();
        var IdPT = $('#frmFilter').find('#idPhuTung').val();
        getData(time,IdPT);
    });
    $('#frmFilter').find('#btnCreate').on('click', function () {
        location.href = "/don-hang-nhap" + '/tao-moi';
    });
});
function getData(time,id) {
    showLoading();
    $.ajax({
        url: "/don-hang-nhap/get-list?createdDate="+time+"&IdPhuTung="+id,
        method: "Get",
        success: function (response) {
            $('#dtTable').html(response);
            hideLoading()
        }
    })
}