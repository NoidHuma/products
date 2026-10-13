(function ($) {
    const originalNumber = $.validator.methods.number;
    const originalRange = $.validator.methods.range;

    $.validator.addMethod("number", function (value, element) {
        if (element.name !== "Price") {
            return originalNumber.call(this, value, element);
        }

        return this.optional(element)
            || /^-?\d+(,\d+)?$/.test(value.trim());
    });

    $.validator.addMethod("range", function (value, element, parameters) {
        if (element.name !== "Price") {
            return originalRange.call(this, value, element, parameters);
        }

        if (this.optional(element)) {
            return true;
        }

        const number = Number(value.trim().replace(",", "."));

        return Number.isFinite(number)
            && number >= parameters[0]
            && number <= parameters[1];
    });
})(jQuery);