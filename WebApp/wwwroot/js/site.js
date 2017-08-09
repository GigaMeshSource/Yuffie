var rules = [ {
    "Key": "Co-animateur",
    "StartingEffect": {
        "Css": {
            "display": "none"
        }
    },
    "Conditions": [{
        "Key": "Type d'animation",
        "Value": "Co-animation",
    }],
    "Effect": {
        "Css": {
            "display": "block"
        }
    }
}]

$(function() {
    var elements = []
    var getElement = function(key){
        key = key.replace(/ /ig, "_")
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].Key == key) {
                return elements[i]
            }
        }
        return null
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
        var element = {
            Key: key,
            Value: null
        }
        if(watchElement.is("ul")) {
            element.Value = []
            var inp = $(watchElement.prev())
            inp.keyup(function(e) {
                if(e.keyCode == 13) { //enter
                    watchElement.append("<li>" + inp.val() + "</li>")
                    element.Value.push(inp.val())
                    processChange()
                    inp.val("")
                }
            })  
        }
        if(watchElement.is("input") || watchElement.is("select")) {
            watchElement.change(function() {
                element.Value = watchElement.val()
                processChange()
            })
        }

        if(watchElement.is("[type=radio]")) {
            watchElement.change(function() {
                if(watchElement.attr("checked")) {
                    element.Value = watchElement.val()
                    processChange()
                }
            })
        }

        if(watchElement.is("[type=checkbox]")) {
            watchElement.change(function() {
                element.Value = watchElement.prop("checked")
                processChange()
            })
        }

        elements.push(element);
    })

    initEffect()
    
    $("#yuffieValidate").click(function(){
        console.log(elements)
        $.ajax({
            type: "POST",
            url: "/Home/PushData",
            data: "data=" + JSON.stringify(elements),
            success: function() {
                window.location = window.location.href
            }
        });
    })

 
})