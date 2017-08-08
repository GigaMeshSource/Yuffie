$(function() {
    var elements = []
    var getElement = function(key){
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].Key == key) {
                return elements[i]
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
                    inp.val("")
                }
            })  
        }
        if(watchElement.is("input") || watchElement.is("select")) {
            watchElement.change(function() {
                element.Value = watchElement.val()                
            })
        }

        if(watchElement.is("[type=radio]")) {
            watchElement.change(function() {
                if(watchElement.attr("checked")) {
                    element.Value = watchElement.val()
                }
            })
        }

        if(watchElement.is("[type=checkbox]")) {
            watchElement.change(function() {
                element.Value = watchElement.prop("checked")                
            })
        }

        elements.push(element);
    })
    
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