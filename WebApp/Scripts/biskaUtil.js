function validateControl(model, parseDData) { 
    var data = null;
    if (model != null) data = $.parseJSON(model);
    else data = parseDData;
    //if (data.MessagesDialog.length == 0) return;
    
    $('form').find('.form-group').each(function (inx,elem) { 
        $(elem).removeClass('has-error');
        $(elem).removeClass('has-success');
        $(elem).removeClass('has-info');
    }); 
    for (var i = 0; i < data.MessagesDialog.length; i++) {  
        var item = data.MessagesDialog[i];
        var Message = item.Message;
        var MessageType = item.MessageType;
        var pName = item.PropertyName;
        var $inptID = $('#' + pName); 
        var $inpt = $("[name='" + pName + "']").first(); 
        if ($inptID.length != 0) $inpt = $inptID;
        //var type = $inpt.attr('nodeName');
        if ($inpt.length == 0) continue;
       
        var type = $inpt.get(0).nodeName;
        var cls = $inpt.attr('class');
        cls = (cls == "undefined" || cls == null ? "" : cls.toLowerCase());
        var IsDatetime = cls.indexOf('date-picker') >= 0;
        var $lbl = $('#lbl_' + pName);
        var chk = $inpt.is(':checkbox'); 
        //$inpt.parents('.form-group').first().attr('class', 'form-group');
        $inpt.parents('span').first().attr('class', '');
        if (type == 'text' && !IsDatetime) $inpt.find('i').remove();
        $lbl.attr('style', ''); 
        if (MessageType == 0) {
            $inpt.parents('.form-group').first().removeClass('has-error');
            $inpt.parents('.form-group').first().addClass('has-success');
            $lbl.attr('style', 'color:#8bad4c');
            //if ((type == "INPUT" || type == "TEXTAREA" || type == "SELECT") && !IsDatetime && item.AddIcon) {
            //    var $icn = $inpt.prev();
            //    $icn.remove();
            //    //$inpt.before('<i class="fa fa-check tooltips"></i>');
            //}
        }
        else if (MessageType == 1) {
            $inpt.parents('.form-group').first().removeClass('has-success');
            $inpt.parents('.form-group').first().addClass('has-error');
            $inpt.parents('span').first().addClass('block input-icon input-icon-left');
            $lbl.attr('style', 'color:#d68273');

            //if ((type == 'INPUT' || type == "TEXTAREA" || type == "SELECT") && !IsDatetime && item.AddIcon) {
            //    var $icn = $inpt.prev();
            //    $icn.remove();
            //    //if (!chk) $inpt.before('<i class="fa fa-info-circle tooltips" data-original-title="' + Message + '" data-container="body"></i>');
            //}
        }
        else if (MessageType == 2) {
            $inpt.parents('.form-group').first().removeClass('has-success');
            $inpt.parents('.form-group').first().addClass('has-error');
            $inpt.parents('span').first().addClass('block input-icon input-icon-left');
            $lbl.attr('style', 'color:#d68273');
            //if ((type == 'INPUT' || type == "password" || type == "TEXTAREA" || type == "SELECT") && !IsDatetime && item.AddIcon) {
            //    var $icn = $inpt.prev();
            //    $icn.remove();
            //    //if (!chk) $inpt.before('<i class="fa fa-info-circle tooltips" data-original-title="' + Message + '" data-container="body"></i>');
            //}
        }
        else if (MessageType == 3) {
            $inpt.parents('.form-group').first().addClass('has-info');
            $inpt.parents('span').first().addClass('block input-icon input-icon-left');
            $lbl.attr('style', 'color:#4b89aa');
            //if ((type == 'INPUT' || type == "TEXTAREA" || type == "SELECT") && !IsDatetime && item.AddIcon) {
            //    var $icn = $inpt.prev();
            //    $icn.remove();
            //    //if (!chk) $inpt.before('<i class="fa fa-info-circle tooltips" data-original-title="' + Message + '" data-container="body"></i>');
            //}
        }
        else {
            $inpt.parents('.form-group').first().removeClass('has-success');
            $inpt.parents('.form-group').first().removeClass('has-error');
           
        }

    }
}

