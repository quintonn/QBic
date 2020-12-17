(function (base64, $, undefined)
{
    base64.encode = function (str)
    {
        return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{2})/g, function (match, p1)
        {
            return String.fromCharCode('0x' + p1);
        }));
    }

    base64.decode = function (str)
    {
        return decodeURIComponent(Array.prototype.map.call(atob(str), function (c)
        {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
    }

}(window.base64 = window.base64 || {}, jQuery));

//exports.byteLength = byteLength
//exports.toByteArray = toByteArray
//exports.fromByteArray = fromByteArray