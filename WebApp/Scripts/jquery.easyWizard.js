/* ========================================================
 * easyWizard v1.1.3
 * http://st3ph.github.com/jquery.easyWizard
 * ========================================================
 * Copyright 2012 - 2015 Stéphane Litou
 * http://stephane-litou.com
 *
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.opensource.org/licenses/GPL-2.0
 * ======================================================== */
(function ($) {
    var arrSettings = [];
    var easyWizardMethods = {
        init: function (options) {
            var settings = $.extend({
                'stepClassName': 'content',
                'showSteps': true,
                'stepsText': '{n}. {t}',
                'showButtons': true,
                'buttonIleriClass': '',
                'buttonGeriClass': '',
                'prevButton': '< Back',
                'nextButton': 'Next >',
                'debug': false,
                'submitButton': true,
                'submitButtonText': 'Submit',
                'submitButtonClass': '',
                before: function (wizardObj, selectedStepObj, nextStepObj) { },
                after: function (wizardObj, prevStepObj, selectedStepObj) { },
                beforeSubmit: function (wizardObj) {
                    sbmtFrm();
                    //wizardObj.find('input, textarea').each(function () {

                    //    if (!this.checkValidity()) {
                    //        this.focus();
                    //        step = $(this).parents('.' + thisSettings.stepClassName).attr('data-step');
                    //        //easyWizardMethods.goToStep.call(wizardObj, step);


                    //        return false;
                    //    }
                    //});
                }
            }, options);

            arrSettings[this.index()] = settings;

            return this.each(function () {
                thisSettings = settings;

                $this = $(this); // Wizard Obj
                $this.addClass('easyWizardElement');
                $steps = $this.find('.' + thisSettings.stepClassName);
                thisSettings.steps = $steps.length;
                thisSettings.width = $(this).width();

                if (thisSettings.steps > 1) {
                    // Create UI
                    //$this.wrapInner('<div class="stepContainer" />');
                    $this.find('.stepContainer').width(thisSettings.width * thisSettings.steps);
                    $this.css({
                        'position': 'relative',
                        'overflow': 'hidden'
                    }).addClass('easyPager');

                    $stepsHtml = $('<ul class="tabsK steps_3 anchor">');

                    $steps.each(function (index) {
                        step = index + 1;
                        $(this).css({
                            'float': 'left',
                            'width': thisSettings.width,
                            'height': '1px'
                        }).attr('data-step', step);

                        if (!index) {
                            $(this).addClass('active').css('height', '');
                        } else {
                            $(this).find('input, textarea, select, button').attr('tabindex', '-1');
                        }
                        // var _a = "<a href='#step-1' class='selected' isdone='1' rel='1'><span class='stepNumber'>1</span><span style='text-align:center;' class='stepDesc'>1.Adım <small>Kimlik Bilgisi &amp; </small><br><small>Adres Bilgisi</small></span></a>"

                        stepText = thisSettings.stepsText.replace('{n}', '<span>' + step + '</span>');
                        stepText = stepText.replace('{t}', $(this).attr('data-step-title'));
                        $stepsHtml.append('<li' + (!index ? ' class="selected"' : '') + ' data-step="' + step + '">' + stepText + '</li>');
                    });
                    if (thisSettings.showSteps) {
                        $this.prepend($stepsHtml);
                    }
                    if (thisSettings.showButtons) {
                        paginationHtml = "<div class='actionBar'>";
                        paginationHtml += thisSettings.submitButton ? "<a href='javascript:void(0);' style='display:block;' class='kaydet " + thisSettings.submitButtonClass + "'>" + thisSettings.submitButtonText + "</a>" : "";
                        paginationHtml += "<a href='javascript:void(0);' class='geri " + thisSettings.buttonGeriClass + "'>" + thisSettings.prevButton + "</a>";
                        paginationHtml += "<a href='javascript:void(0);' class='ileri " + thisSettings.buttonIleriClass + "'>" + thisSettings.nextButton + "</a>";
                        paginationHtml += '</div>';
                        $paginationBloc = $(paginationHtml);
                        $paginationBloc.css('clear', 'both');
                        $paginationBloc.find('.geri, .kaydet').hide();
                        $paginationBloc.find('.geri').bind('click.easyWizard', function (e) {
                            e.preventDefault();

                            $wizard = $(this).parents('.easyWizardElement');
                            easyWizardMethods.prevStep.apply($wizard);
                        });

                        $paginationBloc.find('.ileri').bind('click.easyWizard', function (e) {
                            e.preventDefault();

                            $wizard = $(this).parents('.easyWizardElement');
                            easyWizardMethods.nextStep.apply($wizard);
                        });
                        $this.append($paginationBloc);
                    }

                    $formObj = $this.is('form') ? $this : $(this).find('form');

                    // beforeSubmit Callback
                    $this.find('.kaydet').bind('click.easyWizard', function (e) {
                        $wizard = $(this).parents('.easyWizardElement');
                        var beforeSubmitValue = thisSettings.beforeSubmit($wizard);
                        if (beforeSubmitValue === false) {
                            return false;
                        }
                        return true;
                    });
                } else if (thisSettings.debug) {
                    console.log('Can\'t make a wizard with only one step oO');
                }
            });
        },
        prevStep: function () {
            thisSettings = arrSettings[this.index()];
            $activeStep = this.find('.' + thisSettings.stepClassName + '.active');
            if ($activeStep.prev('.' + thisSettings.stepClassName).length) {
                prevStep = parseInt($activeStep.prev().attr('data-step'));
                stepControl('bck', prevStep);
                //easyWizardMethods.goToStep.call(this, prevStep);
            }
        },
        nextStep: function () {
            thisSettings = arrSettings[this.index()];
            $activeStep = this.find('.' + thisSettings.stepClassName + '.active');
            if ($activeStep.next('.' + thisSettings.stepClassName).length) {
                nextStep = parseInt($activeStep.next().attr('data-step'));
                stepControl('', nextStep);
                //easyWizardMethods.goToStep.call(this, nextStep);
            }
        },
        goToStep: function (step) {
            thisSettings = arrSettings[this.index()];
            $activeStep = this.find('.' + thisSettings.stepClassName + '.active');
            $nextStep = this.find('.' + thisSettings.stepClassName + '[data-step="' + step + '"]');
            selectedStep = $activeStep.attr('data-step');

            // Prevent sliding same step
            if (selectedStep == step) return;

            // Before callBack
            var beforeValue = thisSettings.before(this, $activeStep, $nextStep);
            if (beforeValue === false) {
                return false;
            }

            // Define direction for sliding
            if (selectedStep < step) { // forward
                leftValue = thisSettings.width * -1;
            } else { // backward
                leftValue = thisSettings.width;
            }

            // Slide ! 
            $activeStep.removeClass('active');
            $activeStep.find('input, textarea, select, button').attr('tabindex', '-1');

             $nextStep.css('height', '').addClass('active');
            $nextStep.find('input, textarea, select, button').removeAttr('tabindex');
            var ouH = $nextStep.height();

           // $this.find('.stepContainer').animate({ height: (ouH + 30) }, 140);
            this.find('.stepContainer').stop(true, true).animate({
                'margin-left': thisSettings.width * (step - 1) * -1
                //,'height':ouH
            }, function () {
                $activeStep.css({ height: '1px' });
            }); 
           
            // Defines steps
            //this.find('.tabsK li[data-step="' + step + '"]').removeClass('done'); 
            var _liA = this.find('.tabsK li a').each(function (inx, ax) {

                var $_a = $(ax);
                var _astp = parseInt($_a.attr("data-step"));
                if (_astp > step) $_a.attr("class", "disabled");
                else if (_astp == step) $_a.attr("class", "selected");
                else $_a.attr("class", "done");
            }); 
            //this.find('.tabsK li a[data-step="' + (step - 1) + '"]').removeClass('selected').addClass('done');
            //this.find('.tabsK li a[data-step="' + step + '"]').addClass('selected');

            // Define buttons
            $paginationBloc = this.find('.actionBar');
            if ($paginationBloc.length) {
                if (step == 1) {
                    $paginationBloc.find('.geri, .kaydet').hide();
                    $paginationBloc.find('.ileri').show()
                } else if (step < thisSettings.steps) {
                    $paginationBloc.find('.kaydet').hide();
                    $paginationBloc.find('.geri, .ileri').show();
                } else {
                    //if ($nextStep.prev('.' + thisSettings.stepClassName).length) { // If there is a previous step, must be always the case but... you know =)
                    //    $paginationBloc.find('.geri').hide();
                    //}
                    $paginationBloc.find('.geri').show();
                    $paginationBloc.find('.ileri').hide();
                    $paginationBloc.find('.kaydet').show();
                }
            } 
            // After callBack
            thisSettings.after(this, $activeStep, $nextStep);
        }
    };

    $.fn.easyWizard = function (method) {
        if (easyWizardMethods[method]) {
            return easyWizardMethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return easyWizardMethods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.easyWizard');
        }
    };
})(jQuery);