function Tarayici() {
    var agent = navigator.userAgent;
    var ix1 = agent.toString().indexOf("MSIE");
    var ix2 = agent.toString().indexOf(";", ix1 + 3);
    var isIE = ix1 > -1;
    var vers = "";
    var name = "";
    if (isIE) {
        vers = agent.toString().substring(ix1 + 5, ix2);
        name = "IE";
    }
    else {
        var isChrome = agent.toString().indexOf("Chrome") > -1;
        if (isChrome) {
            name = "Chrome";
            ix1 = agent.toString().indexOf("Chrome");
            vers = agent.toString().substr(ix1 + 7, 2);
        }
        else {
            var isFireFox = agent.toString().indexOf("Firefox") > -1;
            if (isFireFox) {
                name = "Firefox";
                ix1 = agent.toString().indexOf("Firefox");
                vers = agent.toString().substr(ix1 + 8, 2);
            }
        }
    }
    var obj = new Object();
    obj.IsIE = isIE;
    obj.Version = parseFloat(vers);
    obj.Name = name;
    return obj;
}
var tarayici = new Tarayici();
function toggleByRow(tableRow, scrollToThisRow) {
    if (tableRow != null) {
        if (typeof tableRow === "string" && tableRow.toString().indexOf('.') < 0 && tableRow.toString().indexOf('#') < 0) tableRow = '#' + tableRow.toString();
        if ($(tableRow).css("display") == "none") {
            if (tarayici.IsIE == true && tarayici.Version < 8) {
                $(tableRow).css('display', 'block');
                var tblx = $(tableRow).parents("table").first();
                if (tblx.length > 0)
                    $(tableRow).width($(tblx).width());
            }
            else $(tableRow).css('display', 'table-row');
        }
        else {
            $(tableRow).css("display", "none");
        }
    }
    if (scrollToThisRow == true) {
        if ($(tableRow).css('display') != 'none') {
            var topx = $(tableRow).offset().top;
            window.top.scrollbarto(topx);
        }
    }
}
function toggleByClass(sndr, targetClss, scrollToThisRow) {
    var targetClass = targetClss.toString();
    if (targetClass.indexOf('.') < 0) targetClass = "." + targetClass;
    var dispx = $(targetClass).first().hasClass('hide');
    if (dispx) {
        if (tarayici.IsIE == true && tarayici.Version < 8) {
            $(targetClass).css('display', 'block');
            var tblx = $(targetClass).first().parents("table").first();
            if (tblx.length > 0)
                $(targetClass).width($(tblx).width());
        }
        else $(targetClass).removeClass('hide');
        $(sndr).find('span').first().removeClass("fa-angle-up").addClass("fa-angle-down"); 

    }
    else {
        $(targetClass).addClass("hide");
        $(sndr).find('span').first().removeClass("fa-angle-down").addClass("fa-angle-up"); 
    }
    if (scrollToThisRow == true) {
        //if ($(targetClass).first().css("display") != "none") {
        try {
            var topx = $($(targetClass).first().prev()).offset().top + 50;
            window.top.scrollbarto(topx);
        } catch (exp) { }
        //}
    } 
    onload();
}
function toggleByClass2(sndr, targetClss, scrollToThisRow) {
    var targetClass = targetClss.toString();
    if (targetClass.indexOf('.') < 0) targetClass = "." + targetClass;
    var dispx = $(targetClass).first().css('display');
    if (dispx == "none") {
        if (tarayici.IsIE == true && tarayici.Version < 8) {
            $(targetClass).css('display', 'block');
            var tblx = $(targetClass).first().parents("table").first();
            if (tblx.length > 0)
                $(targetClass).width($(tblx).width());
        }
        else $(targetClass).css('display', 'table-row');
        $(sndr).removeClass("row-details-close");
        $(sndr).addClass("row-details-open");

    }
    else {
        $(targetClass).css("display", "none");
        $(sndr).removeClass("row-details-open");
        $(sndr).addClass("row-details-close");
    }
    if (scrollToThisRow == true) {
        //if ($(targetClass).first().css("display") != "none") {
        try {
            var topx = $($(targetClass).first().prev()).offset().top + 50;
            window.top.scrollbarto(topx);
        } catch (exp) { }
        //}
    }
    onload();
}

function hideByClass(targetClss) {
    var targetClass = targetClss.toString();
    if (targetClass.indexOf('.') < 0) targetClass = "." + targetClass;
    var dispx = $(targetClass).first().css('display');
    $(targetClass).css("display", "none");
}
function showByClass(targetClss) {
    var targetClass = targetClss.toString();
    if (targetClass.indexOf('.') < 0) targetClass = "." + targetClass;
    var dispx = $(targetClass).first().css('display');
    if (tarayici.IsIE == true && tarayici.Version < 8) {
        $(targetClass).css('display', 'block');
        var tblx = $(targetClass).first().parents("table").first();
        if (tblx.length > 0)
            $(targetClass).width($(tblx).width());
    }
    else $(targetClass).css('display', 'table-row');
}
function showTableRow(trx) {
    if (tarayici.IsIE == true && tarayici.Version < 8) {
        $(trx).css('display', 'block');
        var tblx = $(trx).first().parents("table").first();
        if (tblx.length > 0)
            $(trx).width($(tblx).width());
    }
    else $(trx).css('display', 'table-row');
}
function getColCount(tbl) {
    var colCount = 0;
    $(tbl).find('tr:nth-child(1) td').each(function () {
        if ($(this).attr('colspan')) {
            colCount += +$(this).attr('colspan');
        } else {
            colCount++;
        }
    });
    return colCount;
}

var delay = (function () {
    var timer = 0;
    return function (callback, ms, sndr) {
        if (sndr != null) {
            var xtimer = $(sndr).attr("timer");
            if (xtimer != undefined) { var id = parseInt(xtimer.toString()); clearTimeout(id); }
        }
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
        if (sndr != null)
            $(sndr).attr("timer", timer);
    };
})();
