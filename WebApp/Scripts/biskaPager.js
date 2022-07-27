
$(function () {
    $('.pgrIlk').click(function () {
        $('#PageIndex').val(1);
        debugger
        frmSubmit();

    });
    $('.pgrGeri').click(function () {
        var pVal = parseInt($('#PageIndex').val());
        $('#PageIndex').val(pVal - 1);
        frmSubmit();
    });
    $('.pgrGit').click(function () {
        var pVal = parseInt($('.pgrPageIndex').val());
        $('#PageIndex').val(pVal);
        frmSubmit();
    });
    $('.pgrIleri').click(function () {
        var pVal = parseInt($('#PageIndex').val());
        $('#PageIndex').val(pVal + 1);
        frmSubmit();

    });
    $('.pgrSon').click(function () {
        var topl = parseInt($('.pgrToplamSayfa').text());
        $('#PageIndex').val(topl);
        frmSubmit();
    });
    $(".pgrSatirSayisi").change(function () {
        var val = $(this).val();
        $('#PageSize').val(val);
        frmSubmit();
    });

    $('.sorting').click(function () { 
        $('#PageIndex').val(1);
        frmSubmit();

    });
    $('.filtrele').find('select').change(function () { 
            frmSubmit(); 
    }); 
    $('.filtrele').find("input").change(function () { 
        setTimeout(function () {
            frmSubmit();
        }, 2000);
    });
    $('.filtrele').find("input").keypress(function () {
        if (event.which == 13) {
            frmSubmit();
        } 
    });
});
var sortVal = $('#Sort').val();
$('.sirala').click(function () {
   
    var sortType = "";
    var fieldName = $(this).attr("field");
    if (sortVal != null && sortVal != "") {
        var sortArr = sortVal.split(' ');

        if (sortArr.length == 1) {
            if (sortArr[0] == fieldName) sortType = "DESC"
        }
    } 
    $('#Sort').val((fieldName + " " + sortType).trim())
    frmSubmit();
});
setTimeout(function () { 
    $('.sirala').each(function (inx, thx) { 
        var thFName = $(thx).attr("field");
        var selectedF = sortVal.indexOf(thFName) >= 0;
        if (selectedF) {
            if (sortVal.indexOf("DESC") >= 0) $(thx).addClass("sorting_desc");
            else $(thx).addClass("sorting_asc");
        }
        else $(thx).addClass("sorting");
    });
}, 1000);
function frmSubmit() {
    var $forms = $('#PageIndex').parents('form').first();
    $forms.submit();
}