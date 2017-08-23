var rules = [ {
    "Key": "Co-animateur",
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Type animation",
        "Value": "Co-animation",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
}//,
// {
//     "Key": "Heure début",
//     "StartingEffect": {
//         "Css": {
//             "display": "none"
//         }
//     },
//     "Conditions": [{
//         "Key": "Laps",
//         "Value": "h spécifique",
//     }],
//     "Effect": {
//         "Css": {
//             "display": "block"
//         }
//     }
// },
// {
//     "Key": "Heure fin",
//     "StartingEffect": {
//         "Css": {
//             "display": "none"
//         }
//     },
//     "Conditions": [{
//         "Key": "Laps",
//         "Value": "h spécifique",
//     }],
//     "Effect": {
//         "Css": {
//             "display": "block"
//         }
//     }
// }
]

$(function() {
    var elements = []
    var getElement = function(key){
        key = key.replace(/ /ig, "_")
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].Key == key) {
                return elements[i]
            }
            else {
                var r = getSubElement(elements[i].Key, key)
                if(r != null)
                    return r
            }
        }
        return null
    }

    var getSubElement = function(parentKey, key) {
        var parent = getElement(parentKey)
        var elements = parent.Value[parent.Value.length - 1]
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].Key == key) {
                return elements[i]
            }
        }
        var add = {
            Key: key,
            Value: null
        }
        elements.push(add)
        return add
    }

    var getRule = function(key){
        for(var i = 0; i < rules.length; ++i) {
            if(rules[i].Key == key) {
                return rules[i]
            }
        }
        return null
    }

    var processEffect = function(key, effect) {
        if(effect.Css != undefined && effect.Css != null) {
            var element = getElement(key)
            var block = $("#con_" + element.Key)
            for(var property in effect.Css) {
                block.css(property, effect.Css[property])
            }
        }
    }

    var initEffect = function() {
        for(var i = 0; i < rules.length; ++i) {
            var rule = rules[i]
            if(rule.StartingEffect != undefined && rule.StartingEffect != null) {
                processEffect(rule.Key, rule.StartingEffect)
            }
        }
    }

    var processChange = function() {
         for(var i = 0; i < rules.length; ++i) {
            var rule = rules[i]
            var result = true
            for(var i = 0; i < rule.Conditions.length; ++i) {
                var condition = rule.Conditions[i]
                var element = getElement(condition.Key)
                if(element == null || element.Value != condition.Value) {
                    result = false
                    break
                }
            }
            if(result) {
                processEffect(rule.Key, rule.Effect)
            }
            else if(rule.StartingEffect != undefined) {
                processEffect(rule.Key, rule.StartingEffect)
            }
        }
    }

    $("[watch]").each(function(index, watchElement) {
        watchElement = $(watchElement)
        var key = watchElement.attr("watch")

        var parentId = null
        var element = null
        var parentElement = null
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
                var e = getElement(element.Key)
                e.Value = watchElement.val()
                processChange()
            })
        }

        if(watchElement.is("[type=radio]")) {
            watchElement.change(function() {
                if(watchElement.attr("checked")) {
                    var e = getElement(element.Key)
                    e.Value = watchElement.val()
                    processChange()
                }
            })
        }

        if(watchElement.is("[type=checkbox]")) {
            watchElement.change(function() {
                var e = getElement(element.Key)
                e.Value = watchElement.attr("checked")
                processChange()
            })
        }

        if(watchElement.is("[type=button]")) {
            element.Value = [[]]
            watchElement.click(function() {
                var item = element.Value[element.Value.length - 1]
                element.Value.push([])
                var id = ""
                for(var i in item) {
                    id += item[i].Key + ": " + item[i].Value + ", "
                }
                $("#con_" + element.Key + " ul").append("<li>" + id + "</li>")

                $("#con_" + element.Key + " [parent-element] input[watch]").val("")

                processChange()
            })
        }
    })

    initEffect()
    
    $("#yuffieValidate").click(function(){
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
    selectMonths: true, // Creates a dropdown to control month
    selectYears: 15, // Creates a dropdown of 15 years to control year,
    today: 'Today',
    clear: 'Clear',
    close: 'Ok',
    closeOnSelect: false // Close upon selecting a date,
    });

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