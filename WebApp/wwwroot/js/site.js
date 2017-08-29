 var rules = [    
{
    "Key": "Service DCO - IP",
    "Impacted": [   "CDN BANQUE", "CDN REGION", "CDN GROUPE", "CDN Agence", "CDN Code Agence", 
                    "SG DR", "SG DEC", "SG UC", "SG Agence", "SG Code Agence", 
                    "CRCM", "PSC", 
                    "ASSU", "Type d'intervention", "Intervention IP", "Formation", "Type d'intervention 2"],
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Service DCO",
        "Value": "IP",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
},
{
    "Key": "Service DCO - SG",
    "Impacted": ["SG DR", "SG DEC", "SG UC", "SG Agence", "SG Code Agence", "CRCM", "PSC"],
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Service DCO",
        "Value": "ANI SG",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
},
{
    "Key": "Service DCO - CDN",
    "Impacted": ["CDN BANQUE", "CDN REGION", "CDN GROUPE", "CDN AGENCES", "CDN Code AGENCES"],
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Service DCO",
        "Value": "ANI CDN",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
},
{
    "Key": "Service DCO - FOR",
    "Impacted": ["ASSU", "Type d'intervention", "Intervention IP", "Formation", "Type d'intervention 2"],
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Service DCO",
        "Value": "FOR",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
},
{
    "Key": "Laps - heure spécifique",
    "Impacted": ["Heure début", "Heure fin"],
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Laps",
        "Value": "Heure spécifique",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
}
]

$(function() {
    var elements = []

    var formatKey = function(key) {
        return key.replace(/ /ig, "_").replace(/\'/ig, "")
    }
    var getElement = function(key){
        key = formatKey(key)
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].Key == key) {
                return elements[i]
            }
            if(element[i].Value.constructor === Array)
            {
                var r = getSubElement(element[i].Key, key);
                if(r != null) return r  
            }
        }
        return null
    }

    var getSubElement = function(parentKey, key) {
        var parent = getElement(parentKey)
        var subelements = parent.Value[parent.Value.length - 1]
        for(var i = 0; i < subelements.length; ++i) {
            if(subelements[i].Key == key) {
                return subelements[i]
            }
        }
        return null
    }

    var processEffect = function(impacted, effect) {
        if(effect.Css != undefined && effect.Css != null) {
            for(var i = 0; i < impacted.length; ++i) {
                key = impacted[i];
                key = formatKey(key)
                var element = getElement(key)
                var block = $("#con_" + element.Key)
                for(var property in effect.Css) {
                    block.css(property, effect.Css[property])
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
         for(var i = 0; i < rules.length; ++i) {
            var rule = rules[i]
            var result = true
            for(var j = 0; j < rule.Conditions.length; ++j) {
                var condition = rule.Conditions[j]
                var element = getElement(condition.Key)
                if(element == null || element.Value != condition.Value) {
                    result = false
                    break
                }
            }
            if(result) {
                processEffect(rule.Impacted, rule.Effect)
                break
            }
            else if(rule.StartingEffect != undefined) {
                processEffect(rule.Impacted, rule.StartingEffect)
            }
        }
    }

    var copySummary = function(element) {
        $("[summary=" + element.Key + "]").html(element.Value)
    }

    $("[watch]").each(function(index, watchElement) {
        watchElement = $(watchElement)
        var key = watchElement.attr("watch")

        var parentId = null
        var element = null
        var parent = watchElement.parents("[parent-element]")
        if(parent.length > 0) {
            parentId = $(parent[0]).attr("parent-element")
        }
        else {
            element = {
                Key: key,
                Value: null
            }
            elements.push(element);
        }

        if(watchElement.is("input") || watchElement.is("select")) {
            watchElement.change(function() {
                var e = getElement(watchElement.attr("watch"))
                e.Value = watchElement.val()
                processChange()
                copySummary(e)
            })
        }

        if(watchElement.is("[type=radio]")) {
            watchElement.change(function() {
                if(watchElement.attr("checked")) {
                    var e = getElement(watchElement.attr("watch"))
                    e.Value = watchElement.val()
                    processChange()
                    copySummary(e)
                }
            })
        }

        if(watchElement.is("[type=checkbox]")) {
            watchElement.change(function() {
                var e = getElement(watchElement.attr("watch"))
                e.Value = watchElement.attr("checked")
                processChange()
                copySummary(e)
            })
        }

        if(watchElement.is("[type=button]")) {
            element.Value = [[]]
            watchElement.click(function() {
                var e = getElement(watchElement.attr("watch"))
                var item = e.Value[element.Value.length - 1]
                e.Value.push([])
                var id = ""
                for(var i in item) {
                    id += item[i].Key + ": " + item[i].Value + ", "
                    if(i >= 2) break;
                }
                
                $("#con_" + e.Key + " [watch-list]").append("<li><div class='chip'>" + id + "</div></li>")

                $("#con_" + e.Key + " [parent-element] input[watch]").val("")

                processChange()
                copySummary(e)
            })
        }
    })

    initEffect()
    
    $("[validate-form]").click(function(){
        console.log(elements)
        $.ajax({
            type: "POST",
            url: "/Home/PushData",
            data: "data=" + JSON.stringify(convertToDictionary(elements)),
            success: function() {
                window.location = window.location.href
            }
        });
    })


    $('select').material_select();
    $('.collapsible').collapsible();
    $('.datepicker').pickadate({
    selectMonths: false, // Creates a dropdown to control month
    selectYears: 15, // Creates a dropdown of 15 years to control year,
    today: 'Today',
    clear: 'Clear',
    close: 'Ok',
    closeOnSelect: true // Close upon selecting a date,
    });

    $(".yuffieNextButton").click(function(e) {
        var parent = $($(e.target).parents("li"));
        var next = parent.next();
        if(next.is("li")) {
            next.find(".collapsible-header").click();
        }
        else {
            var id = parent.parents(".page").attr("id");
            $("li.tab a[href='#" + id + "']").parents("li").next().find("a").click()
        }
    })


    var convertToDictionary = function(array) {
        var dic = {}
        for(var i in array) {
            var e = array[i]
            if(e.Value != null && e.Value.constructor === Array) {
                dic[e.Key] = []
                for(var j in e.Value) {
                    dic[e.Key].push(convertToDictionary(e.Value[j]))
                }
            }
            else {
                dic[e.Key] = e.Value
            }
        }
        return dic;
    }
    exceptions();
})

var exceptions = function() {
    
}