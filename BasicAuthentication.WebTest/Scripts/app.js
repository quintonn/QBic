//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

var accessToken;
var refreshToken;
var timer;

$(document).ready(function ()
{
    checkIfLoggedIn();
});

function checkIfLoggedIn()
{
    $.get("api/v1/initializeSystem", function ()
    {
        $('#loginDiv').css('display', 'none');
        $('#contentDiv').css('display', 'block');
    })
    .fail(function (a, b, c)
    {
        if (a.status == 401)
        {
            $('#loginDiv').css('display', 'block');
            $('#contentDiv').css('display', 'none');
        }
        else
        {
            alert('error');
        }
    });
}


function login()
{
    var data = "grant_type=password&username=" + 'Steve' + "&password=" + 'password' + "&client_id=" + 'BasicAuthTest';
    $.post('http://localhost/BasicAuthTest/api/v1/token', data, function (a,b,c)
    {
        //console.log(a);
        console.log(a.access_token);
        console.log(a.refresh_token);

        accessToken = a.access_token;
        refreshToken = a.refresh_token;

        startTimer();

        $('#loginDiv').css('display', 'none');
        $('#contentDiv').css('display', 'block');
    })
    .fail(function (a, b, c)
    {
        $('#loginDiv').css('display', 'block');
        $('#contentDiv').css('display', 'none');
        alert('login error');
    });
}

function startTimer()
{
    if (timer == null)
    {
        timer = setInterval(pingTest, 10000);
    }
}

function pingTest()
{
    $.ajax(
        {
            url: "api/v1/pingTest",
            beforeSend: function (xhr)
            {
                xhr.setRequestHeader("Authorization", "Bearer " + accessToken);
            },
        }).done(function()
        {
        console.log('ping successfull');
    })
    .fail(function (a, b, c)
    {
        clearInterval(timer);
        console.log('ping error');
        console.log(a);
        console.log(b);
        console.log(c);
        tryRefreshToken();
    });
}

function tryRefreshToken()
{
    console.log('trying refresh token');
    //var data = "grant_type=refresh_token&refresh_token=" + refreshToken + "&client_id=" + 'myClientId';
    var data =
        {
            "grant_type": "refresh_token",
            "refresh_token": refreshToken,
            "client_id": "BasicAuthTest"
        };
    
    $.ajax(
        {
            url: "http://localhost/BasicAuthTest/api/v1/token",
            method: "POST",
            headers: {
                "Authorization": accessToken
            },
            data: data
        }).done(function (resp)
        {
            console.log('refresh token successfull');
            refreshToken = resp.refresh_token;
            accessToken = resp.access_token;
        })
    .fail(function (a, b, c)
    {
        console.log('refresh error');
        console.log(a);
        console.log(b);
        console.log(c);
    });
}
