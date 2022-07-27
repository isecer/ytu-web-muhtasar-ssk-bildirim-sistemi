bootbox.setDefaults({ locale: "tr" });
var fxs = new Object();
function resizeIFrame(h) {
    var wH = $(window).height() - $('#navbar').height();
    if (h < wH) h = wH;
    var $xframe = $('#modulFrame');//$('#bodyFrame')
    //$('#bodyFrame').height(h);
    $xframe.height(h);
}
function showMessage(msg) {
    if (typeof msg === 'string') {
        bootbox.alert(msg);
    }
    else {
        var htmlData = $(msg).html();
        bootbox.alert(htmlData);
    }
}

function getDocumentHeight() {
    var h = Math.max($(document).height(), $(window).height(), document.documentElement.clientHeight);
    return h;
}
function getFramePosition() {
    var $xframe = $('#modulFrame');
    return $xframe.position();
}
function getFormPosition(w, h) {
    var scTop = $(window).scrollTop();
    var winH = $(window).height();
    var refH = Math.max($(window).height(), $(document).height());
    var refW = Math.max($(window).width(), $(document).width());


    scTop = scTop < winH ? winH : scTop;
    var tc = scTop - winH / 2;
    var newT = tc - h / 2;
    var newL = (refW - w) / 2;
    var obj = new Object();
    obj.top = Math.round(newT);
    obj.left = Math.round(newL);
    return obj;
}

var yenidialogcnt = 0;
function yeniDialogAc(Sndr, Title, Url, w, h, leftx, topx, ismodal, fncOnClose, noscrollbar, scrollToTop) {

    var refW = $(window).width();
    if (refW == 0) refW = $(document).width();
    var refH = $(window).height();
    if (refH == 0) refH = $(document).height();
    var maxH = screen.availHeight;
    var maxW = screen.availWidth;
    if (w == null || w == 0) w = refW - 100;
    if (h == null || h == 0) h = refH - 30;

    if (w <= 0 || w > refW) w = 780;
    if (h <= 0 || h > refH) h = 390;
    h += 10;
    if (yenidialogcnt == 50) yenidialogcnt = 0;
    var posx = getFormPosition(w, h);
    var l = leftx == null || leftx == undefined ? posx.left + yenidialogcnt : leftx;
    var t = topx == null || topx == undefined ? posx.top + yenidialogcnt : topx;


    //t = 20;
    if (scrollToTop == true) scrollbarto(t);
    ismodal = (ismodal == null || ismodal == undefined) ? false : ismodal;
    noscrollbar = (noscrollbar == null || noscrollbar == undefined) ? false : noscrollbar;


    var nowx = new Date();
    var id = Number(nowx);
    var newdialog = '<div id="dialog' + id + '" title="Başlık" style="background-color:white; display:none;overflow:hidden;z-index:1002">' +
                                ' <iframe id="dialogFrame' + id + '" frameborder="0" style="width:100%;height:100%;margin:0px;' + (noscrollbar ? "overflow:hidden" : "") + '" ' + (noscrollbar ? 'scrolling="no"' : "") + ' src="#">' +
                                ' </iframe>' +
                                '</div>';

    var docH = getDocumentHeight();
    //var $modalWin = $('<div id="modal' + id + '"  class="modalbackground" style="position:fixed;left:0;top:0;height:'+docH+'px;right:0;background-color:#808080;opacity:0.4;z-index:1001;background-color:rgba(0, 0, 0, 0.5)"></div>');
    //if(ismodal)
    //   $modalWin.appendTo('body');

    $(newdialog).appendTo("body");
    if (Url.toString().indexOf("?") < 0) Url += "?";
    else Url += "&";
    Url += "dlgid=dialog" + id;

    var f = document.getElementById('dialogFrame' + id);
    f.src = root + "Home/Loading";
    var dlgx = $('#dialog' + id).dialog(
            {
                modal: ismodal,
                title: Title,
                width: w, height: h,
                //position: [l, t],
                maxHeight: maxH, maxWidth: maxW,
                resizable: true,
                close: function () {
                    //try {
                    //    $('#modal' + id).remove();

                    //    $('#modulFrame').height(iframeHeight);
                    //    $(window).scrollTop(lastScrollTop);
                    //}
                    //catch (cerr) { }

                    if (fncOnClose != null && fncOnClose != undefined) {
                        setTimeout(fncOnClose, 100);
                    }
                },
                open: function () {
                    f.src = Url; 
                    $closeBtn = $(this).parent().children().children(".ui-dialog-titlebar-close").first();
                    $closeBtn.attr("title", "Kapat").height(25);
 
                    $closeBtn.parents().first().dblclick(function () {
                        var posx = dlgx.dialog("option", "position");
                        if (posx[0] == 0) {
                            dlgx.dialog("option", { width: w, height: h, position: [l, t] });
                            dlgx.dialog("option", "position", "[" + l + "," + t + "]");
                        }
                        else {
                            dlgx.dialog("option", { position: [0, 0], width: ($(window).width() - 10), height: ($(window).height()) });
                        }
                    });
                    //document.getElementById('dialogFrame' + id).focus();
                }
            });
}

