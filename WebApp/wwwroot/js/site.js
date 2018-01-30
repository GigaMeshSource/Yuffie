"use strict";

var changeletters = [
    ["éèêë", "e"],
    ["àäâ", "a"],
    ["ïî", "i"],
    ["öô", "o"],
    ["üû", "u"],
    ["ÿŷ", "y"],
    [" \/", "_"],
    ["'", ""]
]

$(function() {
    var elements = []
    
    var formatKey = function(key) {
        for(var i = 0; i < changeletters.length; ++i) {
            var letters = changeletters[i][0];
            var replacement = changeletters[i][1];
            for(var j =0; j < letters.length; ++j) {
                key = key.replace(/letters[0]/ig, replacement);
            }
        }
        return key;
    }
    var elementDic = {}
    var initElementDic = function(){
        for(var i = 0; i < elements.length; ++i) {
            elementDic[elements[i].Key] = elements[i]
        }
    }
    var getElement = function(key){
        return elementDic[formatKey(key)];
    }

    var processEffect = function(impacted, effect) {
        if(effect.Css != undefined && effect.Css != null) {
            for(var i = 0; i < impacted.length; ++i) {
                key = impacted[i];
                key = formatKey(key)
                var block = $("#con_" + key)
                var summaryBlock = $("#con_sum_" + key)
                for(var property in effect.Css) {
                    block.css(property, effect.Css[property])
                    summaryBlock.css(property, effect.Css[property])
                }
            }
        }
    }

    var initEffect = function() {
        for(var i = 0; i < rules.length; ++i) {
            var rule = rules[i]
            if(rule.StartingEffect != undefined && rule.StartingEffect != null) {
                processEffect(rule.Impacted, rule.StartingEffect)
            }
        }
    }

    var processChange = function() {
        initEffect();
        for(var i = 0; i < rules.length; ++i) {
            var rule = rules[i]
            console.log("Testing rule : " + rule.Key)
            var result = true
            for(var j = 0; j < rule.Conditions.length; ++j) {
                var condition = rule.Conditions[j]
                console.log("Condition : " + condition.Key);
                var element = getElement(condition.Key)
                if(condition.Value.constructor === Array) {
                    result = false
                    for(var k in condition.Value) {
                        var item = condition.Value[k];
                        if(element != null && item == element.Value) {
                            result = true
                            break
                        }
                    }
                    if(!result) break
                }
                else {
                    if(element == null || element.Value != condition.Value) {
                        result = false
                        break
                    }
                }
                console.log("Success")
            }
            if(result) {
                console.log("Execute rule")
                processEffect(rule.Impacted, rule.Effect)
            }
            // else if(rule.StartingEffect != undefined) {
            //     console.log("Rollback rule")
            //     processEffect(rule.Impacted, rule.StartingEffect)
            // }
        }
    }

    var copySummary = function(element) {
        var summary = $("[summary='" + element.Key + "']")
        if(summary.is("div")) {
            summary.empty()
            if(element.Value == null || element.Value.length == 0) {
                summary.append("<p>Aucune valeur saisie</p>")
            }
            else {
                for(var i = 0; i < element.Value.length; ++i) {
                    var subelement = element.Value[i]
                    var id = ""
                    var limit = 2;
                    var index = 0;
                    for(var property in subelement) {
                        id = id + " " + subelement[property]
                        index++;
                        if(index >= limit) {
                            break;
                        }
                    }
                    summary.append("<div class='col s3'><div chip-for='" + (element.Value.length - 1) + "' class='chip'>" + id + "</div></div>")
                }
            }
        }
        else {
            if(element.Value == null || element.Value == "") {
                summary.html("Aucune valeur saisie")
            }
            else {
                summary.html(element.Value)
            }
        }
    }

    $("[watch]").each(function(index, watchElement) {
        watchElement = $(watchElement)
        var key = watchElement.attr("watch")
        var rawKey = ""+key
        var parentId = null
        var element = null
        var parentElement = null
        var parent = watchElement.parents("[parent-element]")
        element = {
            Key: key,
            Value: null,
            Parent: null
        }
        if(parent.length > 0) {
            parentId = $(parent[0]).attr("parent-element")
            element.Parent = parentId
        }
        elements.push(element);

        if(watchElement.is("input") || watchElement.is("select") || watchElement.is("textarea")) {
            element.Value = watchElement.val()
            watchElement.change(function() {
                var e = getElement(watchElement.attr("watch"))
                if(e == null && parentId != null) {
                    e = {
                        Key: watchElement.attr("watch"),
                        Value : null
                    }
                    var parent = getElement(parentId)
                    if(parent.Value == null) {
                        parent.Value = [[]]
                    }
                    parent.Value.push(e)
                }
                e.Value = watchElement.val()
                processChange()
                copySummary(e)
            })
        }

        if(watchElement.is("[type=radio]")) {
            watchElement.change(function() {
                if(watchElement.attr("checked")) {
                    var e = getElement(watchElement.attr("watch"))
                    if(e == null && parentId != null) {
                        e = {
                            Key: watchElement.attr("watch"),
                            Value : null
                        }
                        var parent = getElement(parentId)
                        if(parent.Value == null) {
                            parent.Value = [[]]
                        }
                        parent.Value.push(e)
                    }
                    e.Value = watchElement.val()
                    processChange()
                    copySummary(e)
                }
            })
        }

        if(watchElement.is("[type=checkbox]")) {
            watchElement.change(function() {
                var e = getElement(watchElement.attr("watch"))
                if(e == null && parentId != null) {
                    e = {
                        Key: watchElement.attr("watch"),
                        Value : null
                    }
                    var parent = getElement(parentId)
                    if(parent.Value == null) {
                        parent.Value = [[]]
                    }
                    parent.Value.push(e)
                }
                e.Value = watchElement.attr("checked")
                processChange()
                copySummary(e)
            })
        }

        if(watchElement.is("[type=button]")) {
            watchElement.click(function() {
                var e = getElement(watchElement.attr("watch"))

                var obj = {}
                var id = ""
                $("[parent-element=" + e.Key + "] [watch]").each(function(i, wEl){
                    var wKey = $(wEl).attr("watch")
                    var wE = getElement(wKey)
                    
                    if(wE.Value == null || wE.Value == "") return;

                    obj[wKey] = wE.Value
                    if(i < 2) {
                        id = id + " " + obj[wKey]
                    }
                    $(wEl).val(null);
                    wE.Value = null
                });
                if(obj.length == 0) return;

                if(e.Value == null || e.Value.constructor !== Array) e.Value = []

                e.Value.push(obj)
                
                $("#con_" + e.Key + " [watch-list]").append("<div chip-for='" + (e.Value.length - 1) + "' class='chip'>" + id + "<i chip-close='"+e.Key+"' class='close material-icons'>close</i></div>")

                processChange()
                copySummary(e)
            })
        }
        if(element != null)
            copySummary(element)
    })

    $("[tree-select]").click(function(evt) {
        var target = $(evt.target)
        var alreadySelected = target.attr("tree-selected") == "1";

        var bindings = target.parents("[bind-to]")
        for(var i = 0; i < bindings.length; ++i) {
            var binding =$(bindings[i])
            if(i == 0) {
                var children = binding.find("[bind-to]")
                for(var j = 0; j < children.length; ++j) {
                    var child = $(children[j])
                    var bindToClean = formatKey(child.attr("bind-to"))
                    var e = getElement(bindToClean)
                    if(e != null) {
                        e.Value = null
                        copySummary(e)
                    }
                }
            }
            var bindTo = formatKey(binding.attr("bind-to"))
            var e = getElement(bindTo)
            if(e == null) {
                e = {
                    Key: bindTo,
                    Value: null
                }
                elements.push(e)
            }
            if((i == 0 && !alreadySelected) || i > 0) {
                if(binding.find("span a").length == 1) {
                    e.Value = target.html()
                }
                else {
                    let headers = binding.find(".collapsible-header");
                    if(headers.length > 0)
                        e.Value = $(headers[0]).html()
                    else
                        e.Value = binding.find("p").html()
                }
            }
            else {
                e.Value = null
            }
            copySummary(e)
        }
        $(target.parents("ul")[0]).find("[tree-selected=1]").attr("tree-selected", "0")
        if(alreadySelected) {
            target.attr("tree-selected", "0")
        }
        else {
            target.attr("tree-selected", "1")
        }
        
    })

    initEffect()

    var sendData = function() {
        var toSend = []
        for(var i in elements) {
            var e = elements[i]
            if(e.Value != null && e.Parent == null) {
                toSend.push(e)
            }
        }
        console.log(toSend)

        var error = function(error) {
            $('#modal_validate [error-msg]').removeClass("hide")
            $('#modal_validate [success-msg]').addClass("hide")            
            $('#modal_validate [validate-modal-form]').removeClass("hide")
        }
        $.ajax({
            type: "POST",
            url: "/Home/PushData",
            data: "data=" + JSON.stringify(convertToDictionary(toSend)),
            success: function(wtf, response, result) {
                if(result.status == 200) {
                    $('#modal_validate [success-msg]').removeClass("hide")
                    $('#modal_validate [error-msg]').addClass("hide")
                    $('#modal_validate [validate-modal-form]').addClass("hide")
                    setTimeout(function() {
                        window.location = "index"             
                    }, 5000);
                }
                else 
                {
                    console.log("error")
                    error()
                }
            },
            error: error
        });
    }
    
    $("[validate-form]").click(function(){
        $('#modal_validate [success-msg]').addClass("hide")        
        $('#modal_validate [error-msg]').addClass("hide")
        $('#modal_validate [validate-modal-form]').addClass("hide")
        $('#modal_validate').modal();
        setTimeout(sendData, 2000)
    })

    $("[validate-modal-form]").click(function(){
        $('#modal_validate [success-msg]').addClass("hide")        
        $('#modal_validate [error-msg]').addClass("hide")
        $('#modal_validate [validate-modal-form]').addClass("hide")
        sendData()
    })



    $("body").on("click", "[chip-close]", function(evt) {
        evt.stopPropagation()
        var target = $(evt.target)
        var key = target.attr("chip-close")
        var element = getElement(key)
        var index = $(target.parents("[chip-for]")[0]).attr("chip-for")

        if(element.Value != null && index > -1) {
            element.Value.splice(index);
            $("[chip-for='" + index + "']").remove();
        }
        copySummary(element)
    })

    $(".not-collapse").on("click", function(e) { e.stopPropagation(); });


    $('select').material_select();
    
    $('.collapsible').collapsible();

    $('.chips-placeholder').material_chip({
        placeholder: 'Participants ajoutés',
        secondaryPlaceholder: '+ Participants',
      });

    $('.datepicker').pickadate({
        selectMonths: false, // Creates a dropdown to control month
        selectYears: 15, // Creates a dropdown of 15 years to control year,
        today: 'Today',
        clear: 'Clear',
        close: 'Ok',
        closeOnSelect: true, // Close upon selecting a date,
        monthsFull: ['Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin', 'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'],
        monthShort: ['Janv.', 'Févr.', 'Mars', 'Avril', 'Mai', 'Juin', 'Juil.', 'Août', 'Sept.', 'Oct.', 'Nov.', 'Déc.'],
        weekdaysShort: ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'],
        weekdaysLetter: [ 'D', 'L', 'M', 'M', 'J', 'V', 'S' ],
        today: 'aujourd\'hui',
        clear: 'effacer',
    });

    $('.timepicker').pickatime({
        default: 'now', // Set default time: 'now', '1:30AM', '16:30'
        fromnow: 0,       // set default time to * milliseconds from now (using with default = 'now')
        twelvehour: false, // Use AM/PM or 24-hour format
        donetext: 'OK', // text for done-button
        cleartext: 'Clear', // text for clear-button
        canceltext: 'Cancel', // Text for cancel-button
        autoclose: false, // automatic close timepicker
        ampmclickable: true, // make AM PM clickable
        aftershow: function(){} //Function for after opening timepicker
      });

    $("[next-button]").click(function(e) {
        var parent = $($(e.target).parents("li"));
        var next = parent.next();
        if(next.is("li")) {
            $(next.find(".collapsible-header")[0]).click();
        }
        else {
            var id = parent.parents(".page").attr("id");
            $($("li.tab a[href='#" + id + "']").parents("li").next().find("a")[0]).click()
        }
    })


    var convertToDictionary = function(array) {
        var dic = {}
        for(var i in array) {
            var e = array[i]
            dic[e.Key] = e.Value
        }
        return dic;
    }
    initElementDic();
})
var rules = []