$(function () { 
    var genislik = 190;
    var sayfaSolBosluk = 5;
    var sayfaSagBosluk = 5;
    //CKEDITOR.config.extraPlugins = 'autogrow';
    CKEDITOR.config.autoGrow_minHeight = 250;
    CKEDITOR.config.autoGrow_maxHeight = 600;
    CKEDITOR.config.ImageBrowserURL = root + "ImageUploader/BrowseImages";
    CKEDITOR.config.ImageUploadUrl = root + "ImageUploader/Index";
    CKEDITOR.config.allowedContent = true;
    CKEDITOR.replace('AciklamaHtml',
            {
                qtStyle: { 'border-collapse': 'collapse' },
                startupFocus: true,
                uiColor: '#CCEAEE',
                //width: '210mm', 
                // autoGrow_minWidth: genislik,
                autoGrow_onStartup: true,
                autoGrow_resize_enabled: true,
                filebrowserBrowseUrl: root + 'FileBrowse/BrowseFile',
                filebrowserUploadUrl: root + 'FileBrowse/Upload',
                filebrowserImageUploadUrl: root + 'FileBrowse/Upload',
                filebrowserImageBrowseUrl: root + 'FileBrowse/BrowseFile',

                ImageBrowserURL: root + "ImageUploader/BrowseImages",
                ImageUploadUrl: root + "ImageUploader/Index",
                //enterMode: CKEDITOR.ENTER_BR,
                format_p: { element: 'p', attributes: { 'style': 'margin:0;padding:0' } },
                contentsCss: [root + "Content/ckeditor/contents.css"],
                on:
                {
                    instanceReady: function (ev) {
                        //setTimeout(function () {
                        //    CKEDITOR.instances.YazilanDokuman.focus();
                        //}, 300);

                        var ckFrame = $('.cke_wysiwyg_frame').first();
                        $('#ckeditorinfo').css("display", "none");
                        var cw = ckFrame[0].contentDocument;
                        var bodyx = $(cw).find('body');
                        var cssWdth = (genislik - sayfaSolBosluk - sayfaSagBosluk)
                        cssWdth = cssWdth + "mm";
                        var margLeft = sayfaSolBosluk + "mm";
                        var margRight = sayfaSagBosluk + "mm";
                        $(bodyx).css({
                            //'width': cssWdth,
                            'margin-left': margLeft,
                            'margin-right': margRight,
                            'margin-top': '0mm',
                            'margin-bottom': '0mm',
                            'font-family': 'Times New Roman',
                            'font-size': '12pt'
                        });


                        function removePinTd(strhtml) {
                            var tdStart = strhtml.indexOf('<td');
                            var isFirst = true;
                            while (tdStart > 0) {
                                var pStart = strhtml.indexOf('<p', tdStart);
                                var tdEnd = strhtml.indexOf('</td>', tdStart);
                                if (tdStart < pStart && tdEnd > pStart) {
                                    var pEnd = strhtml.indexOf('>', pStart);
                                    var pStr = strhtml.substring(pStart, pEnd + 1);

                                    var strPart1 = strhtml.substr(0, pStart);
                                    var strX = strhtml.substring(pStart, tdEnd);
                                    var strPart2 = strhtml.substr(tdEnd);
                                    //var strPartX = strX.replace(/<p>/g, '');
                                    var strPartX = strX.replace(new RegExp(pStr, 'g'), '');
                                    strPartX = strPartX.replace(/<\/p>/g, '');
                                    strhtml = strPart1 + strPartX + strPart2;
                                    if (isFirst == false) tdStart = tdStart - 4 - pStr.length;
                                }
                                tdStart = strhtml.indexOf('<td', tdStart + 1);
                                isFirst = false;
                            }
                            return strhtml;

                        }

                        ev.editor.on('paste', function (evt) {
                            evt.data.dataValue = removePinTd(evt.data.dataValue);
                            //evt.data.dataValue = evt.data.dataValue.replace(/<p><\/p>/g, '');
                        }, null, null, 9);

                    }
                }
                , toolbar:
                    [
                               { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source', '-', 'Save', 'NewPage', 'Preview', 'Print', '-', 'Templates'] },
                    //{ name: 'clipboard', groups: ['clipboard', 'undo'], items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
                    //{ name: 'editing', groups: ['find', 'selection', 'spellchecker'], items: ['Find', 'Replace', '-', 'SelectAll', '-', 'Scayt'] },
                    //{ name: 'forms', items: ['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField'] },
                    //'/',
                    //{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                    //{ name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl', 'Language'] },
                    //{ name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                    //{ name: 'insert', items: ['Image', 'Flash', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
                    //'/',
                    //{ name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
                    //{ name: 'colors', items: ['TextColor', 'BGColor'] },
                    //{ name: 'tools', items: ['Maximize', 'ShowBlocks'] },
                    //{ name: 'others', items: ['-'] },
                    //{ name: 'about', items: ['About'] }



                            { name: 'styles', items: ['Font', 'FontSize'] },
                            { name: 'styles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                            { name: 'insert', items: ['Image','base64image', 'mypictures', 'Table'] },
                            { name: 'colors', items: ['TextColor', 'BGColor'] },
                            {
                                name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent',
                                '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'SpecialChar', 'PageBreak']
                            },
                            { name: 'clipboard', items: ['Cut', 'Copy', 'PasteFromWord', '-', 'Undo', 'Redo'] },
                            { name: 'links', items: ['Link', 'Unlink', 'Anchor'] }
                    ]
            });
    //CKEDITOR.on('dialogDefinition', function(event) {
    //    var dialogName = event.data.name;
    //    var dialogDefinition = event.data.definition;
    //    //some code here
    //            debugger
    //    if(dialogName == 'flash'){ // flash dialog box name
    //        //some code her 
    //        dialogDefinition.onShow = function () {
    //            this.getContentElement("info","width").disable(); // info is the name of the tab and width is the id of the element inside the tab
    //            this.getContentElement("info","height").disable();
    //        }
    //    }
    //});
});