function yeniDialogBootbox(Sndr, Title, Url, w, h, leftx, topx, ismodal, noscrollbar, fncOnClose, scrollToTop) {

    var refW = $(window).width();
    if (refW == 0) refW = $(document).width();
    var refH = $(window).height();
    if (refH == 0) refH = $(document).height();
    var maxH = screen.availHeight;
    var maxW = screen.availWidth;
    if (w == null || w == 0) w = refW - 100;
    if (h == null || h == 0) h = refH - 30;

    if (w <= 0 || w > refW) w = 600;
    if (h <= 0 || h > refH) h = 500;

    if (yenidialogcnt == 50) yenidialogcnt = 0;
    var posx = getFormPosition(w, h);
    var l = leftx == null || leftx == undefined ? posx.left + yenidialogcnt : leftx;
    var t = topx == null || topx == undefined ? posx.top + yenidialogcnt : topx;

    var nowx = new Date();
    var id = Number(nowx);
    h += 52;
    ismodal = (ismodal == null || ismodal == undefined) ? false : ismodal;
    if (scrollToTop == true) scrollbarto(t);

    var docH = getDocumentHeight() + 50;

    var dlgid = 'modal-form-' + id;
    if (Url.toString().indexOf("?") < 0) Url += "?";
    else Url += "&";
    Url += "dlgid=" + dlgid;


    var dlgFrame = '<div id="' + dlgid + '" class="modal" tabindex="-1" style="position:absolute;z-index:1005;margin:0;padding:0;width:' + w + 'px;height:' + h + 'px;overflow:hidden;left:' + l + 'px;top:' + t + 'px">' +
                    '<div class="modal-dialog"  style="margin:0px;padding:0;width:' + w + 'px;height:' + h + 'px">' +
                        '<div class="modal-content"  style="margin:0px;padding:0px;height:' + h + 'px;width:' + w + 'px">' +
                          '<div class="modal-header widget-header widget-header-small"  style="margin:auto 0;height:22px; padding:5px;">' +
                                '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
                                    '<h4 class="blue bigger">' + Title + '</h4>' +
                           '</div>' +
                           '<div class="modal-body overflow-visible"  style="margin:0;padding:0;">' +
                                '<div class="row" style="padding:0px 3px">' +
                                  '<iframe id="iframeDialog" src="' + Url + '" border="0" frameBorder="0" style="width:' + (w - 14) + 'px; height:' + (h - 77) + 'px;margin:0px 3px;' +
                                  (noscrollbar ? "overflow:hidden;" : "") + '" ' + (noscrollbar ? ' scrolling="no"' : "") + '></iframe>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>';
    var $dlg = $(dlgFrame).appendTo('body');
    $dlg.draggable({ handle: ".modal-header" });

    var $modalWin = $('<div id="modal' + id + '"  class="modalbackground" style="position:fixed;left:0;top:0;height:' + docH + 'px;width:100%;background-color:#808080;opacity:0.4;z-index:1001;background-color:rgba(0, 0, 0, 0.5)"></div>');
    if (ismodal)
        $modalWin.appendTo('body');

    $dlg.find('.close').click(function () {
        try {
            $dlg.remove();
            $modalWin.remove();
            $('#modulFrame').height(iframeHeight);
            $(window).scrollTop(lastScrollTop);
        }
        catch (errx) { }
        if (fncOnClose != null)
            setTimeout(fncOnClose, 0, $dlg, $dlg.attr('id'));
    });
    $dlg.css("display", "block");
}

var iframeHeight = 0;
var lastScrollTop = 0;
function yeniDialog(options) {
    iframeHeight = $('#modulFrame').height();
    lastScrollTop = $(window).scrollTop();
    options.IsModal = options.IsModal == undefined ? true : options.IsModal;
    options.IsBootbox = options.IsBootbox == undefined ? true : options.IsBootbox;
    if (options.IsBootbox)
        yeniDialogBootbox(options.Sender, options.Title, options.Url, options.Width, options.Height, options.Left, options.Top, options.IsModal, options.NoScrollbar, options.OnClose, options.ScrollToTop);
    else
        yeniDialogAc(options.Sender, options.Title, options.Url, options.Width, options.Height, options.Left, options.Top, options.IsModal, options.OnClose, options.NoScrollbar, options.ScrollToTop);

    setTimeout(function () {
        $('#modulFrame').height(iframeHeight);
        $(window).scrollTop(lastScrollTop);
    }, 200)

}
function dialogKapat(dlgid) { 
    if (dlgid == null || dlgid == "" || dlgid == undefined) {
        $('#dialog').dialog('close');
        $('.modal').remove();
    }
    else {
        try {
            var $dlg = $('#' + dlgid);
            $dlg.find('.close')[0].click();
        } catch (errx) { }
        $('#' + dlgid).dialog('close');
        $('#' + dlgid).remove();
    }
    $('.modalbackground').remove();
    $('#modulFrame').height(iframeHeight);
    $(window).scrollTop(lastScrollTop);

}