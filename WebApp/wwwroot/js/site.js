// Write your Javascript code.
(function($){
    var oldHtml = $.fn.html;
    $.fn.html = function()
    {
        var ret = oldHtml.apply(this, arguments);

        //trigger your event.
        this.trigger("change");

        return ret;
    };
})(jQuery);

$(function() {
    var elements = {}
    var getElement = function(key){
        for(var i = 0; i < elements.length; ++i) {
            if(elements[i].key == key) {
                return elements[i]
            }
        }
    }

    $("input[updateList]").each(function(index, updateListElement) {
        $(updateListElement).keyup(function(e) {
            if(e.keyCode == 13) { //enter
                var list = $(updateListElement).next("ul")
                list.append("<li>" + $(updateListElement).val() + "</li>")
                $(updateListElement).val("")
            }
        })  
    })

    $("[watch]").each(function(index, watchElement) {
        var key = watchElement.prop("watch")
        var element = {
            key: key,
            type:"Text",
            element: watchElement.id(),
            value: null
        }
        if(watchElement.is("ul")) {
             watchElement.on("change",function(){
                $("#" + element.element).each("li", function(i, o) {
                    $(o).innerHtml()
                })
             })
        }

        if(watchElement.is("[type=radio]")) {
            element.type = "Radio"
        }

        if(watchElement.is("[type=checkbox]")) {
            element.type = "Check"
        }

        if(watchElement.is("select")) {
            element.type = "List"
        }

        elements.push(element);
    })
    
    $("#yuffieValidate").click(function(){
        data = [];
        for(var i = 0; i < elements.length; ++i)
        {
            var element = elements[i]
            var value = "";
            if(element.type == "")
            data.push({element.key, value});
        }

        $.post("/Home/PushData", data)
    })
})